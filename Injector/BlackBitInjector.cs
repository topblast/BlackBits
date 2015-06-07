using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Injector
{
    class BlackBitInjector
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint dwFreeType);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, int size, int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        [DllImport("kernel32.dll")]
        static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

        [DllImport("kernel32", CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

        private static bool IsWow64Process(Process proc)
        {
            if (!Environment.Is64BitOperatingSystem)
                return true;

            IntPtr processHandle;
            bool retVal;
            
            try
            {
                processHandle = proc.Handle;
            }
            catch
            {
                return false; // access is denied to the process
            }

            return IsWow64Process(processHandle, out retVal) && retVal;
        }

        static string ApplicationPath
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        public static bool Inject(Process proc)
        {
            string BOOTSTRAP_DLL = (IsWow64Process(proc)) ? "BBitsBootstrap.dll" : "BBitsBootstrap64.dll";
            IntPtr hProc = OpenProcess(0x1F0FFF, false, proc.Id);

            if (hProc == null || hProc == IntPtr.Zero)
                return false;

            IntPtr fnLoadLibrary = GetProcAddress(GetModuleHandle("kernel32"), "LoadLibraryW");

            if (!ExecuteRemoteFunction(hProc, fnLoadLibrary, ApplicationPath + "\\" + BOOTSTRAP_DLL))
                return false;

            IntPtr baseAddress = GetRemoteModuleHandle(proc, BOOTSTRAP_DLL);
            Int64 offset = GetFunctionOffset(ApplicationPath + "\\" + BOOTSTRAP_DLL, "BlackBitsInjection");
            IntPtr fnBlackBitsInjection = IntPtr.Add(baseAddress, Convert.ToInt32(offset));

            ExecuteRemoteFunction(hProc, fnBlackBitsInjection, ApplicationPath);

            IntPtr fnFreeLibrary = GetProcAddress(GetModuleHandle("kernel32"), "FreeLibrary");
            CreateRemoteThread(hProc, IntPtr.Zero, 0, fnFreeLibrary, baseAddress, 0, IntPtr.Zero);

            CloseHandle(hProc);

            return true;
        }

        public static bool ExecuteRemoteFunction(IntPtr hProc, IntPtr remoteFunction, string argument)
        {
            if (remoteFunction == IntPtr.Zero)
                return false;
            IntPtr argAddress = IntPtr.Zero;
            byte[] buff = null;
            if (argument.Length > 0)
            {
                buff = Encoding.Unicode.GetBytes(argument);

                argAddress = VirtualAllocEx(hProc, IntPtr.Zero, buff.Length, 0x1000 | 0x2000, 4);

                if (argAddress == null || argAddress == IntPtr.Zero)
                    return false;

                if (!WriteProcessMemory(hProc, argAddress, buff, buff.Length, 0))
                    return false;
            }
            

            IntPtr hThread = CreateRemoteThread(hProc, IntPtr.Zero, 0, remoteFunction, argAddress, 0, IntPtr.Zero);

            if (hThread == null || hThread == IntPtr.Zero)
                return false;

            UInt32 wait = WaitForSingleObject(hThread, 0xFFFFFFFF);

            if (wait != 0x0)
                return false;

            if (buff != null)
                VirtualFreeEx(hProc, argAddress, buff.Length, 0x8000);

            uint exitCode = 0;
            GetExitCodeThread(hThread, out exitCode);

            CloseHandle(hThread);

            return true;
        }
        
        static IntPtr GetRemoteModuleHandle(Process proc, string moduleName)
        {
            for (int i = 0; i < proc.Modules.Count; i++)
            {
                ProcessModule mod = proc.Modules[i];
                if (mod.ModuleName == moduleName)
                    return mod.BaseAddress;
            }

            return IntPtr.Zero;
        }

        static Int64 GetFunctionOffset(string module, string functionName)
        {
            IntPtr hLoaded = LoadLibrary(module);

            IntPtr lpInject = GetProcAddress(hLoaded, functionName);

            Int64 offset = lpInject.ToInt64() - hLoaded.ToInt64();

            FreeLibrary(hLoaded);

            return offset;
        }
    }
}
