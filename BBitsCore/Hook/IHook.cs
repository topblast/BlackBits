using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BBitsCore.Hook
{
    public abstract class IHook : IDisposable
    {
        internal static IList<IHook> Hooks = new List<IHook>();

        public IHook()
        {
            Hooks.Add(this);
        }

        public abstract bool Initialize();

        public virtual void Dispose()
        {
            Hooks.Remove(this);
        }

        public static IntPtr[] GetVTblAddresses(IntPtr pointer, int numberOfMethods)
        {
            List<IntPtr> vtables = new List<IntPtr>();

            IntPtr vTable = Marshal.ReadIntPtr(pointer);
            for (int i = 0; i < numberOfMethods; i++)
                vtables.Add(Marshal.ReadIntPtr(vTable, i * IntPtr.Size));

            return vtables.ToArray();
        }

        public static IntPtr[] GetVTblAddresses(IntPtr pointer, int startIndex, int numberOfMethods)
        {
            List<IntPtr> vtables = new List<IntPtr>();

            IntPtr vTable = Marshal.ReadIntPtr(pointer);
            for (int i = startIndex; i < startIndex + numberOfMethods; i++)
                vtables.Add(Marshal.ReadIntPtr(vTable, i * IntPtr.Size));

            return vtables.ToArray();
        }
    }
}
