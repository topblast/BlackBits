using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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
                BlackBits.RunBlackBits();
                initialize = true;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message + "\n\n" + e.StackTrace, "BlackBitsCore Error");
                return 2;
            }
            return 0;
        }

        static void Main(string[] args)
        {
            BlackBitsInitialize("");
        }
    }
}
