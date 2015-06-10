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

        public static void Present(IntPtr pSwapChain, Device device, int syncInterval, PresentFlags flags)
        {
            var ren = device.ImmediateContext.OutputMerger.GetRenderTargets(1)[0];
            device.ImmediateContext.ClearRenderTargetView(ren, new Color4(0xFFFFFFFF));
        }
    }
}
