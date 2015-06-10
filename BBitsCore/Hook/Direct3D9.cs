using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using SharpDX;
using SharpDX.Direct3D9;
using EasyHook;

namespace BBitsCore.Hook
{
    sealed class Direct3D9 : IHook
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate int IDirect3D9Device_EndSceneDelegate(IntPtr devicePtr);

        LocalHook endSceneHook;

        private Direct3D9()
        {
            endSceneHook = null;
        }

        private static Direct3D9 _instance = null;
        public static Direct3D9 Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Direct3D9();
                    if (!_instance.Initialize())
                        _instance = null;
                }
                return _instance;
            }
        }

        public override bool Initialize()
        {
            if (NativeAPI.GetModuleHandle("d3d9.dll") == IntPtr.Zero)
                return false;

            Dispose();
            Device device;
            List<IntPtr> vtable = new List<IntPtr>();
            const int D3D9_DEVICE_METHOD_COUNT = 119;

            using (Direct3D d3d = new Direct3D())
            {
                //using (var tempForm = new System.Windows.Forms.Form())
                {
                    using (device = new Device(d3d, 0, DeviceType.NullReference, IntPtr.Zero, CreateFlags.SoftwareVertexProcessing, new PresentParameters()))
                    {
                        vtable.AddRange(IHook.GetVTblAddresses(device.NativePointer, D3D9_DEVICE_METHOD_COUNT));
                    }
                }
            }

            endSceneHook = LocalHook.Create(vtable[42], new IDirect3D9Device_EndSceneDelegate(EndScene), this);
            endSceneHook.ThreadACL.SetExclusiveACL(new int[1]);
            return true;
        }        

        int EndScene(IntPtr devicePtr)
        {
            Device device = (Device)devicePtr;
            device.Clear(ClearFlags.Target, new ColorBGRA(0xFFFF0000), 0, 0, new Rectangle[] { new Rectangle(100, 100, 100, 100) });

            using (var ren = new Renderers.Direct3D9(device))
            {

            }
            
            PluginLoader.D3D9_Endscene(devicePtr);


            device.EndScene();
            
            return SharpDX.Result.Ok.Code;
        }
        
        public override void Dispose()
        {
            if (endSceneHook != null)
            {
                endSceneHook.Dispose();
                endSceneHook = null;
            }
        }
    }
}
