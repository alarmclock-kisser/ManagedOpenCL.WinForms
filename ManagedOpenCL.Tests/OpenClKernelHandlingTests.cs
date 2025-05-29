using System.Drawing;

namespace ManagedOpenCL.Tests
{
	[TestClass]
	public sealed class OpenClKernelHandlingTests
	{
		// ----- ----- ----- TEST ATTRIBUTES ----- ----- ----- \\
		public string Repopath = "";

		public OpenClService? Service;

		public OpenClMemoryRegister? MemoryRegister => this.Service?.MemorRegister;
		public OpenClKernelHandling? KernelHandling => this.Service?.KernelHandling;




		// ----- ----- ----- INITIALIZE & CLEANUP ----- ----- ----- \\
		[TestInitialize]
		public void TestInitialize()
		{
			// Set repopath
			this.Repopath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));

			// Initialize the OpenCL service
			this.Service = new OpenClService(this.Repopath, null, null);

			// Ensure the service is initialized
			this.Service.FillDevicesCombo();
			this.Service.SelectDeviceLike("Intel"); // Assuming "Intel" is a valid device name for testing
		}

		[TestCleanup]
		public void TestCleanup()
		{
			this.MemoryRegister?.Dispose();
			this.KernelHandling?.Dispose();
			this.Service?.Dispose();
			this.Service = null;
		}



		// ----- ----- ----- TEST METHODS ----- ----- ----- \\
		[TestMethod]
		public void GetArgumentDefinitions_TestImagingKernel_InPlace_ShouldReturn8Kvp()
		{
			// Arrange
			string kernelName = "testImagingKernel_inPlaceXX";
			this.KernelHandling?.LoadKernel(kernelName, "", null, true);

			// Act
			Dictionary<string, Type> result = this.KernelHandling?.GetKernelArguments() ?? [];
			Dictionary<string, Type> resultAnalog = this.KernelHandling?.GetKernelArgumentsAnalog(this.KernelHandling?.KernelFile) ?? [];

			// Assert
			Assert.IsNotNull(this.KernelHandling, "KernelHandling should be initialized with context first.");
			Assert.IsNotNull(this.KernelHandling.Kernel, $"Kernel should not be null after loading. ({this.KernelHandling?.KernelFile ?? "NO FILE SET"})");
			Assert.IsTrue(result.Count == 8, $"Arguments (kvp) should count 8. (Is {result.Count}).");
			Assert.IsTrue(resultAnalog.Count == 8, $"Analog arguments (kvp) should count 8. (Is {resultAnalog.Count}).");
			Assert.AreEqual(result, resultAnalog, $"Arguments & analog arguments should be equal. ({result.Count} != {resultAnalog.Count}).");
		}

		[TestMethod]
		public void ExecKernelImage_InPlace_Mandelbrot_ShouldReturnColoredImageWithSize()
		{
			// Arrange
			Size imgSize = new(512, 512);
			ImageObject obj = ImageHandling.PopEmpty(null, imgSize, "");

			// Act
			this.KernelHandling?.ExecKernelImage(obj, "mandelbrotPrecise", "01");

			// Assert
			Assert.IsNotNull(obj.Img, "Image of object should not be null after kernel execution.");
			Assert.AreEqual(imgSize, obj.Img.Size, "Image size should match the expected size after kernel execution.");
			Assert.IsTrue(obj.Img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb, "Image pixel format should be Format32bppArgb after kernel execution.");
			Assert.IsTrue(obj.Pointer == IntPtr.Zero, "Pointer should be zero after kernel execution for in-place processing.");
		}

		[TestMethod]
		public void ExecKernelAudio_Normalize_InPlace_NoChunking_ShouldReturnNormalizedAudio()
		{
			// Arrange
			string kernelName = "normalizeInPlace";
			string kernelVersion = "01";
			AudioObject audioObj = AudioHandling.PopEmpty(1024, 1.1f, 44100, 1, 16);

			// Act
			this.KernelHandling?.ExecKernelAudio(audioObj, kernelName, kernelVersion, 0, 0, 1, [0.9f], false);

			// Assert
			Assert.IsTrue(audioObj.Data.LongLength == 1024, "Audio data should have length of 1024");
			Assert.IsTrue(audioObj.Length == 1024, "Audio length should be 1024 after processing.");
			Assert.IsFalse(audioObj.Data.Any(x => x > 0.9f || x < -0.9f), "Audio data should be normalized within the range [-0.9, 0.9] after processing.");
			Assert.IsTrue(audioObj.Pointer == IntPtr.Zero, "Pointer should be zero after kernel execution for in-place processing.");
		}

		[TestMethod]
		public void ExecKernelAudio_Normalize_InPlace_WithChunking_ShouldReturnNormalizedAudio()
		{
			// Arrange
			string kernelName = "normalizeInPlace";
			string kernelVersion = "01";
			AudioObject audioObj = AudioHandling.PopEmpty(2048, 2.0f, 44100, 1, 16);

			// Act
			this.KernelHandling?.ExecKernelAudio(audioObj, kernelName, kernelVersion, 256, 0.5f, 1, [0.9f], false);

			// Assert
			Assert.IsTrue(audioObj.Data.LongLength == 2048, $"Audio data should have length of 1024. (Is {audioObj.Data.LongLength}).");
			Assert.IsTrue(audioObj.Length == 2048, $"Audio length should be 1024 after processing. (Is {audioObj.Length}).");
			Assert.IsFalse(audioObj.Data.Any(x => x > 0.9f || x < -0.9f), "Audio data should be normalized within the range [-0.9, 0.9] after processing.");
			Assert.IsTrue(audioObj.Pointer == IntPtr.Zero, $"Pointer should be zero after kernel execution for in-place processing. (Is {audioObj.Pointer}).");
		}

		[TestMethod]
		public void ExecKernelAudio_Normalize_OutOfPlace_NoChunking_ShouldReturnNormalizedAudio()
		{
			// Arrange
			string kernelName = "normalizeOutOfPlace";
			string kernelVersion = "01";
			AudioObject audioObj = AudioHandling.PopEmpty(2048, 2.0f, 44100, 1, 16);

			// Act
			this.KernelHandling?.ExecKernelAudio(audioObj, kernelName, kernelVersion, 0, 0, 1, [0.9f], false);

			// Assert
			float maxSample = audioObj.Data.Max(Math.Abs);
			Assert.AreEqual(0.9f, maxSample, 0.01f, $"Max value should be ~0.9f after normalization. (Is {maxSample}).");
			Assert.IsTrue(audioObj.Data.LongLength == 2048, $"Audio data should have length of 1024. (Is {audioObj.Data.LongLength}).");
			Assert.IsTrue(audioObj.Length == 2048, $"Audio length should be 1024 after processing. (Is {audioObj.Length}).");
			Assert.IsTrue(audioObj.Pointer == IntPtr.Zero, $"Pointer should be zero after kernel execution for in-place processing. (Is {audioObj.Pointer}).");
		}

		[TestMethod]
		public void ExecKernelAudio_Normalize_OutOfPlace_WithChunking_ShouldReturnNormalizedAudio()
		{
			// Arrange
			string kernelName = "normalizeOutOfPlace";
			string kernelVersion = "01";
			AudioObject audioObj = AudioHandling.PopEmpty(2048, 2.0f, 44100, 1, 16);

			// Act
			this.KernelHandling?.ExecKernelAudio(audioObj, kernelName, kernelVersion, 256, 0.5f, 1, [0.9f], false);

			// Assert
			Assert.IsTrue(audioObj.Data.LongLength == 2048, $"Audio data should have length of 1024. (Is {audioObj.Data.LongLength}).");
			Assert.IsTrue(audioObj.Length == 2048, $"Audio length should be 1024 after processing. (Is {audioObj.Length}).");
			Assert.IsFalse(audioObj.Data.Any(x => x > 0.9f || x < -0.9f), "Audio data should be normalized within the range [-0.9, 0.9] after processing.");
			Assert.IsTrue(audioObj.Pointer == IntPtr.Zero, $"Pointer should be zero after kernel execution for in-place processing. (Is {audioObj.Pointer}).");
		}
	}
}
