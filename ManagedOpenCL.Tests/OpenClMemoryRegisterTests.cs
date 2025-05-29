using System.Configuration;

namespace ManagedOpenCL.Tests
{
	[TestClass]
	public sealed class OpenClMemoryRegisterTests
	{
		// ----- ----- ----- TEST ATTRIBUTES ----- ----- ----- \\
		public string Repopath = "";

		public OpenClService? Service;

		public OpenClMemoryRegister? MemoryRegister => this.Service?.MemorRegister;
		public OpenClKernelHandling? KernelHandling => this.Service?.KernelHandling;


		public required ImageHandling ImageHandling;
		public required AudioHandling AudioHandling;




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

			// Init. imgH + audioH
			this.ImageHandling = new ImageHandling(this.Repopath);
			this.AudioHandling = new AudioHandling(this.Repopath);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			// this.ImageObject.Dispose();
			this.ImageHandling.Dispose();

			// this.AudioObject.Dispose();
			this.AudioHandling.Dispose();

			this.MemoryRegister?.Dispose();
			this.KernelHandling?.Dispose();
			this.Service?.Dispose();
			this.Service = null;
		}




		// ----- ----- ----- TEST METHODS ----- ----- ----- \\
		[TestMethod]
		public void PushData_FloatArray128_ShouldReturnClMem()
		{
			// Arrange
			float[] data = new float[128];
			Array.Fill(data, 1.0f);  // Füllt das Array mit 1en

			// Act
			ClMem? result = this.MemoryRegister?.PushData(data);

			// Assert
			Assert.IsNotNull(result, "PushData should return a valid ClMem object.");
		}

		[TestMethod]
		public void PushData_FloatArray256_ShouldGetTypeFloat()
		{
			// Arrange
			float[] data = new float[256];
			Array.Fill(data, 1.0f);  // Füllt das Array mit 1en
			
			// Act
			ClMem? result = this.MemoryRegister?.PushData(data);
			Type type = this.MemoryRegister?.GetBufferType(result?.IndexHandle ?? IntPtr.Zero) ?? typeof(void);
			
			// Assert
			Assert.IsNotNull(result, "PushData should return a valid ClMem object.");	
			Assert.AreEqual(typeof(float), type, "GetBufferType should return float for a buffer created from a float array of length 256.");
		}

		[TestMethod]
		public void PushImageObj_Default_ShouldNotReturnIntPtrZero()
		{
			// Arrange
			ImageObject obj = this.ImageHandling.CreateEmpty(add: false);
			
			// Act
			IntPtr result = this.MemoryRegister?.PushImage(obj) ?? IntPtr.Zero;

			// Assert
			Assert.IsTrue(result != IntPtr.Zero, "IntPtr should not be zero after pushinhg image.");
			Assert.AreEqual(result, obj.Pointer, "IntPtr should be equal to pointer of object.");
		}

		[TestMethod]
		public void PushAudioObj_CreateEmpty_NoChunking_ShouldNotReturnIntPtrZero()
		{
			// Arrange
			AudioObject obj = this.AudioHandling.CreateEmptyTrack(8192, 1000, 1, 8, false);

			// Act
			IntPtr result = this.MemoryRegister?.PushAudio(obj) ?? IntPtr.Zero;

			// Assert
			Assert.IsTrue(result != IntPtr.Zero, "IntPtr should not be zero after pushing track.");
			Assert.AreEqual(result, obj.Pointer, "IntPtr should be equal to pointer of object.");
			Assert.IsTrue(obj.ChunkSize == 0, "Object ChunkSize should be 0 after pushing with no chunking.");
			Assert.IsTrue(obj.OverlapSize == 0, "Object OverlapSize should be 0 after pushing with no chunking.");
		}
		
		[TestMethod]
		public void PushAudioObj_CreateEmpty_DefaultChunking_ShouldNotReturnIntPtrZero()
		{
			// Arrange
			AudioObject obj = this.AudioHandling.CreateEmptyTrack(8192, 1000, 1, 8, false);
			int chunkSize = 1024;
			float overlap = 0.5f;
			int overlapSize = (int) (chunkSize * overlap);

			// Act
			IntPtr result = this.MemoryRegister?.PushAudio(obj, chunkSize, overlap) ?? IntPtr.Zero;

			// Assert
			Assert.IsTrue(result != IntPtr.Zero, "IntPtr should not be zero after pushing track.");
			Assert.AreEqual(result, obj.Pointer, "IntPtr should be equal to pointer of object.");
			Assert.IsTrue(obj.ChunkSize == chunkSize, $"Object ChunkSize should be {chunkSize} after pushing with chunking.");
			Assert.IsTrue(obj.OverlapSize == overlapSize, $"Object OverlapSize should be {overlapSize} after pushing with chunking.");
		}
	}
}
