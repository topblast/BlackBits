using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace BBitsCore
{
    class PluginLoader
    {
        private static ICollection<IPlugin> plugins = new List<IPlugin>();

        public static void LoadFromDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                throw new IOException("Directory does not exist");

            string[] dllFiles = Directory.GetFiles(directory, "*.dll");
            foreach(string plugin in dllFiles)
            {
                AssemblyName an = AssemblyName.GetAssemblyName(plugin);
                Assembly assembly = Assembly.Load(an);
                if (assembly == null)
                    continue;

                var types = assembly.GetTypes().Where(s => s.GetInterface(typeof(IPlugin).FullName) != null);

                foreach (var type in types)
                {
                    IPlugin instance = Activator.CreateInstance(type) as IPlugin;
                    if (instance != null)
                        LoadPlugin(instance);
                }
            }
        }

        private static void LoadPlugin(IPlugin plugin)
        {
            plugin.OnLoad();
            plugins.Add(plugin);
        }
    }
}
