using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BBitsCore.Hook
{
    abstract class IHook : IDisposable
    {
        public abstract bool Initialize();

        public abstract void Dispose();

        protected static IntPtr[] GetVTblAddresses(IntPtr pointer, int numberOfMethods)
        {
            List<IntPtr> vtables = new List<IntPtr>();

            IntPtr vTable = Marshal.ReadIntPtr(pointer);
            for (int i = 0; i < numberOfMethods; i++)
                vtables.Add(Marshal.ReadIntPtr(vTable, i * IntPtr.Size));

            return vtables.ToArray();
        }

        protected static IntPtr[] GetVTblAddresses(IntPtr pointer, int startIndex, int numberOfMethods)
        {
            List<IntPtr> vtables = new List<IntPtr>();

            IntPtr vTable = Marshal.ReadIntPtr(pointer);
            for (int i = startIndex; i < startIndex + numberOfMethods; i++)
                vtables.Add(Marshal.ReadIntPtr(vTable, i * IntPtr.Size));

            return vtables.ToArray();
        }
    }
}
