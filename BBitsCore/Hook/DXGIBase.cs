using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EasyHook;
using SharpDX;
using SharpDX.DXGI;
using Device10_0 = SharpDX.Direct3D10.Device;
using Device10_1 = SharpDX.Direct3D10.Device1;
using Device11_0 = SharpDX.Direct3D11.Device;
using System.Runtime.InteropServices;

namespace BBitsCore.Hook
{
    abstract class DXGIBase : IHook
    {
        LocalHook swapResizeHook;
        LocalHook swapPresentHook;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate int DXGI_SwapResizeDelegate(IntPtr pSwapChain, int bufferCount, int width, int height, Format format, SwapChainFlags flag);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate int DXGI_SwapPresentDelegate(IntPtr pSwapChain, int syncInterval, PresentFlags flags);

        protected DXGIBase()
        {
            swapResizeHook = null;
            swapPresentHook = null;
        }

        public override bool Initialize()
        {
            Dispose();

            List<IntPtr> vtable = new List<IntPtr>();
            using (var form = new System.Windows.Forms.Form())
            {
                using (var factory = new Factory())
                {
                    var desc = new SwapChainDescription
                    {
                        BufferCount = 1,
                        Flags = SharpDX.DXGI.SwapChainFlags.None,
                        IsWindowed = true,
                        ModeDescription = new SharpDX.DXGI.ModeDescription(100, 100, new Rational(60, 1), SharpDX.DXGI.Format.R8G8B8A8_UNorm),
                        OutputHandle = form.Handle,
                        SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                        SwapEffect = SharpDX.DXGI.SwapEffect.Discard,
                        Usage = SharpDX.DXGI.Usage.RenderTargetOutput
                    };
                    bool failed = true;
                    ComObject device = null;
                    SwapChain swapChain = null;
                    if (CreateSwapChainAndDevice(desc, out device, out swapChain))
                    {
                        vtable.AddRange(IHook.GetVTblAddresses(swapChain.NativePointer, 18));
                        failed = false;
                    }
                    if (device != null)
                        device.Dispose();
                    if (swapChain != null)
                        swapChain.Dispose();
                    if (failed)
                        return false;
                }
            }

            swapResizeHook = LocalHook.Create(vtable[13], new DXGI_SwapResizeDelegate(ResizeBuffer), this);
            swapPresentHook = LocalHook.Create(vtable[8], new DXGI_SwapPresentDelegate(Present), this);

            swapResizeHook.ThreadACL.SetExclusiveACL(new int[1]);
            swapPresentHook.ThreadACL.SetExclusiveACL(new int[1]);

            return true;
        }

        protected abstract bool CreateSwapChainAndDevice(SwapChainDescription swapDesc, out ComObject device, out SwapChain chain);

        private int ResizeBuffer(IntPtr pSwapChain, int bufferCount, int width, int height, Format format, SwapChainFlags flag)
        {
            SwapChain swapChain = (SwapChain)pSwapChain;

            swapChain.ResizeBuffers(bufferCount, width, height, format, flag);
            return SharpDX.Result.Ok.Code;
        }

        private int Present(IntPtr pSwapChain, int syncInterval, PresentFlags flags)
        {
            //System.Windows.Forms.MessageBox.Show("Device Testing");
            SwapChain swapChain = (SwapChain)pSwapChain;
            DeviceGroup group;

            if (!Devices.ContainsKey(pSwapChain))
            {
                group = new DeviceGroup();

                try
                {
                    group = new DeviceGroup()
                    {
                        Device = swapChain.GetDevice<Device10_0>(),
                        Version = DirectXVersion.DirectX10
                    };
                }
                catch
                {
                    try
                    {
                        group = new DeviceGroup()
                        {
                            Device = swapChain.GetDevice<Device11_0>(),
                            Version = DirectXVersion.DirectX11
                        };
                    }
                    catch
                    {
                    }
                }
                //if (group.Device == null)
                //{
                //    group = new DeviceGroup()
                //    {
                //        Device = swapChain.GetDevice<Device10_1>(),
                //        Version = DirectXVersion.DirectX10_1
                //    };
                //}

                if (group.Device != null && group.Version != DirectXVersion.Unknown)
                    Devices.Add(pSwapChain, group);
            }

            if (Devices.TryGetValue(pSwapChain, out group))
            {
                switch (group.Version)
                {
                    case DirectXVersion.DirectX11:
                        Direct3D11.Present(pSwapChain, group.Device as Device11_0, syncInterval, flags);
                        break;
                    case DirectXVersion.DirectX10_1:
                    case DirectXVersion.DirectX10:
                        Direct3D10.Present(pSwapChain, group.Device as Device10_0, syncInterval, flags);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            swapChain.Present(syncInterval, flags);
            return SharpDX.Result.Ok.Code;
        }

        public override void Dispose()
        {
            if (swapResizeHook != null)
            {
                swapResizeHook.Dispose();
                swapResizeHook = null;
            }
            if (swapPresentHook != null)
            {
                swapPresentHook.Dispose();
                swapPresentHook = null;
            }
        }

        private enum DirectXVersion
        {
            Unknown,
            DirectX11,
            DirectX10_1,
            DirectX10
        }

        private struct DeviceGroup
        {
            public DirectXVersion Version;

            public ComObject Device;
        }
        private static IDictionary<IntPtr, DeviceGroup> _devices = new System.Collections.Generic.Dictionary<IntPtr, DeviceGroup>();

        private static IDictionary<IntPtr, DeviceGroup> Devices
        {
            get { return _devices; }
            set { _devices = value; }
        }
    }
}
