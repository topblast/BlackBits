using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BBitsCore
{
    using Hook;
    public class Program
    {
        private static bool initialize = false;
        public static int BlackBitsInitialize(String pwzArgument)
        {
            if (initialize)
                return 1;
            try
            {
                IHook hook = Direct3D9.Instance;
                if (hook == null)
                    hook = Direct3D11.Instance;
                if (hook == null)
                    hook = Direct3D10.Instance;
                
                string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); 
                PluginLoader.LoadFromDirectory(appPath + @"\plugins");
                initialize = true;
            } catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message + "\n\n" + e.StackTrace, "BlackBitsCore Error");
            }
            return 0;
        }

        static void Main(string[] args)
        {
            BlackBitsInitialize("");
        }
    }
}
