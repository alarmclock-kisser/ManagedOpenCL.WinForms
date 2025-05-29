using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ManagedOpenCL
{
    public class ImageHandling
    {
        // ----- ----- ATTRIBUTES ----- ----- \\
        public string Repopath;
        public ListBox ImagesList;
        public PictureBox ViewPBox;
        public NumericUpDown ZoomNumeric;
        public Label MetaLabel;



        public List<ImageObject> Images = [];



        private float zoomFactor = 1.0f;
        private Point mouseDownLocation;
        private bool isDragging = false;

        // ----- ----- LAMBDA ----- ----- \\
        public ImageObject? CurrentObject => this.ImagesList.SelectedIndex >= 0 ? this.Images[this.ImagesList.SelectedIndex] : null;

        public Image? CurrentImage => this.CurrentObject?.Img ?? null;

		public bool ShowCrosshair { get; set; }


		// ----- ----- CONSTRUCTOR ----- ----- \\
		public ImageHandling(string repopath, ListBox? imageslistbox = null, PictureBox? viewpicturebox = null, NumericUpDown? zoomNumeric = null, Label? metalabel = null)
        {
            // Set attributes
            this.Repopath = repopath;
            this.ImagesList = imageslistbox ??= new ListBox();
			this.ViewPBox = viewpicturebox ??= new PictureBox();
			this.ZoomNumeric = zoomNumeric ??= new NumericUpDown();
			this.MetaLabel = metalabel ??= new Label();

			// Set min/max for zoom
			this.ZoomNumeric.Minimum = 1;
			this.ZoomNumeric.Maximum = 1000;

            // Register events
            //this.ViewPBox.DoubleClick += (s, e) => this.ImportImage();
            this.ImagesList.Click += (s, e) =>
            {
                // Require CTRL down
                if (Control.ModifierKeys == Keys.Control)
                {
                    this.RemoveImage(this.ImagesList.SelectedIndex);
				}
            };
            this.ImagesList.SelectedIndexChanged += (s, e) =>
            {
                this.ViewPBox.Image = this.CurrentImage;
                if (this.ShowCrosshair && this.CurrentImage != null)
				{
					// Render crosshair
					Bitmap bitmap = new(this.CurrentImage);
					using (Graphics g = Graphics.FromImage(bitmap))
					{
						Pen pen = new(Color.Red, 2);
						g.DrawLine(pen, this.CurrentImage.Width / 2, 0, this.CurrentImage.Width / 2, this.CurrentImage.Height);
						g.DrawLine(pen, 0, this.CurrentImage.Height / 2, this.CurrentImage.Width, this.CurrentImage.Height / 2);
					}
                    this.ViewPBox.Image = bitmap;
					this.ViewPBox.Invalidate();
				}
				this.MetaLabel.Text = (this.CurrentObject?.GetMetaString() ?? "No image selected");
            };
            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ImageHandling.CurrentImage))
                {
                    this.ViewPBox.Image = this.CurrentImage;

                    // Optionally, adjust PictureBox size to fit the image
                    if (this.CurrentImage != null)
                    {
                        this.ViewPBox.SizeMode = PictureBoxSizeMode.AutoSize;
                        this.ViewPBox.Width = (int) (this.CurrentImage.Width * this.zoomFactor);
                        this.ViewPBox.Height = (int) (this.CurrentImage.Height * this.zoomFactor);
                        this.ViewPBox.SizeMode = PictureBoxSizeMode.Zoom;

						// Update meta label
                        this.MetaLabel.Text = this.CurrentObject?.GetMetaString() ?? "No image selected";
					}
					else
                    {
                        this.ViewPBox.Image = null;
                    }
                }
            };

            // Register zoom and drag events
            this.ViewPBox.MouseWheel += this.ViewPBox_MouseWheel;
            this.ViewPBox.MouseDown += this.ViewPBox_MouseDown;
            this.ViewPBox.MouseMove += this.ViewPBox_MouseMove;
            this.ViewPBox.MouseUp += this.ViewPBox_MouseUp;

            // Register ZoomNumeric event
            this.ZoomNumeric.ValueChanged += this.ZoomNumeric_ValueChanged;
		}



		// ----- ----- UI METHODS ----- ----- \\
		private void ZoomNumeric_ValueChanged(object? sender, EventArgs e)
        {
            this.zoomFactor = (float)this.ZoomNumeric.Value / 100.0f;
            this.ApplyZoomToPoint(this.mouseDownLocation);
        }

        public void ViewPBox_MouseWheel(object? sender, MouseEventArgs e)
        {
            Point mousePosition = e.Location;

            if (e.Delta > 0)
            {
                this.ZoomNumeric.Value *= 1.1M;
            }
            else
            {
                this.ZoomNumeric.Value /= 1.1M;
            }

            this.zoomFactor = (float)this.ZoomNumeric.Value / 100.0f;
            this.ApplyZoomToPoint(mousePosition);
        }

        public void ViewPBox_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.isDragging = true;
                this.mouseDownLocation = e.Location;
            }
        }

        public void ViewPBox_MouseMove(object? sender, MouseEventArgs e)
        {
            if (this.isDragging)
            {
                this.ViewPBox.Left += e.X - this.mouseDownLocation.X;
                this.ViewPBox.Top += e.Y - this.mouseDownLocation.Y;
            }
        }

        public void ViewPBox_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.isDragging = false;
            }
        }

        private void ApplyZoomToPoint(Point point)
        {
            if (this.CurrentImage != null)
            {
                // Berechnen der neuen Größe basierend auf dem Zoomfaktor
                int newWidth = (int)(this.CurrentImage.Width * this.zoomFactor);
                int newHeight = (int)(this.CurrentImage.Height * this.zoomFactor);

                // Berechnen der neuen Position basierend auf dem Mauspunkt
                int offsetX = (int)((point.X - this.ViewPBox.Left) * (this.zoomFactor - 1));
                int offsetY = (int)((point.Y - this.ViewPBox.Top) * (this.zoomFactor - 1));

                // Setzen der Größe und Position der PictureBox
                this.ViewPBox.Width = newWidth;
                this.ViewPBox.Height = newHeight;
                this.ViewPBox.Left -= offsetX;
                this.ViewPBox.Top -= offsetY;

                // Setzen des SizeMode auf Zoom
                this.ViewPBox.SizeMode = PictureBoxSizeMode.Zoom;

				// Sicherstellen, dass die PictureBox innerhalb der Grenzen des Panels bleibt
				this.EnsurePictureBoxWithinBounds();
            }
        }

        private void EnsurePictureBoxWithinBounds()
        {
            Panel? parent = this.ViewPBox.Parent as Panel;
            if (parent != null)
            {
                if (this.ViewPBox.Left > 0)
                {
                    this.ViewPBox.Left = 0;
                }
                if (this.ViewPBox.Top > 0)
                {
                    this.ViewPBox.Top = 0;
                }
                if (this.ViewPBox.Right < parent.Width)
                {
                    this.ViewPBox.Left = parent.Width - this.ViewPBox.Width;
                }
                if (this.ViewPBox.Bottom < parent.Height)
                {
                    this.ViewPBox.Top = parent.Height - this.ViewPBox.Height;
                }
            }
        }



		// ----- ----- METHODS ----- ----- \\
		public void SetPictureBox(PictureBox newPictureBox)
		{
			// Events von alter ViewPBox entfernen
			if (this.ViewPBox != null)
			{
				// null
			}

			// Neue ViewPBox setzen
			this.ViewPBox = newPictureBox;

			// Bild setzen, falls vorhanden
			this.ViewPBox.Image = this.CurrentImage;
			this.ViewPBox.Invalidate();
		}

		public void FitZoom()
        {
            // Check if image is null
            if (this.CurrentImage == null)
            {
                return;
            }

            // Get parent of ViewPBox (panel)
            Panel? parent = this.ViewPBox.Parent as Panel;
            if (parent == null)
            {
                return;
            }

            // Get dimensions
            int containerWidth = parent.Width;
            int containerHeight = parent.Height;

            // Calculate zoom factor
            float widthFactor = (float)containerWidth / this.CurrentImage.Width;
            float heightFactor = (float)containerHeight / this.CurrentImage.Height;
            this.zoomFactor = Math.Min(widthFactor, heightFactor);

            // Set zoom numeric value
            this.ZoomNumeric.Value = (decimal)(this.zoomFactor * 100.0f);

            // Apply zoom
            this.ApplyZoom();
        }

        private void ApplyZoom()
        {
            if (this.CurrentImage != null)
            {
                // Calculate new size based on zoom factor
                int newWidth = (int)(this.CurrentImage.Width * this.zoomFactor);
                int newHeight = (int)(this.CurrentImage.Height * this.zoomFactor);

                // Set the size of the PictureBox
                this.ViewPBox.Width = newWidth;
                this.ViewPBox.Height = newHeight;

                // Set the size mode to stretch
                this.ViewPBox.SizeMode = PictureBoxSizeMode.StretchImage;

                // Center the PictureBox in the panel
                Panel? parent = this.ViewPBox.Parent as Panel;
                if (parent != null)
                {
                    this.ViewPBox.Left = (parent.Width - newWidth) / 2;
                    this.ViewPBox.Top = (parent.Height - newHeight) / 2;
                }
            }
        }

        public void CenterImage()
        {
            // PictureBox is in panel
            if (this.CurrentImage != null)
            {
                int containerWidth = this.ViewPBox.Parent?.Width ?? 0;
                int containerHeight = this.ViewPBox.Parent?.Height ?? 0;

                this.ViewPBox.Left = (containerWidth - this.ViewPBox.Width) / 2;
                this.ViewPBox.Top = (containerHeight - this.ViewPBox.Height) / 2;
            }
        }


        public static Tuple<int, int, int> GetRgbFromHue(float hue = 180.0f)
        {
            float s = 1.0f; // Saturation (Standardwert: 1)
            float v = 1.0f; // Value (Standardwert: 1)

            int h = (int) (hue / 60) % 6;
            float f = hue / 60 - h;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            float r = 0, g = 0, b = 0;

            switch (h)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;
                case 5:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }

            return Tuple.Create((int) (r * 255), (int) (g * 255), (int) (b * 255));
        }

        public static float GetHueFromRgb(int r = 128, int g = 128, int b = 128)
        {
            float rf = r / 255.0f;
            float gf = g / 255.0f;
            float bf = b / 255.0f;

            float max = Math.Max(rf, Math.Max(gf, bf));
            float min = Math.Min(rf, Math.Min(gf, bf));

            float hue = 0.0f;

            if (Math.Abs(max - min) < 0.01f)
            {
                hue = 0.0f; // Undefined
            }
            else if (Math.Abs(max - rf) < 0.01f)
            {
                hue = 60.0f * ((gf - bf) / (max - min));
            }
            else if (Math.Abs(max - gf) < 0.01f)
            {
                hue = 60.0f * (2.0f + (bf - rf) / (max - min));
            }
            else if (Math.Abs(max - bf) < 0.01f)
            {
                hue = 60.0f * (4.0f + (rf - gf) / (max - min));
            }

            if (hue < 0)
            {
                hue += 360.0f;
            }

            return hue;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void FillImagesListBox()
        {
            // Get selected index
            int selected = this.ImagesList.SelectedIndex;

            // Clear listbox & fill with every image name
            this.ImagesList.Items.Clear();
            foreach (ImageObject img in this.Images)
            {
                string entry = img.Name;
                if (img.Pointer != 0)
                {
                    entry = " * " + entry;
                }

                if (entry.Length > 32)
                {
                    entry = entry.Substring(0, 30) + "..." + entry.Substring(entry.Length - 2, 2);
				}

                this.ImagesList.Items.Add(entry);
            }

            // Select none selected if possible
            if (selected >= 0 && selected < this.ImagesList.Items.Count)
            {
                this.ImagesList.SelectedIndex = selected;
            }
            else if (this.ImagesList.Items.Count > 0)
            {
                this.ImagesList.SelectedIndex = 0;
            }
        }

        public void LoadResourcesImages(int maxFileSizeMb = 8)
        {
            // Get every file in Repopath + Resources + Images
            string[] files = Directory.GetFiles(Path.Combine(this.Repopath, "Resources"), "*");

            // Loop through every file
            foreach (string f in files)
            {
                // Create file info
                FileInfo fi = new(f);
                if (fi.Length <= maxFileSizeMb * 1024 * 1024 && (fi.Extension.ToLower() == ".jpg" || fi.Extension.ToLower() == ".bmp" || fi.Extension.ToLower() == ".png"))
                {
                    // If smaller, add
                    this.AddImage(f);
                }
            }
        }


        public string ImportImage()
        {
            string filepath = "";

            // Create OpenFileDialog
            using OpenFileDialog ofd = new();

            // Set OFD params
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            ofd.Filter = @"Image Files|*.tif;*.tiff;*.bmp;*.png;*.jpg;*.jpeg";
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;

            // Show dialog and get result
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Get selected file path
                filepath = ofd.FileName;

                // Add image to list
                this.AddImage(filepath);
            }

            return filepath;
        }

        public ImageObject AddImage(string filepath)
        {
            // Create obj
            ImageObject obj = new ImageObject(filepath);

			// Add new ImageObject to list
			this.Images.Add(obj);

            // Fill listbox
            this.FillImagesListBox();

            // Select last entry in listbox
            this.ImagesList.SelectedIndex = this.ImagesList.Items.Count - 1;

			// Fit zoom
			this.FitZoom();

            return obj;
		}

        public void RemoveImage(int index = -1)
        {
            // Get index if -1
            if (index < 0)
            {
                index = this.ImagesList.SelectedIndex;
            }

            // Check index range
            if (index < 0 || index >= this.Images.Count)
            {
                return;
            }

            // Remove from list
            this.Images.RemoveAt(index);

            // Refill listbox
            this.FillImagesListBox();
        }

        public ImageObject CreateEmpty(Color? backColor = null, Size? size = null, string name = "", bool add = true)
        {
            // Verify color & size
            backColor ??= Color.Transparent;
			size ??= new Size(512, 512);

            // Generate name
            if (string.IsNullOrEmpty(name))
            {
                name = "Image_" + (this.Images.Count + 1).ToString("00");
            }

            // Create bitmap with size and color
            Bitmap bitmap = new(size.Value.Width, size.Value.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(backColor.Value);
            }

            // Create new ImageObject
            ImageObject imgObj = new(bitmap, name);

            // Optionally: Add to list
            if (add)
            {
                this.Images.Add(imgObj);
            }

            // Fill listbox
            this.FillImagesListBox();

            // Select last entry in listbox
            this.ImagesList.SelectedIndex = this.ImagesList.Items.Count - 1;

			// Fit zoom
            this.FitZoom();

            return imgObj;
		}

        public static ImageObject PopEmpty(Color? color = null, Size? size = null, string name = "")
        {
			// Verify color & size
			color ??= Color.Transparent;
			size ??= new Size(512, 512);

			// Generate name
			if (string.IsNullOrEmpty(name))
			{
                Random rand = new Random();
				name = "Image_" + (rand.Next()).ToString("00");
			}

			// Create bitmap with size and color
			Bitmap bitmap = new(size.Value.Width, size.Value.Height, PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(bitmap))
			{
				g.Clear(color.Value);
			}

			// Create new ImageObject
			ImageObject imgObj = new(bitmap, name);

            return imgObj;
		}

		public ImageObject? Clone(int index = -1)
        {
            // Get index if -1
            if (index < 0)
            {
                index = this.ImagesList.SelectedIndex;
            }

            // Check index range
            if (index < 0 || index >= this.Images.Count)
            {
                return null;
            }

            // Clone image object
            ImageObject clone = new(this.Images[index]);

            // Add to list
            this.Images.Add(clone);

            // Fill listbox
            this.FillImagesListBox();

            // Select last entry in listbox
            this.ImagesList.SelectedIndex = this.ImagesList.Items.Count - 1;

            // Return clone
            return clone;
        }

		public void ImportGif()
		{
			using OpenFileDialog ofd = new()
			{
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
				Filter = @"GIF Files|*.gif",
				Multiselect = false,
				RestoreDirectory = true
			};

			if (ofd.ShowDialog() == DialogResult.OK)
			{
				string filepath = ofd.FileName;

				using Bitmap gif = new(filepath);
				FrameDimension dimension = FrameDimension.Time;
				int frameCount = gif.GetFrameCount(dimension);

				for (int i = 0; i < frameCount; i++)
				{
					// Aktives Frame setzen
					gif.SelectActiveFrame(dimension, i);

					// Kopie erzeugen, da gif bei jedem Frame nur das interne Bild wechselt
					Bitmap frameCopy = new(gif.Width, gif.Height);
					using (Graphics g = Graphics.FromImage(frameCopy))
					{
						g.DrawImageUnscaled(gif, 0, 0);
					}

					// Neues ImageObject erstellen
					ImageObject imgObj = new(frameCopy, Path.GetFileNameWithoutExtension(filepath) + i.ToString("D3"));

					// Zur Liste hinzufügen
					this.Images.Add(imgObj);
				}
			}

			// Liste aktualisieren
			this.FillImagesListBox();
		}


        public void Dispose()
        {
            // Free every imgobj
            foreach (ImageObject obj in this.Images)
            {
                obj.Dispose();
            }

            this.Images = [];
        }

	}

	public class ImageObject
    {
        // ----- ----- ATTRIBUTES ----- ----- \\
        public string Name = "image";
        public string Filepath = "";

        public Image? Img = null;
        public int Width = 0;
        public int Height = 0;
        public int BitsPerPixel = 0;
        public PixelFormat Format = PixelFormat.DontCare;

        public IntPtr Pointer = 0;

        public List<string> Modifications = [];

        // ----- ----- LAMBDA ----- ----- \\
        public long Size => this.Width * this.Height * this.BitsPerPixel / 8;

        public int Channels => 4;

        public bool OnHost => this.Img != null && this.Pointer == 0;
        public bool OnDevice => this.Img == null && this.Pointer != 0;


        // ----- ----- CONSTRUCTOR ----- ----- \\
        public ImageObject(string filepath)
        {
            // Verify filepath + name
            this.Filepath = this.VerifyFilepath(filepath);
            this.Name = Path.GetFileNameWithoutExtension(this.Filepath);

            // Load image from file
            this.Img = Image.FromFile(filepath);

            // Set attributes
            this.Width = this.Img.Width;
            this.Height = this.Img.Height;
            this.Format = this.Img.PixelFormat;
            this.BitsPerPixel = Image.GetPixelFormatSize(this.Img.PixelFormat);
        }

        public ImageObject(Bitmap bitmap, string name = "Empty")
		{
			// Set attributes
			this.Name = name;
			this.Img = bitmap;
			this.Width = bitmap.Width;
			this.Height = bitmap.Height;
			this.Format = bitmap.PixelFormat;
			this.BitsPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat);
		}

        public ImageObject(ImageObject obj)
        {
            this.Name = obj.Name;
			this.Filepath = obj.Filepath;
			this.Img = obj.Img;
			this.Width = obj.Width;
			this.Height = obj.Height;
			this.Format = obj.Format;
			this.BitsPerPixel = obj.BitsPerPixel;
            this.Pointer = obj.Pointer;
		}

        public ImageObject(byte[] pixels, Size size)
        {
			// Create bitmap from byte array
			this.Img = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
			this.Width = size.Width;
			this.Height = size.Height;
		    this.Pointer = 0;
			this.Format = PixelFormat.Format32bppArgb;
			this.BitsPerPixel = Image.GetPixelFormatSize(this.Format);
			this.SetImageFromBytes(pixels, true);

			// Set name
			this.Name = "Image_" + (this.Width * this.Height).ToString("00");
            this.Filepath = "";
			this.Modifications = [];
		}



		// ----- ----- METHODS ----- ----- \\
		public string VerifyFilepath(string filepath)
        {
            string verified = "";
            string[] formats = [".tif", ".tiff", ".bmp", ".png", ".jpg", ".jpeg"];

            // Check file exists
            if (!File.Exists(filepath))
            {
                return verified;
            }

            // Check extension
            if (!formats.Contains(Path.GetExtension(filepath).ToLower()))
            {
                return verified;
            }

            // Check size
            FileInfo info = new(filepath);
            if (info.Length < 1)
            {
                return verified;
            }

            // Pass
            return filepath;
        }

        public List<byte[]> GetPixelRowsAsBytes()
        {
            // New byte[]-list for each row & new bitmap from image
            List<byte[]> rows = [];

            // Check img
            if (this.Img == null)
            {
                return rows;
            }

            // New bitmap
            Bitmap bmp = new(this.Img);

            // For every row: Lock, copy, unlock
            for (int y = 0; y < bmp.Height; y++)
            {
                // Get row
                Rectangle rect = new(0, y, bmp.Width, 1);
                BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

                // Copy row
                byte[] row = new byte[bmpData.Stride];
                Marshal.Copy(bmpData.Scan0, row, 0, row.Length);
                rows.Add(row);

                // Unlock
                bmp.UnlockBits(bmpData);
            }

            // Return list of byte[]
            return rows;
        }

        public Image? SetImageFromChunks(List<byte[]> rows)
        {
            // Get dimensions from rows
            int width = (rows.FirstOrDefault()?.Length ?? 0) / 4;
            int height = rows.Count;

            // Check dimensions
            if (width < 1 || height < 1)
            {
                this.Pointer = 0;
                this.Img = null;
                return null;
            }

            // New bitmap
            Bitmap bmp = new(width, height, PixelFormat.Format32bppArgb);

            // For every row: Lock, copy, unlock
            for (int y = 0; y < bmp.Height; y++)
            {
                // Get row
                Rectangle rect = new(0, y, bmp.Width, 1);
                BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

                // Copy row
                byte[] row = rows[y];
                Marshal.Copy(row, 0, bmpData.Scan0, row.Length);

                // Unlock
                bmp.UnlockBits(bmpData);
            }

            // Update image
            this.Img = bmp;
            this.Pointer = 0;

            // Return image
            return bmp;
        }



        public byte[] GetPixelsAsBytes(bool nullImage = false)
        {
            if (this.Img == null)
			{
				return [];
			}

			Bitmap bmp = new(this.Img);
            Rectangle rect = new(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

            try
            {
                // Berechne die tatsächliche Byte-Länge OHNE Stride-Padding
                int bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
                int unpaddedRowSize = bmp.Width * bytesPerPixel;
                int totalBytes = unpaddedRowSize * bmp.Height;

                byte[] pixels = new byte[totalBytes];

                // Kopiere Zeile für Zeile und ignoriere Stride-Padding
                for (int y = 0; y < bmp.Height; y++)
                {
                    IntPtr rowPtr = bmpData.Scan0 + (y * bmpData.Stride);
                    Marshal.Copy(rowPtr, pixels, y * unpaddedRowSize, unpaddedRowSize);
                }

                // Optional: Null bytes
                if (nullImage)
                {
                    this.Img?.Dispose();
					this.Img = null;
				}

				return pixels;
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
        }

        public Image? SetImageFromBytes(byte[] pixels, bool nullPointer = false)
        {
            int width = this.Width;
            int height = this.Height;

            // Versuche, das Format zu erraten (z. B. 3 oder 4 Bytes/Pixel)
            PixelFormat format = (pixels.Length == width * height * 3)
                ? PixelFormat.Format24bppRgb
                : PixelFormat.Format32bppArgb;

            Bitmap bmp = new(width, height, format);
            Rectangle rect = new(0, 0, width, height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, format);

            try
            {
                Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
                this.Img = bmp;

				// Optional: Null pointer
				if (nullPointer)
				{
					this.Pointer = 0;
				}

				return bmp;
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
        }

        public void ResetImage()
        {
            // Check filepath
            if (string.IsNullOrEmpty(this.Filepath))
            {
                return;
            }

            // Dispose image
            if (this.Img != null)
            {
                this.Img.Dispose();
                this.Img = null;
            }

            // Load image from file
            this.Img = Image.FromFile(this.Filepath);

            // Set attributes
            this.Width = this.Img.Width;
            this.Height = this.Img.Height;
            this.Format = this.Img.PixelFormat;
            this.BitsPerPixel = Image.GetPixelFormatSize(this.Img.PixelFormat);
			this.Pointer = 0;
			this.Modifications.Clear();
		}

        public string? Export(bool showMsgbox = false)
        {
            // Create SaveFileDialog
            using SaveFileDialog sfd = new();

			// Set SFD params
			sfd.Title = "Save Image";
			sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            sfd.Filter = @"Image Files|*.bmp;*.png;*.jpg;*.jpeg*.tif;*.tiff;";
            sfd.FileName = this.Name + "_" + string.Join("_", this.Modifications) + ".png";

			// Show dialog and get result
			DialogResult result = sfd.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Get selected file path
                string filepath = sfd.FileName;

                // Save image to file
                this.Img?.Save(filepath, ImageFormat.Png);

				// Optionally show message box
				if (showMsgbox)
				{
					MessageBox.Show($"Image saved to {filepath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}

				// Return file path
				return filepath;
            }

			// Optionally show message box
			if (showMsgbox)
			{
				MessageBox.Show("No file selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			return null;


        }

        public string GetMetaString()
        {
            string meta = "";

			// Add dimensions, bits and pointer
			meta += $"{this.Width} x {this.Height} px., {this.BitsPerPixel} bpp, <{this.Pointer}>";

			// Return meta
			return meta;
		}

        public void AddModification(string modification = "null", string value = "")
		{
			// Add modification to list
			this.Modifications.Add(modification + (!string.IsNullOrEmpty(value) ? "-" + value : ""));
		}

        public void Dispose()
        {
            this.Img?.Dispose();
            this.Img = null;
            this.Width = 0;
            this.Height = 0;
            this.Pointer = IntPtr.Zero;
            this.BitsPerPixel = 0;
        }

	}
}
