namespace ManagedOpenCL.Tests
{
	[TestClass]
	public sealed class OpenClKernelHandlingTests
	{
		public string Repopath = "";

		public OpenClService? Service;

		public OpenClMemoryRegister? MemoryRegister => this.Service?.MemorRegister;
		public OpenClKernelHandling? KernelHandling => this.Service?.KernelHandling;



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


		[TestMethod]
		public void Push()
		{

		}
	}
}
