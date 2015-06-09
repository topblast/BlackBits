using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EasyHook;
using SharpDX;
using SharpDX.DXGI;
using System.Runtime.InteropServices;

namespace BBitsCore.Hook
{
    sealed class DXGIHook : IHook
    {
        LocalHook swapResizeHook;
        LocalHook swapPresentHook;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate int DXGI_SwapResizeDelegate(IntPtr pSwapChain, int bufferCount, int width, int height, Format format, SwapChainFlags flag);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate int DXGI_SwapPresentDelegate(IntPtr pSwapChain, int syncInterval, PresentFlags flags);

        private DXGIHook()
        {
            swapResizeHook = null;
            swapPresentHook = null;
        }

        private static DXGIHook _instance = null;
        public static DXGIHook Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DXGIHook();
                    _instance.Initialize();
                }
                return _instance;
            }
        }

        public override bool Initialize()
        {
            Dispose();

            List<IntPtr> vtable = new List<IntPtr>();
            try
            {
                using (var factory = new Factory())
                {
                    using (var swapChain = new SwapChain(factory, factory.GetAdapter(0), new SwapChainDescription()))
                    {
                        vtable.AddRange(IHook.GetVTblAddresses(swapChain.NativePointer, 18));
                    }
                }
            }
            catch
            {
                return false;
            }

            swapResizeHook = LocalHook.Create(vtable[13], new DXGI_SwapResizeDelegate(ResizeBuffer), this);
            swapPresentHook = LocalHook.Create(vtable[8], new DXGI_SwapPresentDelegate(Present), this);

            swapResizeHook.ThreadACL.SetExclusiveACL(new int[1]);
            swapPresentHook.ThreadACL.SetExclusiveACL(new int[1]);

            return false;
        }

        private int ResizeBuffer(IntPtr pSwapChain, int bufferCount, int width, int height, Format format, SwapChainFlags flag)
        {
            SwapChain swapChain = (SwapChain)pSwapChain;
            swapChain.ResizeBuffers(bufferCount, width, height, format, flag);
            return SharpDX.Result.Ok.Code;
        }

        private int Present(IntPtr pSwapChain, int syncInterval, PresentFlags flags)
        {
            SwapChain swapChain = (SwapChain)pSwapChain;
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
    }
}
