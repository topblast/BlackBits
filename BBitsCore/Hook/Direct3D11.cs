using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using EasyHook;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
namespace BBitsCore.Hook
{
    sealed class Direct3D11 : DXGIBase
    {
        public delegate void OnResizeDelegate(Device device, SwapChain swapChain);
        public delegate void OnPresentDelegate(Device device, SwapChain swapChain);

        public static event OnResizeDelegate OnResize;
        public static event OnPresentDelegate OnPresent;

        private static Direct3D11 _instance = null;
        public static Direct3D11 Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Direct3D11();
                    if (!_instance.Initialize())
                        _instance = null;
                }
                return _instance;
            }
        }

        protected override bool CreateSwapChainAndDevice(SharpDX.DXGI.SwapChainDescription swapDesc, out ComObject device, out SharpDX.DXGI.SwapChain chain)
        {
            device = null;
            chain = null;

            if (NativeAPI.GetModuleHandle("d3d11.dll") == IntPtr.Zero)
                return false;

            Device dev = null;

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, swapDesc, out dev, out chain);

            device = dev;

            return device != null && chain != null;
        }

        public static void Resize(SwapChain swapChain, Device device, int bufferCount, int width, int height, Format format, SwapChainFlags flag)
        {
            if (OnResize != null)
                OnResize(device, swapChain);
        }

        public static void Present(SwapChain swapChain, Device device, int syncInterval, PresentFlags flags)
        {
            if (OnPresent != null)
                OnPresent(device, swapChain);
        }
    }
}
