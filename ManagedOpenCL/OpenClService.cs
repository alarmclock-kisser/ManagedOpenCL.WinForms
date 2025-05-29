using OpenTK;
using OpenTK.Compute.OpenCL;
using System.Text;

namespace ManagedOpenCL
{
	public class OpenClService
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		public string Repopath;
		public ListBox LogList;
		public ComboBox DevicesCombo;

		public string KernelPath => Path.Combine(this.Repopath, "Kernels");

		public OpenClMemoryRegister? MemorRegister;
		public OpenClKernelHandling? KernelHandling;

		public int INDEX = -1;
		public CLContext? CTX = null;
		public CLDevice? DEV = null;
		public CLPlatform? PLAT = null;




		// ----- ----- ----- LAMBDA ----- ----- ----- \\
		public Dictionary<CLDevice, CLPlatform> DevicesPlatforms => this.GetDevicesPlatforms();
		public Dictionary<string, string> Names => this.GetNames();






		// ----- ----- ----- CONSTRUCTORS ----- ----- ----- \\
		public OpenClService(string repopath, ListBox? logList = null, ComboBox? devicesComboBox = null)
		{
			this.Repopath = repopath;
			this.LogList = logList ?? new ListBox();
			this.DevicesCombo = devicesComboBox ?? new ComboBox();

			// Register events
			this.DevicesCombo.SelectedIndexChanged += (sender, e) => this.InitContext(this.DevicesCombo.SelectedIndex, silent: false);

			// Fill devices combo box
			this.FillDevicesCombo();
		}




		// ----- ----- ----- METHODS ----- ----- ----- \\





		// ----- ----- ----- PUBLIC METHODS ----- ----- ----- \\
		// Log
		public void Log(string message = "", string inner = "", int indent = 0)
		{
			string msg = "[Context]: " + new string(' ', indent * 2) + message;

			if (!string.IsNullOrEmpty(inner))
			{
				msg += " (" + inner + ")";
			}

			// Add to logList
			this.LogList.Items.Add(msg);

			// Scroll down
			this.LogList.SelectedIndex = this.LogList.Items.Count - 1;
		}


		// Dispose
		public void Dispose(bool silent = false)
		{
			// Dispose context
			if (this.CTX != null)
			{
				CL.ReleaseContext(this.CTX.Value);
				this.PLAT = null;
				this.DEV = null;
				this.CTX = null;
			}

			// Dispose memory handling
			this.MemorRegister?.Dispose();

			// Dispose kernel handling
			this.KernelHandling?.Dispose();

			// Log
			if (!silent)
			{
				this.Log("Disposed OpenCL context", "No context available");
			}
		}


		// Platforms & Devices
		public CLPlatform[] GetPlatforms()
		{
			// Get all OpenCL platforms
			CLResultCode error = CL.GetPlatformIds(out CLPlatform[] platforms);
			if (error != CLResultCode.Success || platforms.Length == 0)
			{
				this.Log("Error getting Cl-Platforms", platforms.Length.ToString(), 1);
			}

			// Return
			return platforms;
		}

		public Dictionary<CLDevice, CLPlatform> GetDevicesPlatforms()
		{
			// Return dict
			Dictionary<CLDevice, CLPlatform> devicesPlatforms = [];

			// Get platforms
			CLPlatform[] platforms = this.GetPlatforms();

			// Foreach platform get devices
			for (int i = 0; i < platforms.Length; i++)
			{
				// Get devices
				CLResultCode error = CL.GetDeviceIds(platforms[i], DeviceType.All, out CLDevice[] devices);
				if (error != CLResultCode.Success)
				{
					this.Log("Error getting Devices for CL-Platform", i.ToString(), 1);
				}

				// Foreach device, add to dict with platform
				foreach (CLDevice dev in devices)
				{
					devicesPlatforms.Add(dev, platforms[i]);
				}
			}

			// Return
			return devicesPlatforms;
		}


		// Get device & platform info
		public string GetDeviceInfo(CLDevice? device = null, DeviceInfo info = DeviceInfo.Name, bool silent = false)
		{
			// Verify device
			device ??= this.DEV;
			if (device == null)
			{
				if (!silent)
				{
					this.Log("No OpenCL device", "No device specified");
				}

				return "N/A";
			}

			// Get device info
			CLResultCode error = CL.GetDeviceInfo(device.Value, info, out byte[] infoBytes);
			if (error != CLResultCode.Success || infoBytes.Length == 0)
			{
				if (!silent)
				{
					this.Log("Failed to get device info", error.ToString());
				}

				return "N/A";
			}

			// Convert to string if T is string
			if (info == DeviceInfo.Name || info == DeviceInfo.DriverVersion || info == DeviceInfo.Version || info == DeviceInfo.Vendor || info == DeviceInfo.Profile || info == DeviceInfo.OpenClCVersion || info == DeviceInfo.Extensions)
			{
				// Handle extensions as comma-separated string
				if (info == DeviceInfo.Extensions)
				{
					string extensions = Encoding.UTF8.GetString(infoBytes).Trim('\0');
					return string.Join(", ", extensions.Split('\0'));
				}
				return Encoding.UTF8.GetString(infoBytes).Trim('\0');
			}

			// Convert to string if T is a numeric type
			if (info == DeviceInfo.MaximumComputeUnits || info == DeviceInfo.MaximumWorkItemDimensions || info == DeviceInfo.MaximumWorkGroupSize || info == DeviceInfo.MaximumClockFrequency || info == DeviceInfo.AddressBits || info == DeviceInfo.VendorId)
			{
				return BitConverter.ToInt32(infoBytes, 0).ToString();
			}
			else if (info == DeviceInfo.MaximumWorkItemSizes)
			{
				return string.Join(", ", infoBytes.Select(b => b.ToString()).ToArray());
			}
			else if (info == DeviceInfo.MaximumConstantBufferSize)
			{
				return BitConverter.ToInt32(infoBytes, 0).ToString();
			}
			else if (info == DeviceInfo.GlobalMemorySize || info == DeviceInfo.LocalMemorySize || info == DeviceInfo.GlobalMemoryCacheSize)
			{
				return BitConverter.ToInt64(infoBytes, 0).ToString();
			}
			else if (info == DeviceInfo.MaximumMemoryAllocationSize)
			{
				return BitConverter.ToUInt64(infoBytes, 0).ToString();
			}
			else if (info == DeviceInfo.MaximumMemoryAllocationSize)
			{
				return BitConverter.ToInt64(infoBytes, 0).ToString();
			}

			// Convert to string if T is a boolean type
			if (info == DeviceInfo.ImageSupport)
			{
				return (infoBytes[0] != 0).ToString();
			}

			// Convert to string if T is a byte array
			// Here you can add more cases if needed

			// Return "N/A" if info type is not supported
			if (!silent)
			{
				this.Log("Unsupported type for device info", info.ToString());
			}
			return "N/A";
		}

		public string GetPlatformInfo(CLPlatform? platform = null, PlatformInfo info = PlatformInfo.Name, bool silent = false)
		{
			// Verify platform
			platform ??= this.PLAT;
			if (platform == null)
			{
				if (!silent)
				{
					this.Log("No OpenCL platform", "No platform specified");
				}
				return "N/A";
			}

			// Get platform info
			CLResultCode error = CL.GetPlatformInfo(platform.Value, info, out byte[] infoBytes);
			if (error != CLResultCode.Success || infoBytes.Length == 0)
			{
				if (!silent)
				{
					this.Log("Failed to get platform info", error.ToString());
				}
				return "N/A";
			}

			// Convert to string for text-based info types
			if (info == PlatformInfo.Name ||
				info == PlatformInfo.Vendor ||
				info == PlatformInfo.Version ||
				info == PlatformInfo.Profile ||
				info == PlatformInfo.Extensions)
			{
				return Encoding.UTF8.GetString(infoBytes).Trim('\0');
			}

			// Convert numeric types to string
			if (info == PlatformInfo.PlatformHostTimerResolution)
			{
				return BitConverter.ToUInt64(infoBytes, 0).ToString();
			}

			// Handle extension list as comma-separated string
			if (info == PlatformInfo.Extensions)
			{
				string extensions = Encoding.UTF8.GetString(infoBytes).Trim('\0');
				return string.Join(", ", extensions.Split('\0'));
			}

			// Return raw hex for unsupported types
			if (!silent)
			{
				this.Log("Unsupported platform info type", info.ToString());
			}
			return BitConverter.ToString(infoBytes).Replace("-", "");
		}

		public Dictionary<string, string> GetNames()
		{
			// Get all OpenCL devices & platforms
			Dictionary<CLDevice, CLPlatform> devicesPlatforms = this.DevicesPlatforms;

			// Create dictionary for device names and platform names
			Dictionary<string, string> names = [];

			// Iterate over devices
			foreach (CLDevice device in devicesPlatforms.Keys)
			{
				// Get device name
				string deviceName = this.GetDeviceInfo(device, DeviceInfo.Name, true) ?? "N/A";

				// Get platform name
				string platformName = this.GetPlatformInfo(devicesPlatforms[device], PlatformInfo.Name, true) ?? "N/A";

				// Add to dictionary
				names.Add(deviceName, platformName);
			}

			// Return names
			return names;
		}


		// UI
		public void FillDevicesCombo(ComboBox? comboBox = null, int selection = -1)
		{
			comboBox ??= this.DevicesCombo;
			if (comboBox == null)
			{
				this.Log("No combo box provided", "Cannot fill devices combo box");
				return;
			}

			// Get all OpenCL devices & platforms
			Dictionary<CLDevice, CLPlatform> devicesPlatforms = this.DevicesPlatforms;

			// Clear combo box
			comboBox.Items.Clear();

			// Add devices to combo box
			foreach (KeyValuePair<CLDevice, CLPlatform> device in devicesPlatforms)
			{
				string deviceName = this.GetDeviceInfo(device.Key, DeviceInfo.Name, true) ?? "N/A";
				string platformName = this.GetPlatformInfo(device.Value, PlatformInfo.Name, true) ?? "N/A";
				comboBox.Items.Add(deviceName + " (" + platformName + ")");
			}

			// Select selection if valid
			if (selection >= 0 && selection < comboBox.Items.Count)
			{
				comboBox.SelectedIndex = selection;
			}
			else
			{
				comboBox.SelectedIndex = -1;
			}
		}

		public List<string> GetInfoDeviceInfo(CLDevice? dev = null, bool raw = false, bool showMsgbox = false)
		{
			dev ??= this.DEV;
			if (dev == null)
			{
				return [];
			}

			List<string> infoList = [];
			List<string> descList =
				[
					"Name", "Vendor", "Vendor id", "Address Bits", "Global memory size", "Local memory size",
					"Cache memory size",
					"Compute units", "Clock frequency", "Max. buffer size", "OpenCLC version", "Version",
					"Driver version"
				];

			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.Name));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.Vendor));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.VendorId));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.AddressBits));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.GlobalMemorySize));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.LocalMemorySize));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.GlobalMemoryCacheSize));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.MaximumComputeUnits));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.MaximumClockFrequency));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.MaximumConstantBufferSize));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.OpenClCVersion));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.Version));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.DriverVersion));

			if (!raw)
			{
				for (int i = 0; i < infoList.Count; i++)
				{
					infoList[i] = descList[i] + " : '" + infoList[i] + "'";
				}
			}

			// Show message box if requested
			if (showMsgbox)
			{
				string msg = string.Join(Environment.NewLine, infoList);
				MessageBox.Show(msg, "OpenCL Device Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			// Return info list

			return infoList;
		}

		public List<string> GetInfoPlatformInfo(CLPlatform? plat = null, bool raw = false, bool showMsgbox = false)
		{
			plat ??= this.PLAT;
			if (plat == null)
			{
				return [];
			}

			List<string> infoList = [];
			List<string> descList =
				[
					"Name", "Vendor", "Version", "Profile", "Extensions"
				];

			infoList.Add(this.GetPlatformInfo(plat.Value, PlatformInfo.Name));
			infoList.Add(this.GetPlatformInfo(plat.Value, PlatformInfo.Vendor));
			infoList.Add(this.GetPlatformInfo(plat.Value, PlatformInfo.Version));
			infoList.Add(this.GetPlatformInfo(plat.Value, PlatformInfo.Profile));
			infoList.Add(this.GetPlatformInfo(plat.Value, PlatformInfo.Extensions));

			if (!raw)
			{
				for (int i = 0; i < infoList.Count; i++)
				{
					infoList[i] = descList[i] + " : '" + infoList[i] + "'";
				}
			}

			// Show message box if requested
			if (showMsgbox)
			{
				string msg = string.Join(Environment.NewLine, infoList);
				MessageBox.Show(msg, "OpenCL Platform Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			return infoList;
		}

		public void FillKernelsCombobox(ComboBox comboBox)
		{
			// Clear combo box
			comboBox.Items.Clear();

			if (this.KernelHandling == null)
			{
				this.Log("Kernel handling not initialized", "Cannot fill kernels combo box");
				return;
			}

			this.KernelHandling.FillKernelsCombobox(comboBox);
		}


		// Init context
		public void InitContext(int index = 0, bool silent = false)
		{
			// Dispose prev context
			this.Dispose(true);

			// Get all OpenCL devices & platforms
			Dictionary<CLDevice, CLPlatform> devicesPlatforms = this.DevicesPlatforms;

			// Check if index is valid
			if (index < 0 || index >= devicesPlatforms.Count)
			{
				if (!silent)
				{
					this.Log("Invalid device index", index.ToString());
				}
				return;
			}

			// Get device and platform
			this.DEV = devicesPlatforms.Keys.ElementAt(index);
			this.PLAT = devicesPlatforms.Values.ElementAt(index);

			// Create context
			this.CTX = CL.CreateContext(0, [this.DEV.Value], 0, IntPtr.Zero, out CLResultCode error);
			if (error != CLResultCode.Success || this.CTX == null)
			{
				if (!silent)
				{
					this.Log("Failed to create OpenCL context", error.ToString());
				}
				return;
			}

			// Init memory handling & fourier handling & kernel handling
			this.MemorRegister = new OpenClMemoryRegister(this.Repopath, this.CTX.Value, this.DEV.Value, this.PLAT.Value, this.LogList);
			this.KernelHandling = new OpenClKernelHandling(this.Repopath, this.MemorRegister, this.CTX.Value, this.DEV.Value, this.PLAT.Value, this.MemorRegister.QUE, this.LogList);

			// Set index
			this.INDEX = index;

			// Log
			if (!silent)
			{
				this.Log("Created OpenCL context", this.GetDeviceInfo(this.DEV, DeviceInfo.Name, silent) ?? "N/A");
			}
		}

		public void SelectDeviceLike(string deviceNameWildcard = "Intel")
		{
			// Get all OpenCL devices & platforms
			Dictionary<CLDevice, CLPlatform> devicesPlatforms = this.DevicesPlatforms;

			// Find device with name like deviceNameWildcard
			int index = -1;
			foreach (KeyValuePair<CLDevice, CLPlatform> device in devicesPlatforms)
			{
				string deviceName = this.GetDeviceInfo(device.Key, DeviceInfo.Name, true) ?? "N/A";
				if (deviceName.Contains(deviceNameWildcard))
				{
					index = Array.IndexOf(devicesPlatforms.Keys.ToArray(), device.Key);
					break;
				}
			}

			// Select device
			if (index >= 0)
			{
				this.DevicesCombo.SelectedIndex = index;
				// this.InitContext(index);
			}
			else
			{
				this.Log("OpenCL device not found", "No device found with name like '" + deviceNameWildcard + "'");
			}
		}

		public void FillPointers(ListBox listBox_pointers)
		{
			// Clear listbox
			listBox_pointers.Items.Clear();
			
			// Fill with memory pointers
			if (this.MemorRegister == null)
			{
				this.Log("Memory register not initialized", "Cannot fill pointers listbox");
				return;
			}

			// Fill handles with type
			foreach (ClMem mem in this.MemorRegister.Memory)
			{
				listBox_pointers.Items.Add(mem.IndexHandle.ToString("X16") + " - " + mem.ElementType.Name);
			}
		}





		// ----- ----- ----- PROTECTED METHODS ----- ----- ----- \\





	}
}
