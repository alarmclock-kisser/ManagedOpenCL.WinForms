using System.Windows.Forms.VisualStyles;

namespace ManagedOpenCL
{
	public partial class WindowMain : Form
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		public string Repopath => Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));

		public OpenClService Service;

		public ImageHandling IMGH;
		public AudioHandling AUDH;





		// ----- ----- ----- LAMBDA ----- ----- ----- \\
		public bool Initialized => this.Service.MemorRegister != null &&
								   this.Service.KernelHandling != null &&
								   this.Service.INDEX != -1 &&
								   this.Service.PLAT != null &&
								   this.Service.DEV != null &&
								   this.Service.CTX != null;



		// ----- ----- ----- CONSTRUCTORS ----- ----- ----- \\
		public WindowMain()
		{
			this.InitializeComponent();

			// Initialize the OpenCL service
			this.Service = new OpenClService(this.Repopath, this.listBox_log, this.comboBox_devices);

			// Register events
			this.listBox_log.DoubleClick += (sender, e) => this.CopyLogLineToClipboard(this.listBox_log.SelectedIndex);

			// Select Intel device
			this.Service.SelectDeviceLike("Intel");

			// Fill kernels combo box
			this.Service.KernelHandling?.FillGenericKernelNamesCombobox(this.comboBox_kernelName);

			// Initialize Image and Audio handling
			this.IMGH = new ImageHandling(this.Repopath, this.listBox_images, this.pictureBox_view, this.numericUpDown_zoom, this.label_meta);
			this.AUDH = new AudioHandling(this.Repopath, this.listBox_log, this.listBox_audios, this.pictureBox_waveform, this.button_playback, this.textBox_timestamp, this.label_meta, this.hScrollBar_offset, this.vScrollBar_volume, this.numericUpDown_samplesPerPixel);

			// Load resources
			this.IMGH.LoadResourcesImages();
			this.AUDH.LoadResourcesAudios();
		}





		// ----- ----- ----- METHODS ----- ----- ----- \\
		public void CopyLogLineToClipboard(int index = -1)
		{
			if (index < 0)
			{
				// If no index is provided, use the selected index
				index = this.listBox_log.SelectedIndex;
			}

			// Check if index is valid
			if (index < 0 || index >= this.listBox_log.Items.Count)
			{
				MessageBox.Show("Invalid log line index.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Get the log line and copy it to clipboard
			string logLine = this.listBox_log.Items[index].ToString() ?? string.Empty;
			Clipboard.SetText(logLine);
			MessageBox.Show($"{logLine}", "Log line copied to clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		public void MoveAudio(int index = -1)
		{
			// Check initialized
			if (!this.Initialized || this.Service.MemorRegister == null || this.Service.KernelHandling == null)
			{
				MessageBox.Show("OpenCL service is not initialized. Please select a device first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (index < 0)
			{
				// If no index is provided, use the selected index
				index = this.listBox_audios.SelectedIndex;
			}
			// Check if index is valid
			if (index < 0 || index >= this.listBox_audios.Items.Count)
			{
				MessageBox.Show("Invalid audio track index.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Get the audio track and move it
			AudioObject track = this.AUDH.Tracks[index];

			// Move chunks Host <-> Device
			if (track.OnHost)
			{
				int chunkSize = (int) this.numericUpDown_chunkSize.Value;
				float overlap = (float) this.numericUpDown_overlap.Value;

				List<float[]> chunks = track.GetChunks(chunkSize, overlap);
				if (chunks.Count == 0)
				{
					MessageBox.Show("No chunks available to push.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				// Move chunks to device
				track.Pointer = this.Service.MemorRegister.PushChunks<float>(chunks)?.IndexHandle ?? IntPtr.Zero;
				if (track.Pointer == IntPtr.Zero)
				{
					MessageBox.Show("Failed to move audio chunks to device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				else
				{
					track.Data = [];
					this.Service.MemorRegister?.Log("Audio chunks moved to device", track.Pointer.ToString("X16"), 1);
				}
			}
			else if (track.OnDevice)
			{
				List<float[]> chunks = this.Service.MemorRegister.PullChunks<float>(track.Pointer);
				if (chunks.Count == 0)
				{
					MessageBox.Show("No chunks available to pull.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				// Aggregate chunks in obj
				track.AggregateChunks(chunks);
				if (track.Data.Length == 0)
				{
					MessageBox.Show("Failed to pull audio chunks from device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				else
				{
					// Clear device pointer after pulling to host
					this.Service.MemorRegister?.FreeBuffer(track.Pointer);
					track.Pointer = IntPtr.Zero;
					this.Service.MemorRegister?.Log("Audio chunks moved to host", chunks.Count.ToString("N0"), 1);
				}
			}
			else
			{
				MessageBox.Show("Audio track is neither on host nor on device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			this.AUDH.RefreshView();
		}

		public void MoveImage(int index = -1)
		{
			// Check initialized
			if (!this.Initialized || this.Service.MemorRegister == null || this.Service.KernelHandling == null)
			{
				MessageBox.Show("OpenCL service is not initialized. Please select a device first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			if (index < 0)
			{
				// If no index is provided, use the selected index
				index = this.listBox_images.SelectedIndex;
			}

			// Check if index is valid
			if (index < 0 || index >= this.listBox_images.Items.Count)
			{
				MessageBox.Show("Invalid image index.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Get the image object and move it
			ImageObject imgObj = this.IMGH.Images[index];

			// Move image Host <-> Device
			if (imgObj.OnHost)
			{
				byte[] pixels = imgObj.GetPixelsAsBytes(true);
				if (pixels.Length == 0)
				{
					MessageBox.Show("No pixels available to push.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				// Move image to device
				imgObj.Pointer = this.Service.MemorRegister.PushData(pixels)?.IndexHandle ?? IntPtr.Zero;
				if (imgObj.Pointer == 0)
				{
					MessageBox.Show("Failed to move image to device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				else
				{
					imgObj.Img = null; // Clear host image after moving to device
					this.Service.MemorRegister?.Log("Image moved to device", imgObj.Pointer.ToString("X16"), 1);
				}
			}
			else if (imgObj.OnDevice)
			{
				byte[] pixels = this.Service.MemorRegister.PullData<byte>(imgObj.Pointer);
				if (pixels.Length == 0)
				{
					MessageBox.Show("No pixels available to pull.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				// Create image from pulled pixels
				imgObj.SetImageFromBytes(pixels, true);
				if (imgObj.Img == null)
				{
					MessageBox.Show("Failed to pull image from device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				else
				{
					// Clear device pointer after pulling to host
					this.Service.MemorRegister?.FreeBuffer(imgObj.Pointer);
					imgObj.Pointer = 0; // Clear device pointer after pulling to host
					this.Service.MemorRegister?.Log("Image moved to host", imgObj.Width + "x" + imgObj.Height, 1);
				}
			}
			else
			{
				MessageBox.Show("Image object is neither on host nor on device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Refresh image view
			this.IMGH.FillImagesListBox();
		}

		public void PerformFFT(int index = -1)
		{
			// Check initialized
			if (!this.Initialized || this.Service.MemorRegister == null || this.Service.KernelHandling == null)
			{
				MessageBox.Show("OpenCL service is not initialized. Please select a device first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			if (index < 0)
			{
				// If no index is provided, use the selected index
				index = this.listBox_audios.SelectedIndex;
			}

			// Check if index is valid
			if (index < 0 || index >= this.listBox_audios.Items.Count)
			{
				MessageBox.Show("Invalid audio track index.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Get the audio track and perform FFT
			AudioObject track = this.AUDH.Tracks[index];
			if (!track.OnDevice || track.Pointer == IntPtr.Zero)
			{
				// Optionally move audio to device
				this.MoveAudio(index);

				if (track.OnHost)
				{
					MessageBox.Show("Audio track is not on device or has no pointer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}

			// Execute FFT on the track
			IntPtr fftPointer = this.Service.KernelHandling.ExecuteFFT(track.Pointer, track.Form, track.ChunkSize, track.OverlapSize);
			if (fftPointer == IntPtr.Zero)
			{
				MessageBox.Show("Failed to perform FFT on audio track.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			else
			{
				// Update the track with the FFT result
				track.Pointer = fftPointer;
				track.Form = track.Form == 'f' ? 'c' : 'f';
			}

			this.AUDH.RefreshView();
		}

		public void ExecuteKernel(int index = -1, string kernelVersion = "00", string kernelName = "kernel")
		{
			// Check initialized
			if (!this.Initialized || this.Service.MemorRegister == null || this.Service.KernelHandling == null)
			{
				MessageBox.Show("OpenCL service is not initialized. Please select a device first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Check kernel name and version
			if (string.IsNullOrEmpty(kernelName) || string.IsNullOrEmpty(kernelVersion))
			{
				MessageBox.Show("Please select a kernel name and version.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			this.Service.KernelHandling.LoadKernel(kernelName + kernelVersion, "", this.panel_kernelArguments, this.checkBox_kernelInvariables.Checked);
			if (this.Service.KernelHandling.Kernel == null || this.Service.KernelHandling.KernelFile == null)
			{
				MessageBox.Show("Failed to load kernel.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			else if (this.Service.KernelHandling.KernelFile.Contains("\\Audio\\"))
			{
				// Verify index
				if (index < 0)
				{
					// If no index is provided, use the selected index
					index = this.listBox_audios.SelectedIndex;
				}

				// Check if index is valid
				if (index < 0 || index >= this.listBox_audios.Items.Count)
				{
					MessageBox.Show("Invalid audio track index.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				// Get the audio track
				AudioObject track = this.AUDH.Tracks[index];

				// Optionally move audio to device
				bool moved = false;
				if (track.OnHost)
				{
					this.MoveAudio(index);
					moved = true;
				}

				// Optionally check if input is factor
				float factor = 1.0f;
				NumericUpDown? factorNumeric = this.panel_kernelArguments.Controls.Find("argInput_factor", true).FirstOrDefault() as NumericUpDown;
				if (factorNumeric != null)
				{
					factor = (float) factorNumeric.Value;
					this.Service.KernelHandling.Log($"Factor for kernel {kernelName}: {factor}", "", 1);
				}

				// Check pointer
				if (track.Pointer == IntPtr.Zero)
				{
					MessageBox.Show("Audio track is not on device or has no pointer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				// Execute the kernel on the audio track
				track.Pointer = this.Service.KernelHandling.ExecKernelAudio(track, kernelName, kernelVersion, track.ChunkSize, (float)(track.ChunkSize / track.OverlapSize), factor, null, true);
				if (track.Pointer == IntPtr.Zero)
				{
					MessageBox.Show("Failed to execute kernel on audio track.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				// Optionally move back to host
				if (moved)
				{
					this.MoveAudio(index);
				}

				// Refresh audio view
				this.AUDH.RefreshView();
			}
			else if (this.Service.KernelHandling.KernelFile.Contains("\\Imaging\\"))
			{
				// Verify index
				if (index < 0)
				{
					// If no index is provided, use the selected index
					index = this.listBox_images.SelectedIndex;
				}

				// Check if index is valid
				if (index < 0 || index >= this.listBox_images.Items.Count)
				{
					MessageBox.Show("Invalid image index.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				// Get the image object
				ImageObject imgObj = this.IMGH.Images[index];

				// Optionally move image to device
				bool moved = false;
				if (imgObj.OnHost)
				{
					this.MoveImage(index);
					moved = true;
				}

				// Check pointer
				if (imgObj.Pointer == IntPtr.Zero)
				{
					MessageBox.Show("Image object is not on device or has no pointer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				// Execute the kernel on the image object
				imgObj.Pointer = this.Service.KernelHandling.ExecKernelImage(imgObj, kernelName, kernelVersion, null, true);
				if (imgObj.Pointer == IntPtr.Zero)
				{
					MessageBox.Show("Failed to execute kernel on image object.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				// Optionally move back to host
				if (moved)
				{
					this.MoveImage(index);
				}

				// Refresh image view
				this.IMGH.FillImagesListBox();
			}
			else
			{
				MessageBox.Show("Kernel file does not match audio or image processing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}

		public void PerformStretch(int index = -1, string kernelVersion = "01", string kernelName = "stretch")
		{
			// Check initialized
			if (!this.Initialized || this.Service.MemorRegister == null || this.Service.KernelHandling == null)
			{
				MessageBox.Show("OpenCL service is not initialized. Please select a device first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Check kernel name and version
			if (string.IsNullOrEmpty(kernelName) || string.IsNullOrEmpty(kernelVersion))
			{
				MessageBox.Show("Please select a kernel name and version.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			
			// Verify index
			if (index < 0)
			{
				// If no index is provided, use the selected index
				index = this.listBox_audios.SelectedIndex;
			}
			
			// Check if index is valid
			if (index < 0 || index >= this.listBox_audios.Items.Count)
			{
				MessageBox.Show("Invalid audio track index.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			
			// Get the audio track
			AudioObject track = this.AUDH.Tracks[index];
			
			// Optionally move audio to device
			bool moved = false;
			if (track.OnHost)
			{
				this.MoveAudio(index);
				moved = true;
			}

			// Check pointer
			if (track.Pointer == IntPtr.Zero)
			{
				MessageBox.Show("Audio track is not on device or has no pointer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Perform FFT forward
			this.PerformFFT(index);
			if (track.Pointer == IntPtr.Zero || track.Form != 'c')
			{
				MessageBox.Show("Failed to perform FFT on audio track.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Get factor
			float factor = (float) this.numericUpDown_test_stretchFactor.Value;

			// Execute the kernel on the audio track
			track.Pointer = this.Service.KernelHandling.ExecuteKernelGenericAudioChunks(track, kernelVersion, kernelName, track.ChunkSize, factor, null, false);
			if (track.Pointer == IntPtr.Zero)
			{
				MessageBox.Show("Failed to execute kernel on audio track.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Perform IFFT inverse
			this.PerformFFT(index);
			if (track.Pointer == IntPtr.Zero || track.Form != 'f')
			{
				MessageBox.Show("Failed to perform IFFT on audio track.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Optionally move back to host
			if (moved)
			{
				this.MoveAudio(index);
			}

			// Refresh audio view
			this.AUDH.RefreshView();
		}


		// ----- ----- ----- EVENT HANDLERS ----- ----- ----- \\
		private void button_info_Click(object sender, EventArgs e)
		{
			// Check initialized
			if (!this.Initialized)
			{
				MessageBox.Show("OpenCL service is not initialized. Please select a device first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			List<string> info = [];

			// If CTRL down: Get PLatform info
			if (ModifierKeys.HasFlag(Keys.Control))
			{
				info = this.Service.GetInfoPlatformInfo();
			}
			else
			{
				// Get Device info
				info = this.Service.GetInfoDeviceInfo();
			}

			// Check empty
			if (info.Count == 0)
			{
				MessageBox.Show("No information available.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			// Show info in a message box
			string message = string.Join(Environment.NewLine, info);
			MessageBox.Show(message, "OpenCL-Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void comboBox_kernelName_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.Service.KernelHandling?.FillGenericKernelVersionsCombobox(this.comboBox_kernelVersion, this.comboBox_kernelName.SelectedItem?.ToString() ?? "", true);
		}

		private void button_load_Click(object sender, EventArgs e)
		{
			if (this.Service.KernelHandling == null)
			{
				MessageBox.Show("Kernel handling is not initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			string kernelName = this.comboBox_kernelName.SelectedItem?.ToString() ?? string.Empty;
			string kernelVersion = this.comboBox_kernelVersion.SelectedItem?.ToString() ?? string.Empty;
			if (string.IsNullOrEmpty(kernelName) || string.IsNullOrEmpty(kernelVersion))
			{
				MessageBox.Show("Please select a kernel name and version.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			this.Service.KernelHandling.LoadKernel(kernelName + kernelVersion, "", this.panel_kernelArguments, this.checkBox_kernelInvariables.Checked);

		}

		private void checkBox_kernelInvariables_CheckedChanged(object sender, EventArgs e)
		{
			this.Service.KernelHandling?.Dispose();

			this.button_load_Click(sender, e);
		}

		private void button_importAudio_Click(object sender, EventArgs e)
		{
			this.AUDH.ImportAudioFile();
		}

		private void button_exportAudio_Click(object sender, EventArgs e)
		{
			string? exported = this.AUDH.CurrentTrack?.Export();
			if (exported != null)
			{
				MessageBox.Show($"Audio exported to: {exported}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void button_importImage_Click(object sender, EventArgs e)
		{
			this.IMGH.ImportImage();
		}

		private void button_exportImage_Click(object sender, EventArgs e)
		{
			string? exported = this.IMGH.CurrentObject?.Export();
			if (exported != null)
			{
				MessageBox.Show($"Image exported to: {exported}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void button_moveAudio_Click(object sender, EventArgs e)
		{
			this.MoveAudio(this.listBox_audios.SelectedIndex);

			this.Service.FillPointers(this.listBox_pointers);
		}

		private void button_moveImage_Click(object sender, EventArgs e)
		{
			this.MoveImage(this.listBox_images.SelectedIndex);

			this.Service.FillPointers(this.listBox_pointers);
		}

		private void button_fft_Click(object sender, EventArgs e)
		{
			this.PerformFFT(this.listBox_audios.SelectedIndex);

			this.Service.FillPointers(this.listBox_pointers);
		}

		private void button_execute_Click(object sender, EventArgs e)
		{
			// Get current image
			ImageObject? obj = this.IMGH.CurrentObject;
			if (obj == null)
			{
				MessageBox.Show("No image currently selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			
			this.ExecuteKernel(this.listBox_audios.SelectedIndex, this.comboBox_kernelVersion.SelectedItem?.ToString() ?? "", this.comboBox_kernelName.SelectedItem?.ToString() ?? "");
			this.IMGH.FillImagesListBox();

			this.Service.FillPointers(this.listBox_pointers);
		}

		private void button_resetImage_Click(object sender, EventArgs e)
		{
			this.IMGH.CurrentObject?.ResetImage();

			this.IMGH.FillImagesListBox();
		}

		private void button_resetAudio_Click(object sender, EventArgs e)
		{
			this.AUDH.CurrentTrack?.Reload();

			this.AUDH.RefreshView();
		}

		private void button_normalize_Click(object sender, EventArgs e)
		{
			this.AUDH.CurrentTrack?.Normalize();

			this.AUDH.RefreshView();
		}

		private void button_test_stretch_Click(object sender, EventArgs e)
		{
			// this.PerformStretch(this.listBox_audios.SelectedIndex, "01", "stretch");

			AudioObject? obj = this.AUDH.CurrentTrack;
			if (obj == null)
			{
				MessageBox.Show("No audio track currently selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			float factor = (float) this.numericUpDown_test_stretchFactor.Value;
			int chunkSize = (int) this.numericUpDown_chunkSize.Value;
			float overlap = (float) this.numericUpDown_overlap.Value;

			this.Service.KernelHandling?.ExecKernelAudio(obj, this.comboBox_kernelName.SelectedItem?.ToString() ?? "stretch", this.comboBox_kernelVersion.SelectedItem?.ToString() ?? "01", chunkSize, overlap, factor, null, true);
		}
	}
}
