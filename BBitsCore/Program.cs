﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BBitsCore
{
    public class Program
    {
        public static int BlackBitsInitialize(String pwzArgument)
        {
            string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            PluginLoader.LoadFromDirectory(appPath + @"\plugins");

            return 0;
        }

        static void Main(string[] args)
        {
            BlackBitsInitialize("");
        }
    }
}
