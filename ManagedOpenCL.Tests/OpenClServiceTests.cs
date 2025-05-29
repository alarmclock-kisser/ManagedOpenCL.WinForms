namespace ManagedOpenCL.Tests
{
	[TestClass]
	public sealed class OpenCLServiceTests
	{
		public string Repopath = "";

		public OpenClService? Service;



		[TestInitialize]
		public void TestInitialize()
		{
			// Set repopath
			this.Repopath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));

			// Initialize the OpenCL service
			this.Service = new OpenClService(this.Repopath, null, null);
		}



		[TestCleanup]
		public void TestCleanup()
		{
			this.Service?.Dispose();
			this.Service = null;
		}


		[TestMethod]
		public void SelectDeviceLike_Intel_ContextCreatedWithHandlings()
		{
			// Arrange
			this.Service?.FillDevicesCombo();

			// Act
			this.Service?.SelectDeviceLike("Intel");

			// Assert
			Assert.IsNotNull(this.Service?.CTX, "Context should be created when selecting a device like 'Intel'.");
			Assert.IsNotNull(this.Service.DEV, "Device should be set when selecting a device like 'Intel'.");
			Assert.IsNotNull(this.Service.PLAT, "Platform should be set when selecting a device like 'Intel'.");
			Assert.IsTrue(this.Service.INDEX != -1, "Index should be set when selecting a device like 'Intel'.");
			Assert.IsTrue(this.Service.MemorRegister != null, "Memory register should be initialized when selecting a device like 'Intel'.");
			Assert.IsTrue(this.Service.KernelHandling != null, "Kernel handling should be initialized when selecting a device like 'Intel'.");
		}
	}
}
