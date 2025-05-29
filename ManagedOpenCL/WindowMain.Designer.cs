namespace ManagedOpenCL
{
    partial class WindowMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.listBox_log = new ListBox();
			this.comboBox_devices = new ComboBox();
			this.comboBox_kernelName = new ComboBox();
			this.button_load = new Button();
			this.button_info = new Button();
			this.comboBox_kernelVersion = new ComboBox();
			this.panel_kernelArguments = new Panel();
			this.checkBox_kernelInvariables = new CheckBox();
			this.listBox_pointers = new ListBox();
			this.button_execute = new Button();
			this.listBox_audios = new ListBox();
			this.listBox_images = new ListBox();
			this.button_exportAudio = new Button();
			this.button_importImage = new Button();
			this.button_exportImage = new Button();
			this.button_importAudio = new Button();
			this.label_info_audios = new Label();
			this.label_info_images = new Label();
			this.panel_view = new Panel();
			this.pictureBox_view = new PictureBox();
			this.label_info_pointers = new Label();
			this.button_moveAudio = new Button();
			this.button_moveImage = new Button();
			this.pictureBox_waveform = new PictureBox();
			this.vScrollBar_volume = new VScrollBar();
			this.hScrollBar_offset = new HScrollBar();
			this.button_playback = new Button();
			this.textBox_timestamp = new TextBox();
			this.label_meta = new Label();
			this.numericUpDown_zoom = new NumericUpDown();
			this.label_info_zoom = new Label();
			this.numericUpDown_samplesPerPixel = new NumericUpDown();
			this.numericUpDown_chunkSize = new NumericUpDown();
			this.numericUpDown_overlap = new NumericUpDown();
			this.label_info_chunkSize = new Label();
			this.label_info_overlap = new Label();
			this.button_fft = new Button();
			this.button_resetAudio = new Button();
			this.button_resetImage = new Button();
			this.button_normalize = new Button();
			this.button_test_stretch = new Button();
			this.numericUpDown_test_stretchFactor = new NumericUpDown();
			this.panel_view.SuspendLayout();
			((System.ComponentModel.ISupportInitialize) this.pictureBox_view).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.pictureBox_waveform).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_zoom).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_samplesPerPixel).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_chunkSize).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_overlap).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_test_stretchFactor).BeginInit();
			this.SuspendLayout();
			// 
			// listBox_log
			// 
			this.listBox_log.FormattingEnabled = true;
			this.listBox_log.ItemHeight = 15;
			this.listBox_log.Location = new Point(12, 640);
			this.listBox_log.Name = "listBox_log";
			this.listBox_log.Size = new Size(1497, 169);
			this.listBox_log.TabIndex = 0;
			// 
			// comboBox_devices
			// 
			this.comboBox_devices.FormattingEnabled = true;
			this.comboBox_devices.Location = new Point(12, 12);
			this.comboBox_devices.Name = "comboBox_devices";
			this.comboBox_devices.Size = new Size(691, 23);
			this.comboBox_devices.TabIndex = 1;
			// 
			// comboBox_kernelName
			// 
			this.comboBox_kernelName.FormattingEnabled = true;
			this.comboBox_kernelName.Location = new Point(1515, 12);
			this.comboBox_kernelName.Name = "comboBox_kernelName";
			this.comboBox_kernelName.Size = new Size(265, 23);
			this.comboBox_kernelName.TabIndex = 3;
			this.comboBox_kernelName.SelectedIndexChanged += this.comboBox_kernelName_SelectedIndexChanged;
			// 
			// button_load
			// 
			this.button_load.Location = new Point(1842, 12);
			this.button_load.Name = "button_load";
			this.button_load.Size = new Size(50, 23);
			this.button_load.TabIndex = 4;
			this.button_load.Text = "Load";
			this.button_load.UseVisualStyleBackColor = true;
			this.button_load.Click += this.button_load_Click;
			// 
			// button_info
			// 
			this.button_info.Location = new Point(709, 12);
			this.button_info.Name = "button_info";
			this.button_info.Size = new Size(23, 23);
			this.button_info.TabIndex = 5;
			this.button_info.Text = "i";
			this.button_info.UseVisualStyleBackColor = true;
			this.button_info.Click += this.button_info_Click;
			// 
			// comboBox_kernelVersion
			// 
			this.comboBox_kernelVersion.FormattingEnabled = true;
			this.comboBox_kernelVersion.Location = new Point(1786, 12);
			this.comboBox_kernelVersion.Name = "comboBox_kernelVersion";
			this.comboBox_kernelVersion.Size = new Size(50, 23);
			this.comboBox_kernelVersion.TabIndex = 6;
			// 
			// panel_kernelArguments
			// 
			this.panel_kernelArguments.BackColor = Color.Gainsboro;
			this.panel_kernelArguments.Location = new Point(1515, 41);
			this.panel_kernelArguments.Name = "panel_kernelArguments";
			this.panel_kernelArguments.Size = new Size(265, 300);
			this.panel_kernelArguments.TabIndex = 7;
			// 
			// checkBox_kernelInvariables
			// 
			this.checkBox_kernelInvariables.AutoSize = true;
			this.checkBox_kernelInvariables.Location = new Point(1786, 41);
			this.checkBox_kernelInvariables.Name = "checkBox_kernelInvariables";
			this.checkBox_kernelInvariables.Size = new Size(114, 19);
			this.checkBox_kernelInvariables.TabIndex = 8;
			this.checkBox_kernelInvariables.Text = "Show invariables";
			this.checkBox_kernelInvariables.UseVisualStyleBackColor = true;
			this.checkBox_kernelInvariables.CheckedChanged += this.checkBox_kernelInvariables_CheckedChanged;
			// 
			// listBox_pointers
			// 
			this.listBox_pointers.FormattingEnabled = true;
			this.listBox_pointers.ItemHeight = 15;
			this.listBox_pointers.Location = new Point(1515, 347);
			this.listBox_pointers.Name = "listBox_pointers";
			this.listBox_pointers.Size = new Size(265, 154);
			this.listBox_pointers.TabIndex = 9;
			// 
			// button_execute
			// 
			this.button_execute.Location = new Point(1786, 318);
			this.button_execute.Name = "button_execute";
			this.button_execute.Size = new Size(106, 23);
			this.button_execute.TabIndex = 10;
			this.button_execute.Text = "EXECUTE";
			this.button_execute.UseVisualStyleBackColor = true;
			this.button_execute.Click += this.button_execute_Click;
			// 
			// listBox_audios
			// 
			this.listBox_audios.FormattingEnabled = true;
			this.listBox_audios.ItemHeight = 15;
			this.listBox_audios.Location = new Point(1515, 640);
			this.listBox_audios.Name = "listBox_audios";
			this.listBox_audios.Size = new Size(180, 154);
			this.listBox_audios.TabIndex = 11;
			// 
			// listBox_images
			// 
			this.listBox_images.FormattingEnabled = true;
			this.listBox_images.ItemHeight = 15;
			this.listBox_images.Location = new Point(1712, 640);
			this.listBox_images.Name = "listBox_images";
			this.listBox_images.Size = new Size(180, 154);
			this.listBox_images.TabIndex = 12;
			// 
			// button_exportAudio
			// 
			this.button_exportAudio.Location = new Point(1640, 611);
			this.button_exportAudio.Name = "button_exportAudio";
			this.button_exportAudio.Size = new Size(55, 23);
			this.button_exportAudio.TabIndex = 13;
			this.button_exportAudio.Text = "Export\r\n";
			this.button_exportAudio.UseVisualStyleBackColor = true;
			this.button_exportAudio.Click += this.button_exportAudio_Click;
			// 
			// button_importImage
			// 
			this.button_importImage.Location = new Point(1712, 611);
			this.button_importImage.Name = "button_importImage";
			this.button_importImage.Size = new Size(55, 23);
			this.button_importImage.TabIndex = 14;
			this.button_importImage.Text = "Import";
			this.button_importImage.UseVisualStyleBackColor = true;
			this.button_importImage.Click += this.button_importImage_Click;
			// 
			// button_exportImage
			// 
			this.button_exportImage.Location = new Point(1837, 611);
			this.button_exportImage.Name = "button_exportImage";
			this.button_exportImage.Size = new Size(55, 23);
			this.button_exportImage.TabIndex = 15;
			this.button_exportImage.Text = "Export";
			this.button_exportImage.UseVisualStyleBackColor = true;
			this.button_exportImage.Click += this.button_exportImage_Click;
			// 
			// button_importAudio
			// 
			this.button_importAudio.Location = new Point(1515, 611);
			this.button_importAudio.Name = "button_importAudio";
			this.button_importAudio.Size = new Size(55, 23);
			this.button_importAudio.TabIndex = 16;
			this.button_importAudio.Text = "Import";
			this.button_importAudio.UseVisualStyleBackColor = true;
			this.button_importAudio.Click += this.button_importAudio_Click;
			// 
			// label_info_audios
			// 
			this.label_info_audios.AutoSize = true;
			this.label_info_audios.Location = new Point(1515, 797);
			this.label_info_audios.Name = "label_info_audios";
			this.label_info_audios.Size = new Size(120, 15);
			this.label_info_audios.TabIndex = 17;
			this.label_info_audios.Text = "No audios loaded. (0)";
			// 
			// label_info_images
			// 
			this.label_info_images.AutoSize = true;
			this.label_info_images.Location = new Point(1712, 797);
			this.label_info_images.Name = "label_info_images";
			this.label_info_images.Size = new Size(123, 15);
			this.label_info_images.TabIndex = 18;
			this.label_info_images.Text = "No images loaded. (0)";
			// 
			// panel_view
			// 
			this.panel_view.BackColor = SystemColors.ActiveCaptionText;
			this.panel_view.BorderStyle = BorderStyle.FixedSingle;
			this.panel_view.Controls.Add(this.pictureBox_view);
			this.panel_view.Location = new Point(12, 41);
			this.panel_view.Name = "panel_view";
			this.panel_view.Size = new Size(720, 480);
			this.panel_view.TabIndex = 19;
			// 
			// pictureBox_view
			// 
			this.pictureBox_view.BackColor = SystemColors.Control;
			this.pictureBox_view.Location = new Point(3, 3);
			this.pictureBox_view.Name = "pictureBox_view";
			this.pictureBox_view.Size = new Size(712, 472);
			this.pictureBox_view.TabIndex = 0;
			this.pictureBox_view.TabStop = false;
			// 
			// label_info_pointers
			// 
			this.label_info_pointers.AutoSize = true;
			this.label_info_pointers.Location = new Point(1515, 504);
			this.label_info_pointers.Name = "label_info_pointers";
			this.label_info_pointers.Size = new Size(143, 15);
			this.label_info_pointers.TabIndex = 20;
			this.label_info_pointers.Text = "No pointers on device. (0)";
			// 
			// button_moveAudio
			// 
			this.button_moveAudio.Location = new Point(1576, 611);
			this.button_moveAudio.Name = "button_moveAudio";
			this.button_moveAudio.Size = new Size(58, 23);
			this.button_moveAudio.TabIndex = 21;
			this.button_moveAudio.Text = "Move";
			this.button_moveAudio.UseVisualStyleBackColor = true;
			this.button_moveAudio.Click += this.button_moveAudio_Click;
			// 
			// button_moveImage
			// 
			this.button_moveImage.Location = new Point(1773, 611);
			this.button_moveImage.Name = "button_moveImage";
			this.button_moveImage.Size = new Size(58, 23);
			this.button_moveImage.TabIndex = 22;
			this.button_moveImage.Text = "Move";
			this.button_moveImage.UseVisualStyleBackColor = true;
			this.button_moveImage.Click += this.button_moveImage_Click;
			// 
			// pictureBox_waveform
			// 
			this.pictureBox_waveform.BackColor = SystemColors.Control;
			this.pictureBox_waveform.BorderStyle = BorderStyle.FixedSingle;
			this.pictureBox_waveform.Location = new Point(12, 542);
			this.pictureBox_waveform.Name = "pictureBox_waveform";
			this.pictureBox_waveform.Size = new Size(720, 77);
			this.pictureBox_waveform.TabIndex = 23;
			this.pictureBox_waveform.TabStop = false;
			// 
			// vScrollBar_volume
			// 
			this.vScrollBar_volume.Location = new Point(735, 542);
			this.vScrollBar_volume.Name = "vScrollBar_volume";
			this.vScrollBar_volume.Size = new Size(17, 77);
			this.vScrollBar_volume.TabIndex = 24;
			// 
			// hScrollBar_offset
			// 
			this.hScrollBar_offset.Location = new Point(12, 622);
			this.hScrollBar_offset.Name = "hScrollBar_offset";
			this.hScrollBar_offset.Size = new Size(720, 15);
			this.hScrollBar_offset.TabIndex = 25;
			// 
			// button_playback
			// 
			this.button_playback.Location = new Point(755, 542);
			this.button_playback.Name = "button_playback";
			this.button_playback.Size = new Size(23, 23);
			this.button_playback.TabIndex = 26;
			this.button_playback.Text = ">\r\n";
			this.button_playback.UseVisualStyleBackColor = true;
			// 
			// textBox_timestamp
			// 
			this.textBox_timestamp.Location = new Point(755, 596);
			this.textBox_timestamp.Name = "textBox_timestamp";
			this.textBox_timestamp.PlaceholderText = "0:00:00.000";
			this.textBox_timestamp.ReadOnly = true;
			this.textBox_timestamp.Size = new Size(80, 23);
			this.textBox_timestamp.TabIndex = 27;
			// 
			// label_meta
			// 
			this.label_meta.AutoSize = true;
			this.label_meta.Location = new Point(12, 524);
			this.label_meta.Name = "label_meta";
			this.label_meta.Size = new Size(177, 15);
			this.label_meta.TabIndex = 28;
			this.label_meta.Text = "No image information available.";
			// 
			// numericUpDown_zoom
			// 
			this.numericUpDown_zoom.Location = new Point(738, 498);
			this.numericUpDown_zoom.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
			this.numericUpDown_zoom.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			this.numericUpDown_zoom.Name = "numericUpDown_zoom";
			this.numericUpDown_zoom.Size = new Size(97, 23);
			this.numericUpDown_zoom.TabIndex = 29;
			this.numericUpDown_zoom.Value = new decimal(new int[] { 100, 0, 0, 0 });
			// 
			// label_info_zoom
			// 
			this.label_info_zoom.AutoSize = true;
			this.label_info_zoom.Location = new Point(740, 480);
			this.label_info_zoom.Name = "label_info_zoom";
			this.label_info_zoom.Size = new Size(52, 15);
			this.label_info_zoom.TabIndex = 30;
			this.label_info_zoom.Text = "Zoom %";
			// 
			// numericUpDown_samplesPerPixel
			// 
			this.numericUpDown_samplesPerPixel.Location = new Point(755, 571);
			this.numericUpDown_samplesPerPixel.Maximum = new decimal(new int[] { 8192, 0, 0, 0 });
			this.numericUpDown_samplesPerPixel.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			this.numericUpDown_samplesPerPixel.Name = "numericUpDown_samplesPerPixel";
			this.numericUpDown_samplesPerPixel.Size = new Size(80, 23);
			this.numericUpDown_samplesPerPixel.TabIndex = 31;
			this.numericUpDown_samplesPerPixel.Value = new decimal(new int[] { 128, 0, 0, 0 });
			// 
			// numericUpDown_chunkSize
			// 
			this.numericUpDown_chunkSize.Location = new Point(1515, 553);
			this.numericUpDown_chunkSize.Maximum = new decimal(new int[] { 16384, 0, 0, 0 });
			this.numericUpDown_chunkSize.Minimum = new decimal(new int[] { 64, 0, 0, 0 });
			this.numericUpDown_chunkSize.Name = "numericUpDown_chunkSize";
			this.numericUpDown_chunkSize.Size = new Size(100, 23);
			this.numericUpDown_chunkSize.TabIndex = 33;
			this.numericUpDown_chunkSize.Value = new decimal(new int[] { 1024, 0, 0, 0 });
			// 
			// numericUpDown_overlap
			// 
			this.numericUpDown_overlap.DecimalPlaces = 2;
			this.numericUpDown_overlap.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
			this.numericUpDown_overlap.Location = new Point(1621, 553);
			this.numericUpDown_overlap.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
			this.numericUpDown_overlap.Name = "numericUpDown_overlap";
			this.numericUpDown_overlap.Size = new Size(74, 23);
			this.numericUpDown_overlap.TabIndex = 34;
			this.numericUpDown_overlap.Value = new decimal(new int[] { 5, 0, 0, 65536 });
			// 
			// label_info_chunkSize
			// 
			this.label_info_chunkSize.AutoSize = true;
			this.label_info_chunkSize.Location = new Point(1515, 535);
			this.label_info_chunkSize.Name = "label_info_chunkSize";
			this.label_info_chunkSize.Size = new Size(67, 15);
			this.label_info_chunkSize.TabIndex = 35;
			this.label_info_chunkSize.Text = "Chunk size:";
			// 
			// label_info_overlap
			// 
			this.label_info_overlap.AutoSize = true;
			this.label_info_overlap.Location = new Point(1621, 535);
			this.label_info_overlap.Name = "label_info_overlap";
			this.label_info_overlap.Size = new Size(51, 15);
			this.label_info_overlap.TabIndex = 36;
			this.label_info_overlap.Text = "Overlap:";
			// 
			// button_fft
			// 
			this.button_fft.Location = new Point(784, 542);
			this.button_fft.Name = "button_fft";
			this.button_fft.Size = new Size(51, 23);
			this.button_fft.TabIndex = 37;
			this.button_fft.Text = "(I)FFT";
			this.button_fft.UseVisualStyleBackColor = true;
			this.button_fft.Click += this.button_fft_Click;
			// 
			// button_resetAudio
			// 
			this.button_resetAudio.Location = new Point(1515, 582);
			this.button_resetAudio.Name = "button_resetAudio";
			this.button_resetAudio.Size = new Size(55, 23);
			this.button_resetAudio.TabIndex = 38;
			this.button_resetAudio.Text = "Reset";
			this.button_resetAudio.UseVisualStyleBackColor = true;
			this.button_resetAudio.Click += this.button_resetAudio_Click;
			// 
			// button_resetImage
			// 
			this.button_resetImage.Location = new Point(1712, 582);
			this.button_resetImage.Name = "button_resetImage";
			this.button_resetImage.Size = new Size(55, 23);
			this.button_resetImage.TabIndex = 39;
			this.button_resetImage.Text = "Reset";
			this.button_resetImage.UseVisualStyleBackColor = true;
			this.button_resetImage.Click += this.button_resetImage_Click;
			// 
			// button_normalize
			// 
			this.button_normalize.Location = new Point(1576, 582);
			this.button_normalize.Name = "button_normalize";
			this.button_normalize.Size = new Size(58, 23);
			this.button_normalize.TabIndex = 40;
			this.button_normalize.Text = "Normal.";
			this.button_normalize.UseVisualStyleBackColor = true;
			this.button_normalize.Click += this.button_normalize_Click;
			// 
			// button_test_stretch
			// 
			this.button_test_stretch.Location = new Point(905, 494);
			this.button_test_stretch.Name = "button_test_stretch";
			this.button_test_stretch.Size = new Size(75, 23);
			this.button_test_stretch.TabIndex = 41;
			this.button_test_stretch.Text = "Stretch";
			this.button_test_stretch.UseVisualStyleBackColor = true;
			this.button_test_stretch.Click += this.button_test_stretch_Click;
			// 
			// numericUpDown_test_stretchFactor
			// 
			this.numericUpDown_test_stretchFactor.DecimalPlaces = 5;
			this.numericUpDown_test_stretchFactor.Increment = new decimal(new int[] { 5, 0, 0, 196608 });
			this.numericUpDown_test_stretchFactor.Location = new Point(986, 494);
			this.numericUpDown_test_stretchFactor.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
			this.numericUpDown_test_stretchFactor.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
			this.numericUpDown_test_stretchFactor.Name = "numericUpDown_test_stretchFactor";
			this.numericUpDown_test_stretchFactor.Size = new Size(150, 23);
			this.numericUpDown_test_stretchFactor.TabIndex = 42;
			this.numericUpDown_test_stretchFactor.Value = new decimal(new int[] { 1, 0, 0, 0 });
			// 
			// WindowMain
			// 
			this.AutoScaleDimensions = new SizeF(7F, 15F);
			this.AutoScaleMode = AutoScaleMode.Font;
			this.ClientSize = new Size(1904, 821);
			this.Controls.Add(this.numericUpDown_test_stretchFactor);
			this.Controls.Add(this.button_test_stretch);
			this.Controls.Add(this.button_normalize);
			this.Controls.Add(this.button_resetImage);
			this.Controls.Add(this.button_resetAudio);
			this.Controls.Add(this.button_fft);
			this.Controls.Add(this.label_info_overlap);
			this.Controls.Add(this.label_info_chunkSize);
			this.Controls.Add(this.numericUpDown_overlap);
			this.Controls.Add(this.numericUpDown_chunkSize);
			this.Controls.Add(this.numericUpDown_samplesPerPixel);
			this.Controls.Add(this.label_info_zoom);
			this.Controls.Add(this.numericUpDown_zoom);
			this.Controls.Add(this.label_meta);
			this.Controls.Add(this.textBox_timestamp);
			this.Controls.Add(this.button_playback);
			this.Controls.Add(this.hScrollBar_offset);
			this.Controls.Add(this.vScrollBar_volume);
			this.Controls.Add(this.pictureBox_waveform);
			this.Controls.Add(this.button_moveImage);
			this.Controls.Add(this.button_moveAudio);
			this.Controls.Add(this.label_info_pointers);
			this.Controls.Add(this.panel_view);
			this.Controls.Add(this.label_info_images);
			this.Controls.Add(this.label_info_audios);
			this.Controls.Add(this.button_importAudio);
			this.Controls.Add(this.button_exportImage);
			this.Controls.Add(this.button_importImage);
			this.Controls.Add(this.button_exportAudio);
			this.Controls.Add(this.listBox_images);
			this.Controls.Add(this.listBox_audios);
			this.Controls.Add(this.button_execute);
			this.Controls.Add(this.listBox_pointers);
			this.Controls.Add(this.checkBox_kernelInvariables);
			this.Controls.Add(this.panel_kernelArguments);
			this.Controls.Add(this.comboBox_kernelVersion);
			this.Controls.Add(this.button_info);
			this.Controls.Add(this.button_load);
			this.Controls.Add(this.comboBox_kernelName);
			this.Controls.Add(this.comboBox_devices);
			this.Controls.Add(this.listBox_log);
			this.MaximumSize = new Size(1920, 860);
			this.MinimumSize = new Size(1920, 860);
			this.Name = "WindowMain";
			this.Text = "ManagedOpenCL";
			this.panel_view.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize) this.pictureBox_view).EndInit();
			((System.ComponentModel.ISupportInitialize) this.pictureBox_waveform).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_zoom).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_samplesPerPixel).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_chunkSize).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_overlap).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_test_stretchFactor).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ListBox listBox_log;
		private ComboBox comboBox_devices;
		private ComboBox comboBox_kernelName;
		private Button button_load;
		private Button button_info;
		private ComboBox comboBox_kernelVersion;
		private Panel panel_kernelArguments;
		private CheckBox checkBox_kernelInvariables;
		private ListBox listBox_pointers;
		private Button button_execute;
		private ListBox listBox_audios;
		private ListBox listBox_images;
		private Button button_exportAudio;
		private Button button_importImage;
		private Button button_exportImage;
		private Button button_importAudio;
		private Label label_info_audios;
		private Label label_info_images;
		private Panel panel_view;
		private Label label_info_pointers;
		private Button button_moveAudio;
		private Button button_moveImage;
		private PictureBox pictureBox_view;
		private PictureBox pictureBox_waveform;
		private VScrollBar vScrollBar_volume;
		private HScrollBar hScrollBar_offset;
		private Button button_playback;
		private TextBox textBox_timestamp;
		private Label label_meta;
		private NumericUpDown numericUpDown_zoom;
		private Label label_info_zoom;
		private NumericUpDown numericUpDown_samplesPerPixel;
		private NumericUpDown numericUpDown_chunkSize;
		private NumericUpDown numericUpDown_overlap;
		private Label label_info_chunkSize;
		private Label label_info_overlap;
		private Button button_fft;
		private Button button_resetAudio;
		private Button button_resetImage;
		private Button button_normalize;
		private Button button_test_stretch;
		private NumericUpDown numericUpDown_test_stretchFactor;
	}
}
