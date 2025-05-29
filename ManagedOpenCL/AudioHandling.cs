using NAudio.Wave;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace ManagedOpenCL
{
	public class AudioHandling : IDisposable
	{
		// UI Controls
		private readonly ListBox _logList;
		private readonly ListBox _tracksList;
		private readonly PictureBox _wavePicture;
		private readonly Button _playButton;
		private readonly TextBox _timeText;
		private readonly Label _metaLabel;
		private readonly HScrollBar _offsetScroll;
		private readonly VScrollBar _volumeScroll;
		private readonly NumericUpDown _zoomNumeric;

		public Color GraphColor = Color.MediumSlateBlue;

		// Audio Data
		private readonly List<AudioObject> _tracks = new();
		private readonly SynchronizationContext _uiContext;
		private CancellationTokenSource _waveformCancellation = new();

		// Properties
		public string RepoPath { get; }
		public IReadOnlyList<AudioObject> Tracks => this._tracks.AsReadOnly();
		public AudioObject? CurrentTrack
		{
			get
			{
				if (this._tracksList.InvokeRequired)
				{
					return (AudioObject?) this._tracksList.Invoke(new Func<AudioObject?>(() => this.GetCurrentTrackInternal()));
				}
				else
				{
					return this.GetCurrentTrackInternal();
				}
			}
		}

		private AudioObject? GetCurrentTrackInternal()
		{
			int index = this._tracksList.SelectedIndex;
			return index >= 0 && index < this._tracks.Count ? this._tracks[index] : null;
		}

		private System.Windows.Forms.Timer _playbackTimer;

		public AudioHandling(string root, ListBox? listBoxLog = null, ListBox? listBoxTracks = null,
							PictureBox? pictureBoxWave = null, Button? buttonPlay = null,
							TextBox? textBoxTime = null, Label? labelMeta = null,
							HScrollBar? hScrollBarOffset = null, VScrollBar? vScrollBarVolume = null,
							NumericUpDown? numericUpDownZoom = null)
		{
			this.RepoPath = root;
			this._uiContext = SynchronizationContext.Current ?? throw new InvalidOperationException("Must be created on UI thread");

			// Initialize UI controls
			this._logList = listBoxLog ?? new ListBox();
			this._tracksList = listBoxTracks ?? new ListBox();
			this._wavePicture = pictureBoxWave ?? new PictureBox();
			this._playButton = buttonPlay ?? new Button();
			this._timeText = textBoxTime ?? new TextBox();
			this._metaLabel = labelMeta ?? new Label();
			this._offsetScroll = hScrollBarOffset ?? new HScrollBar();
			this._volumeScroll = vScrollBarVolume ?? new VScrollBar();
			this._zoomNumeric = numericUpDownZoom ?? new NumericUpDown();

			// Initialize playback timer
			this._playbackTimer = new System.Windows.Forms.Timer { Interval = 30 };
			this._playbackTimer.Tick += this.PlaybackTimer_Tick;

			this.SetupEventHandlers();

			// Load resources audio
			// this.LoadResourcesAudios();
		}

		public Image CurrentWaveform
		{
			get
			{
				if (this.CurrentTrack == null)
				{
					return new Bitmap(this._wavePicture.Width, this._wavePicture.Height);
				}

				if (this.CurrentTrack.Data.Length == 0 && this.CurrentTrack.ComplexData.Length != 0)
				{
					this.CurrentTrack.DrawComplexformParallel(this._wavePicture, (int) this._zoomNumeric.Value, (long)this._offsetScroll.Value, this.GraphColor);
				}

				return this.CurrentTrack.DrawWaveformParallel(
					this._wavePicture,
					(int) this._zoomNumeric.Value,
					(long) this._offsetScroll.Value,
					this.GraphColor
				);
			}
		}

		public void RefreshView()
		{
			// Aktualisiere die Waveform
			if (this.CurrentTrack != null)
			{
				this._metaLabel.Text = this.CurrentTrack.MetaString;
				this.RedrawWaveform();
				this.UpdateScrollbarSettings(); // ✅ Scrollbar aktualisieren
			}
			else
			{
				this.ClearTrackUI();
			}
		}

		private void SetupEventHandlers()
		{
			this._playButton.Click += (s, e) => this.TogglePlayback();

			this._wavePicture.Paint += this.WavePicture_Paint;
			this._tracksList.SelectedIndexChanged += (s, e) => this.UpdateUIForSelectedTrack();

			this._playButton.Click += (s, e) =>
			{
				if (this.CurrentTrack == null)
				{
					return;
				}
				if (this.CurrentTrack.Player.PlaybackState != PlaybackState.Playing)
				{
					this._playbackTimer.Start();
				}
				else
				{
					this._playbackTimer.Stop();
				}
			};

			this._offsetScroll.Scroll += (s, e) =>
			{
				if (this.CurrentTrack != null)
				{
					this.UpdateScrollbarSettings(); // ✅ Scrollbar aktualisieren
				}
			};

			this._zoomNumeric.ValueChanged += (s, e) =>
			{
				///this.UpdateScrollbarSettings();
				//this.RedrawWaveform();
			};

			this._volumeScroll.Scroll += (s, e) =>
			{
				if (this.CurrentTrack != null && e.Type == ScrollEventType.EndScroll)
				{
					float volume = (100 - this._volumeScroll.Value) / 100f;
					this.CurrentTrack.Player.Volume = volume; // Lautstärke aktualisieren
					this.UpdateScrollbarSettings(); // ✅ Lautstärkeänderung könnte Scrollbar beeinflussen
				}
			};

			this._wavePicture.MouseWheel += (s, e) =>
			{ 				
				if (this.CurrentTrack == null)
				{
					return;
				}
				int zoom = (int) this._zoomNumeric.Value;
				if (e.Delta > 0 && zoom < 8192)
				{
					this._zoomNumeric.Value = Math.Min(8192, zoom + 5);
				}
				else if (e.Delta < 0 && zoom > 1)
				{
					this._zoomNumeric.Value = Math.Max(1, zoom - 5);
				}
				//this.RedrawWaveform();
			};
		}

		private void UpdateScrollbarSettings()
		{
			if (this.CurrentTrack == null)
			{
				return;
			}

			int zoom = (int) this._zoomNumeric.Value;
			int samplesPerPixel = Math.Max(1, 128 / zoom);
			int visibleSamples = this._wavePicture.Width * samplesPerPixel;

			// Total track length in samples
			int totalSamples = (int) this.CurrentTrack.Length;

			// Calculate maximum scroll position
			int maxScroll = Math.Max(0, totalSamples - visibleSamples);

			this._offsetScroll.Minimum = 0;
			this._offsetScroll.Maximum = maxScroll;
			this._offsetScroll.LargeChange = visibleSamples / 10;
			this._offsetScroll.SmallChange = visibleSamples / 10;

			// Adjust current position if it's beyond new maximum
			if (this._offsetScroll.Value > maxScroll)
			{
				this._offsetScroll.Value = maxScroll;
			}
		}

		private void PlaybackTimer_Tick(object? sender, EventArgs e)
		{
			if (this.CurrentTrack == null)
			{
				return;
			}

			var currentPos = this.CurrentTrack.Position;
			this._timeText.Text = TimeSpan.FromSeconds((double) currentPos).ToString(@"mm\:ss\.fff");

			this.UpdateScrollbarPosition((long) currentPos); // ✅ Scrollbar synchronisieren

			this._wavePicture.Invalidate();
		}

		private void WavePicture_Paint(object? sender, PaintEventArgs e)
		{
			if (this.CurrentTrack == null || this._wavePicture.Image == null)
			{
				return;
			}

			// Draw waveform
			e.Graphics.DrawImage(this._wavePicture.Image, Point.Empty);

			// Calculate visible range
			int zoom = (int) this._zoomNumeric.Value;
			int samplesPerPixel = Math.Max(1, 128 / zoom);
			int visibleStart = this._offsetScroll.Value;
			int visibleEnd = visibleStart + (this._wavePicture.Width * samplesPerPixel);
			decimal currentPos = this.CurrentTrack.Position;
		}

		public string? ImportAudioFile()
		{
			using OpenFileDialog ofd = new()
			{
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
				Filter = "Audio Files|*.mp3;*.wav;*.flac;*.aac|All Files|*.*",
				Multiselect = false
			};

			if (ofd.ShowDialog() != DialogResult.OK)
			{
				return null;
			}

			try
			{
				this.AddTrackAsync(ofd.FileName);
				this.RefreshView(); // Aktualisiere die Ansicht nach dem Hinzufügen
				return ofd.FileName;
			}
			catch (Exception ex)
			{
				this.Log($"Error importing file: {ex.Message}");
				return null;
			}

		}

		public AudioObject AddTrackAsync(string filePath)
		{
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("Audio file not found", filePath);
			}

			AudioObject track = new AudioObject(filePath);

			this._tracks.Add(track);

			this._uiContext.Post(_ =>
			{
				this._tracksList.Items.Add(Path.GetFileName(filePath));
				if (this._tracksList.SelectedIndex == -1)
				{
					this._tracksList.SelectedIndex = 0;
				}
			}, null);

			this.RedrawWaveform();

			return track;
		}

		public AudioObject CreateEmptyTrack(long length, int samplerate = 44100, int channels = 1, int bitdepth = 16, bool add = true)
		{
			float[] data = new float[length];
			Array.Fill(data, 0.0f);

			int number = this.Tracks.Count;

			AudioObject obj = new(data, samplerate, channels, bitdepth, number);


			if (add)
			{
				this._tracks.Add(obj);
				this._tracksList.Items.Add(obj.Name);
			}

			this.RedrawWaveform();

			return obj;
		}

		public void LoadResourcesAudios()
		{
			// Get all ,mp3 and .wav files in the resources directory
			string[] audioFiles = Directory.GetFiles(Path.Combine(this.RepoPath, "Resources"), "*.*", SearchOption.AllDirectories)
				.Where(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
				.ToArray();
			if (audioFiles.Length == 0)
			{
				this.Log("No audio files found in Resources/Audios directory.");
				return;
			}

			foreach (string file in audioFiles)
			{
				try
				{
					this.AddTrackAsync(file);
				}
				catch (Exception ex)
				{
					this.Log($"Error loading audio file '{file}': {ex.Message}");
				}
			}
		}

		public void RemoveCurrentTrack()
		{
			if (this.CurrentTrack == null)
			{
				return;
			}

			int index = this._tracksList.SelectedIndex;
			this._tracks[index].Dispose();
			this._tracks.RemoveAt(index);

			this._uiContext.Post(_ =>
			{
				this._tracksList.Items.RemoveAt(index);
				if (this._tracksList.Items.Count > 0)
				{
					this._tracksList.SelectedIndex = Math.Min(index, this._tracksList.Items.Count - 1);
				}
				else
				{
					this.ClearTrackUI();
				}
			}, null);
		}

		private void UpdateScrollbarPosition(long currentPos)
		{
			// currentPos in Einzel-Samples → Umrechnen in Frames
			int frames = (int) (currentPos / this.CurrentTrack?.Channels ?? 2);

			int zoom = (int) this._zoomNumeric.Value;
			int samplesPerPixel = Math.Max(1, 128 / zoom);
			if (samplesPerPixel <= 0)
			{
				samplesPerPixel = 1;
			}

			int totalFrames = (int) (this.CurrentTrack?.Length ?? 0 / this.CurrentTrack?.Channels ?? 2);
			int pictureBoxWidth = this._wavePicture.Width;

			int maxScrollValue = Math.Max(0, (totalFrames / samplesPerPixel) - pictureBoxWidth);
			int scrollValue = frames / samplesPerPixel;

			if (scrollValue < 0)
			{
				scrollValue = 0;
			}

			if (scrollValue > maxScrollValue)
			{
				scrollValue = maxScrollValue;
			}

			if (this._offsetScroll.Value != scrollValue)
			{
				this._offsetScroll.Maximum = maxScrollValue;
				this._offsetScroll.Value = scrollValue;
			}
		}



		private void UpdateUIForSelectedTrack()
		{
			if (this.CurrentTrack == null)
			{
				this.ClearTrackUI();
				return;
			}

			this._metaLabel.Text = this.CurrentTrack.MetaString;
			this._volumeScroll.Value = 50;

		    this.UpdateScrollbarSettings();
			this.RedrawWaveform();
		}


		

		private void RedrawWaveform()
		{
			if (this.CurrentTrack == null)
			{
				return;
			}

			this._waveformCancellation.Cancel();
			this._waveformCancellation = new CancellationTokenSource();

			try
			{
				int zoom = (int) this._zoomNumeric.Value;
				int samplesPerPixel = Math.Max(1, 128 / zoom);

				Image? waveform = this.CurrentTrack.DrawWaveformParallel(
					this._wavePicture,
					samplesPerPixel,
					this._offsetScroll.Value,
					this.GraphColor
					);

				this._uiContext.Post(_ =>
				{
					var oldImage = this._wavePicture.Image;
					this._wavePicture.Image = waveform;
					oldImage?.Dispose();
					this._wavePicture.Invalidate();
				}, null);
			}
			catch (OperationCanceledException)
			{
				// Expected during cancellation
			}
			catch (Exception ex)
			{
				this.Log($"Waveform error: {ex.Message}");
			}
		}

		private void OnPlaybackStateChanged(object? sender, PlaybackState state)
		{
			this._uiContext.Post(_ =>
			{
				this._playButton.Text = state == PlaybackState.Playing ? "Stop" : "Play";
				if (state == PlaybackState.Stopped)
				{
					this._offsetScroll.Value = 0;
					this._playbackTimer.Stop();
				}
			}, null);
		}

		private void OnPositionChanged(object? sender, decimal position)
		{
			this._uiContext.Post(_ =>
			{
				this._timeText.Text = TimeSpan.FromSeconds((double) position).ToString(@"mm\:ss\.fff");
				this._wavePicture.Invalidate();
			}, null);
		}

		private void ClearTrackUI()
		{
			this._uiContext.Post(_ =>
			{
				this._wavePicture.Image?.Dispose();
				this._wavePicture.Image = null;
				this._playButton.Text = "Play";
				this._timeText.Text = "00:00.000";
				this._metaLabel.Text = "No track selected";
				this._offsetScroll.Value = 0;
			}, null);
		}

		private void Log(string message)
		{
			this._uiContext.Post(_ =>
			{
				this._logList.Items.Add($"[Audio]:      {message}");
				this._logList.TopIndex = this._logList.Items.Count - 1;
			}, null);
		}

		private void UpdatePlaybackPosition()
		{
			while (this.CurrentTrack?.Player.PlaybackState == PlaybackState.Playing && !this._waveformCancellation.IsCancellationRequested)
			{
				if (this.CurrentTrack == null)
				{
					break;
				}

				double currentTime = this.CurrentTrack.CurrentTime;
				int currentSample = (int) (currentTime * this.CurrentTrack.Samplerate);

				this.InvokeIfRequired(() =>
				{
					// Update TimeText
					this._timeText.Text = currentTime.ToString("0.00");

					// Update Scroll Position
					int visibleRange = this._wavePicture.Width * (int) this._zoomNumeric.Value;
					int targetScrollPos = currentSample - (visibleRange / 2);

					if (targetScrollPos != this._offsetScroll.Value)
					{
						this._offsetScroll.Value = Math.Max(0, Math.Min(targetScrollPos, this._offsetScroll.Maximum));
					}

					// Force waveform redraw
					this._wavePicture.Image = this.CurrentWaveform;
				});

				Thread.Sleep(30); // Faster updates for smoother movement
			}
		}

		private void InvokeIfRequired(Action action)
		{
			if (this._playButton.InvokeRequired)
			{
				this._playButton.Invoke(action);
			}
			else
			{
				action();
			}
		}

		private void TogglePlayback()
		{
			if (this.CurrentTrack == null || this.CurrentTrack.Data.LongLength == 0)
			{
				return;
			}

			if (this.CurrentTrack.Player.PlaybackState == PlaybackState.Playing)
			{
				this._waveformCancellation.Cancel();
				this.CurrentTrack.Stop();
				this.UpdateButtonState(false);
			}
			else
			{
				this._waveformCancellation = new CancellationTokenSource();

				// Lautstärke aus VolumeScroll lesen (invertiert: 100 → 0.0f, 0 → 1.0f)
				float initialVolume = (100 - this._volumeScroll.Value) / 100f;

				// Play mit initialer Lautstärke aufrufen
				this.CurrentTrack.Play(this._waveformCancellation.Token, () =>
				{
					this.UpdateButtonState(false);
				}, initialVolume);

				this.UpdateButtonState(true);
				Task.Run(this.UpdatePlaybackPosition, this._waveformCancellation.Token);
			}
		}

		private void UpdateButtonState(bool playing)
		{
			if (this._playButton.InvokeRequired)
			{
				this._playButton.Invoke((MethodInvoker) (() => {
					this._playButton.Text = playing ? "■" : "▶";
				}));
			}
			else
			{
				this._playButton.Text = playing ? "■" : "▶";
			}
		}


		public void Dispose()
		{
			this._playbackTimer.Stop();
			this._playbackTimer.Dispose();
			this._waveformCancellation.Cancel();

			foreach (var track in this._tracks)
			{
				track.Dispose();
			}

			this._wavePicture.Image?.Dispose();
			GC.SuppressFinalize(this);
		}
	}


	public class AudioObject : IDisposable
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		public string Filepath { get; set; }
		public string Name { get; set; }
		public float[] Data { get; set; } = [];
		public Vector2[] ComplexData { get; set; } = [];
		public int Samplerate { get; set; } = -1;
		public int Bitdepth { get; set; } = -1;
		public int Channels { get; set; } = -1;
		public long Length { get; set; } = -1;

		public IntPtr Pointer { get; set; } = 0;
		public int ChunkSize { get; set; } = 0;
		public int OverlapSize { get; set; } = 0;
		public char Form { get; set; } = 'f';

		public string MetaString => this.GetMetaString();

		public WaveOutEvent Player { get; set; } = new WaveOutEvent();

		// ----- ----- ----- PROPERTIES ----- ----- ----- \\
		public long Position
		{
			get
			{
				return this.Player == null || this.Player.PlaybackState != PlaybackState.Playing
					? 0
					: this.Player.GetPosition() / (this.Channels * (this.Bitdepth / 8));
			}
		}

		public double CurrentTime
		{
			get
			{
				return this.Samplerate <= 0 ? 0 : (double) this.Position / this.Samplerate;
			}
		}


		public bool OnHost => (this.Data.Length > 0 || this.ComplexData.Length > 0) && this.Pointer == 0;
		public bool OnDevice => (this.Data.Length == 0 && this.ComplexData.Length == 0) && this.Pointer != 0;


		// ----- ----- ----- CONSTRUCTOR ----- ----- ----- \\
		public AudioObject(string filepath)
		{
			this.Filepath = filepath;
			this.Name = Path.GetFileNameWithoutExtension(filepath);
			this.LoadAudioFile();
		}

		public AudioObject(float[] data, int samplerate = 44100, int channels = 1, int bitdepth = 16, int number = 0)
		{
			this.Data = data;
			this.Name = "Empty_" + number.ToString("000");
			this.Filepath = "No file supplied";
			this.Samplerate = samplerate;
			this.Channels = channels;
			this.Bitdepth = bitdepth;
			this.Length = data.LongLength;
		}



		public void LoadAudioFile()
		{
			if (string.IsNullOrEmpty(this.Filepath))
			{
				throw new FileNotFoundException("File path is empty");
			}

			using AudioFileReader reader = new(this.Filepath);
			this.Samplerate = reader.WaveFormat.SampleRate;
			this.Bitdepth = reader.WaveFormat.BitsPerSample;
			this.Channels = reader.WaveFormat.Channels;
			this.Length = reader.Length; // Length in bytes

			// Calculate number of samples
			long numSamples = reader.Length / (reader.WaveFormat.BitsPerSample / 8);
			this.Data = new float[numSamples];

			int read = reader.Read(this.Data, 0, (int) numSamples);
			if (read != numSamples)
			{
				float[] resizedData = new float[read];
				Array.Copy(this.Data, resizedData, read);
				this.Data = resizedData;
			}
		}

		public byte[] GetBytes()
		{
			int bytesPerSample = this.Bitdepth / 8;
			byte[] bytes = new byte[this.Data.Length * bytesPerSample];

			Parallel.For(0, this.Data.Length, i =>
			{
				switch (this.Bitdepth)
				{
					case 8:
						bytes[i] = (byte) (this.Data[i] * 127);
						break;
					case 16:
						short sample16 = (short) (this.Data[i] * short.MaxValue);
						Buffer.BlockCopy(BitConverter.GetBytes(sample16), 0, bytes, i * bytesPerSample, bytesPerSample);
						break;
					case 24:
						int sample24 = (int) (this.Data[i] * 8388607);
						Buffer.BlockCopy(BitConverter.GetBytes(sample24), 0, bytes, i * bytesPerSample, 3);
						break;
					case 32:
						Buffer.BlockCopy(BitConverter.GetBytes(this.Data[i]), 0, bytes, i * bytesPerSample, bytesPerSample);
						break;
				}
			});

			return bytes;
		}

		public List<float[]> GetChunksOld(int size = 2048, float overlap = 0.5f)
		{
			if (this.Data == null || this.Data.Length == 0)
			{
				return [];
			}

			if (size <= 0 || overlap < 0 || overlap >= 1)
			{
				return [];
			}

			this.ChunkSize = size;
			this.OverlapSize = (int) (size * overlap);
			int step = size - this.OverlapSize;
			int numChunks = (this.Data.Length - size) / step + 1;

			List<float[]> chunks = new List<float[]>(numChunks);

			for (int i = 0; i < numChunks; i++)
			{
				float[] chunk = new float[size];
				int sourceOffset = i * step;
				Array.Copy(this.Data, sourceOffset, chunk, 0, size);
				chunks.Add(chunk);
			}

			return chunks;
		}

		public List<float[]> GetChunks(int size = 2048, float overlap = 0.5f)
		{
			if (this.Data == null || this.Data.Length == 0)
			{
				return [];
			}

			if (size <= 0 || overlap < 0 || overlap >= 1)
			{
				return [];
			}

			this.ChunkSize = size;
			this.OverlapSize = (int) (size * overlap);
			int step = size - this.OverlapSize;
			int numChunks = (this.Data.Length - size) / step + 1;

			// Parallel vorbereiten
			float[][] chunks = new float[numChunks][];

			Parallel.For(0, numChunks, i =>
			{
				int sourceOffset = i * step;
				float[] chunk = new float[size];
				Array.Copy(this.Data, sourceOffset, chunk, 0, size);
				chunks[i] = chunk;
			});

			// In List umwandeln
			return chunks.ToList();
		}

		public List<Vector2[]> GetCompexChunks(int size = 2048, float overlap = 0.5f)
		{
			if (this.ComplexData == null || this.ComplexData.Length == 0)
			{
				return [];
			}

			if (size <= 0 || overlap < 0 || overlap >= 1)
			{
				return [];
			}

			this.ChunkSize = size;
			this.OverlapSize = (int) (size * overlap);
			int step = size - this.OverlapSize;
			int numChunks = (this.ComplexData.Length - size) / step + 1;
			
			List<Vector2[]> chunks = new List<Vector2[]>(numChunks);
			for (int i = 0; i < numChunks; i++)
			{
				Vector2[] chunk = new Vector2[size];
				int sourceOffset = i * step;
				Array.Copy(this.ComplexData, sourceOffset, chunk, 0, size);
				chunks.Add(chunk);
			}

			return chunks;
		}


		public void AggregateChunks(List<float[]> chunks, bool nullPointer = true)
		{
			if (chunks == null || chunks.Count == 0)
			{
				return;
			}

			int size = this.ChunkSize;
			int step = size - this.OverlapSize;
			int outputLength = (chunks.Count - 1) * step + size;

			// Zielpuffer vorbereiten
			float[] output = new float[outputLength];
			float[] weightSum = new float[outputLength];

			// Parallel lokal puffern, dann atomar addieren
			int processorCount = Environment.ProcessorCount;

			Parallel.For(0, chunks.Count, () =>
				(new float[outputLength], new float[outputLength]),

				(i, state, local) =>
				{
					(Single[] localData, Single[] localWeight) = local;
					float[] chunk = chunks[i];
					int offset = i * step;

					for (int j = 0; j < Math.Min(size, chunk.Length); j++)
					{
						int idx = offset + j;
						localData[idx] += chunk[j];
						localWeight[idx] += 1f;
					}
					return (localData, localWeight);
				},

				local =>
				{
					(Single[] localData, Single[] localWeight) = local;
					lock (output) // kurzes Lock für zusammenführung
					{
						for (int i = 0; i < outputLength; i++)
						{
							output[i] += localData[i];
							weightSum[i] += localWeight[i];
						}
					}
				}
			);

			// Normalisierung
			Parallel.For(0, outputLength, i =>
			{
				if (weightSum[i] > 0f)
				{
					output[i] /= weightSum[i];
				}
			});

			// Setze das Ergebnis
			this.Data = output;

			// Setze die Länge basierend auf der Anzahl der Samples
			this.Length = output.LongLength;
			this.Pointer = IntPtr.Zero; // Setze Pointer auf 0, da wir auf dem Host sind
		}

		public void AggregateComplexes(List<Vector2[]> complexChunks)
		{
			this.ComplexData = new Vector2[complexChunks.Count * complexChunks[0].Length];
			int index = 0;
			foreach (var chunk in complexChunks)
			{
				foreach (var value in chunk)
				{
					this.ComplexData[index++] = value;
				}
			}
		}

		public void Play(CancellationToken cancellationToken, Action? onPlaybackStopped = null, float initialVolume = 1.0f)
		{
			if (this.Data == null || this.Data.Length == 0)
			{
				return;
			}

			this.Player = new WaveOutEvent
			{
				Volume = initialVolume
			};

			byte[] bytes = this.GetBytes();
			WaveFormat waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(this.Samplerate, this.Channels);
			RawSourceWaveStream stream = new(new MemoryStream(bytes), waveFormat);

			this.Player.PlaybackStopped += (s, e) =>
			{
				onPlaybackStopped?.Invoke();
				stream.Dispose();
				this.Player.Dispose();
			};

			this.Player.Init(stream);
			this.Player.Play();

			// Überprüfe regelmäßig auf Abbruch
			Task.Run(() =>
			{
				while (this.Player.PlaybackState == PlaybackState.Playing)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						this.Player.Stop();
						break;
					}
					Thread.Sleep(100);
				}
			}, cancellationToken);
		}

		public void Stop()
		{
			this.Player.Stop();
		}

		public double GetCurrentTime()
		{
			if (this.Player.PlaybackState != PlaybackState.Playing)
			{
				return 0;
			}

			// Korrekte Berechnung der aktuellen Zeit
			return (double) this.Player.GetPosition() /
				   (this.Samplerate * this.Channels * (this.Bitdepth / 8));
		}

		public Bitmap DrawWaveformParallelOld(PictureBox pictureBox, int samplesPerPixel = 1024, long offset = 0, Color? graphColor = null)
		{
			graphColor ??= Color.BlueViolet;
			Bitmap bitmap = new(pictureBox.Width, pictureBox.Height);

			using (Graphics graphics = Graphics.FromImage(bitmap))
			using (Pen wavePen = new(graphColor.Value, 1f))
			{
				graphics.Clear(pictureBox.BackColor);

				if (this.Data == null || this.Data.Length == 0)
				{
					return bitmap;
				}

				int width = pictureBox.Width;
				int height = pictureBox.Height;
				float[] data = this.Data;
				int channels = this.Channels;

				// Draw waveform
				Parallel.For(0, width, x =>
				{
					long sampleIndex = offset + (x * samplesPerPixel);
					if (sampleIndex * channels >= data.Length - channels)
					{
						return;
					}

					if (channels == 2)
					{
						float left = data[sampleIndex * 2];
						float right = data[sampleIndex * 2 + 1];

						int leftY = (int) (height * 0.25f - left * height * 0.2f);
						int rightY = (int) (height * 0.75f - right * height * 0.2f);

						lock (graphics)
						{
							graphics.DrawLine(wavePen, x, height * 0.25f, x, leftY);
							graphics.DrawLine(wavePen, x, height * 0.75f, x, rightY);
						}
					}
					else
					{
						float sample = data[sampleIndex];
						int y = (int) (height * 0.5f - sample * height * 0.4f);

						lock (graphics)
						{
							graphics.DrawLine(wavePen, x, height * 0.5f, x, y);
						}
					}
				});

				// Draw playhead if playing
				if (this.Player.PlaybackState == PlaybackState.Playing)
				{
					int playheadX = (int) ((this.Position - offset) / samplesPerPixel);

					if (playheadX >= 0 && playheadX < width)
					{
						using (Pen playheadPen = new(Color.Red, 3))
						{
							graphics.DrawLine(playheadPen, playheadX, 0, playheadX, height);
							graphics.FillRectangle(Brushes.Red, playheadX - 2, 0, 5, 10);
							graphics.FillRectangle(Brushes.Red, playheadX - 2, height - 10, 5, 10);
						}
					}
				}
			}

			return bitmap;
		}

		public Bitmap DrawWaveformParallel(PictureBox pictureBox, int samplesPerPixel = 1024, long offset = 0, Color? graphColor = null)
		{
			graphColor ??= Color.MediumSlateBlue;
			Bitmap bitmap = new(pictureBox.Width, pictureBox.Height);

			using Graphics g = Graphics.FromImage(bitmap);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.Clear(pictureBox.BackColor);

			if (Data == null || Data.Length == 0)
			{
				return bitmap;
			}

			int width = bitmap.Width;
			int height = bitmap.Height;
			int midY = height / 2;
			int halfH = height / 2;

			float[] data = this.Data;
			int channels = this.Channels;

			// Füllfarben
			Color fillColor = Color.FromArgb(100, graphColor.Value);
			Color borderColor = graphColor.Value;

			GraphicsPath pathLeft = new();
			GraphicsPath pathRight = new();

			PointF[] upperLeft = new PointF[width];
			PointF[] upperRight = new PointF[width];

			Parallel.For(0, width, x =>
			{
				long sampleIndex = offset + x * samplesPerPixel;
				if (sampleIndex * channels >= data.Length - channels)
				{
					return;
				}

				float left = (channels >= 1) ? data[sampleIndex * channels] : 0f;
				float right = (channels >= 2) ? data[sampleIndex * channels + 1] : left;

				float yL = halfH / 2 - left * halfH * 0.9f;
				float yR = 3 * halfH / 2 - right * halfH * 0.9f;

				upperLeft[x] = new PointF(x, yL);
				upperRight[x] = new PointF(x, yR);
			});

			// Linker Kanal (oben)
			pathLeft.AddLines(upperLeft);
			pathLeft.AddLine(width - 1, halfH / 2, 0, halfH / 2);
			pathLeft.CloseFigure();

			// Rechter Kanal (unten)
			pathRight.AddLines(upperRight);
			pathRight.AddLine(width - 1, 3 * halfH / 2, 0, 3 * halfH / 2);
			pathRight.CloseFigure();

			// Draw filled shapes
			using Brush fillBrush = new SolidBrush(fillColor);
			g.FillPath(fillBrush, pathLeft);
			if (channels == 2)
			{
				g.FillPath(fillBrush, pathRight);
			}

			// Draw border line (waveform outline)
			using Pen borderPen = new(borderColor, 1.2f);
			g.DrawLines(borderPen, upperLeft);
			if (channels == 2)
			{
				g.DrawLines(borderPen, upperRight);
			}

			// Draw center lines
			using Pen axisPen = new(Color.FromArgb(40, 0, 0, 0), 1);
			g.DrawLine(axisPen, 0, halfH / 2, width, halfH / 2);         // top center
			if (channels == 2)
			{
				g.DrawLine(axisPen, 0, 3 * halfH / 2, width, 3 * halfH / 2); // bottom center
			}

			// Draw playhead
			if (this.Player?.PlaybackState == PlaybackState.Playing)
			{
				int playheadX = (int) ((this.Position - offset) / samplesPerPixel);
				if (playheadX >= 0 && playheadX < width)
				{
					using Pen playheadPen = new(Color.Red, 2);
					g.DrawLine(playheadPen, playheadX, 0, playheadX, height);
					g.FillEllipse(Brushes.Red, playheadX - 3, 2, 6, 6);
					g.FillEllipse(Brushes.Red, playheadX - 3, height - 8, 6, 6);
				}
			}

			return bitmap;
		}


		public Bitmap DrawComplexformParallel(PictureBox pictureBox, int samplesPerPixel = 1024, long offset = 0, Color? graphColor = null)
		{
			graphColor ??= Color.MediumSlateBlue;
			Bitmap bitmap = new(pictureBox.Width, pictureBox.Height);
			using Graphics g = Graphics.FromImage(bitmap);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.Clear(pictureBox.BackColor);
			if (this.ComplexData == null || this.ComplexData.Length == 0)
			{
				return bitmap;
			}

			int width = bitmap.Width;
			int height = bitmap.Height;
			int midY = height / 2;
			int halfH = height / 2;
			Vector2[] data = this.ComplexData;
			// Füllfarben
			Color fillColor = Color.FromArgb(100, graphColor.Value);
			Color borderColor = graphColor.Value;
			GraphicsPath pathLeft = new();
			GraphicsPath pathRight = new();
			PointF[] upperLeft = new PointF[width];
			PointF[] upperRight = new PointF[width];
			Parallel.For(0, width, x =>
			{
				long sampleIndex = offset + x * samplesPerPixel;
				if (sampleIndex * 2 >= data.Length - 1)
				{
					return;
				}

				Vector2 leftVec = data[sampleIndex * 2];
				Vector2 rightVec = data[sampleIndex * 2 + 1];
				float leftMagnitude = leftVec.X;
				float rightMagnitude = rightVec.Y;
				float yL = halfH / 2 - leftMagnitude * halfH * 0.9f;
				float yR = 3 * halfH / 2 - rightMagnitude * halfH * 0.9f;
				upperLeft[x] = new PointF(x, yL);
				upperRight[x] = new PointF(x, yR);
			});
			// Linker Kanal (oben)
			pathLeft.AddLines(upperLeft);
			pathLeft.AddLine(width - 1, halfH / 2, 0, halfH / 2);
			pathLeft.CloseFigure();
			// Rechter Kanal (unten)
			pathRight.AddLines(upperRight);
			pathRight.AddLine(width - 1, 3 * halfH / 2, 0, 3 * halfH / 2);
			pathRight.CloseFigure();
			// Draw filled shapes
			using Brush fillBrush = new SolidBrush(fillColor);
			g.FillPath(fillBrush, pathLeft);
			if (this.Channels == 2)
			{
				g.FillPath(fillBrush, pathRight);
			}

			// Draw border line (waveform outline)
			using Pen borderPen = new(borderColor, 1.2f);
			g.DrawLines(borderPen, upperLeft);
			if (this.Channels == 2)
			{
				g.DrawLines(borderPen, upperRight);
			}

			// Draw center lines
			using Pen axisPen = new(Color.FromArgb(40, 0, 0, 0), 1);
			g.DrawLine(axisPen, 0, halfH / 2, width, halfH / 2);         // top center
			if (this.Channels == 2)
			{
				g.DrawLine(axisPen, 0, 3 * halfH / 2, width, 3 * halfH / 2); // bottom center
			}

			// Draw playhead
			if (this.Player?.PlaybackState == PlaybackState.Playing)
			{
				int playheadX = (int) ((this.Position - offset) / samplesPerPixel);
				if (playheadX >= 0 && playheadX < width)
				{
					using Pen playheadPen = new(Color.Red, 2);
					g.DrawLine(playheadPen, playheadX, 0, playheadX, height);
					g.FillEllipse(Brushes.Red, playheadX - 3, 2, 6, 6);
					g.FillEllipse(Brushes.Red, playheadX - 3, height - 8, 6, 6);
				}
			}

			return bitmap;
		}


		public string? Export()
		{
			using SaveFileDialog sfd = new();
			sfd.Title = "Export audio file";
			sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
			sfd.Filter = "Wave files (*.wav)|*.wav|MP3 files (*.mp3)|*.mp3";
			sfd.OverwritePrompt = true;

			if (sfd.ShowDialog() == DialogResult.OK)
			{
				byte[] bytes = this.GetBytes();
				WaveFormat waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(this.Samplerate, this.Channels);

				using (RawSourceWaveStream stream = new(new MemoryStream(bytes), waveFormat))
				using (FileStream fileStream = new(sfd.FileName, FileMode.Create))
				{
					WaveFileWriter.WriteWavFileToStream(fileStream, stream);
				}

				return sfd.FileName;
			}
			return null;
		}

		public void Reload()
		{
			// Null pointer
			this.Pointer = 0;
			this.Form = 'f';
			this.ChunkSize = 0;
			this.OverlapSize = 0;

			this.LoadAudioFile();
		}

		public void Normalize(float maxAmplitude = 1.0f)
		{
			if (this.Data == null || this.Data.Length == 0)
			{
				return;
			}

			// Schritt 1: Maximalwert (Betrag) ermitteln – parallel
			float globalMax = 0f;
			object lockObj = new object();

			Parallel.For(0, this.Data.Length, () => 0f, (i, _, localMax) =>
			{
				float abs = Math.Abs(this.Data[i]);
				return abs > localMax ? abs : localMax;
			},
			localMax =>
			{
				lock (lockObj)
				{
					if (localMax > globalMax)
					{
						globalMax = localMax;
					}
				}
			});

			// Kein Normalisieren nötig, wenn max = 0
			if (globalMax == 0f)
			{
				return;
			}

			// Schritt 2: Daten parallel skalieren
			float scale = maxAmplitude / globalMax;

			Parallel.For(0, this.Data.Length, i =>
			{
				this.Data[i] *= scale;
			});
		}

		public void Dispose()
		{
			this.Player?.Dispose();
			this.Data = [];
			this.Pointer = 0;
			GC.SuppressFinalize(this);
		}

		public string GetMetaString()
		{
			return $"Name: {this.Name}, Samplerate: {this.Samplerate}, Bitdepth: {this.Bitdepth}, Channels: {this.Channels}, Length: {this.Length / 3 / 1024:N0} KBytes, <{this.Pointer.ToString("X16")}>";
		}

	}

}