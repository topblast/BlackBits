using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Device9 = SharpDX.Direct3D9.Device;
using Device10 = SharpDX.Direct3D10.Device;
using Device11 = SharpDX.Direct3D11.Device;


namespace BBitsCore
{
    class PluginLoader : IDisposable
    {
        public ICollection<PluginContainer> Plugins { get; protected set; }

        protected PluginLoader()
        {
            Plugins = new List<PluginContainer>();
        }

        public void LoadPlugin(IPlugin plugin)
        {
            plugin.OnLoad();
            lock(Plugins)
            {
                Plugins.Add(new PluginContainer(plugin));
            }
        }

        public void Update(int ticks)
        {
            lock (Plugins)
            {
                Parallel.ForEach(Plugins, (container) => { container.Update(ticks); });
            }            
        }

        public void Reset(Device9 device)
        {
            lock (Plugins)
            {
                foreach(var con in Plugins)
                {
                    con.Plugin.D3D9_Reset(device);
                }
            }
        }

        public void EndScene(Device9 device)
        {
            lock (Plugins)
            {
                foreach (var con in Plugins)
                {
                    con.Plugin.D3D9_Endscene(device);
                }
            }
        }

        public void Resize(Device10 device)
        {
            lock (Plugins)
            {
                foreach (var con in Plugins)
                {
                    con.Plugin.D3D10_Resize(device);
                }
            }
        }

        public void Present(Device10 device)
        {
            lock (Plugins)
            {
                foreach (var con in Plugins)
                {
                    con.Plugin.D3D10_Present(device);
                }
            }
        }

        public void Resize(Device11 device)
        {
            lock (Plugins)
            {
                foreach (var con in Plugins)
                {
                    con.Plugin.D3D11_Resize(device);
                }
            }
        }

        public void Present(Device11 device)
        {
            lock (Plugins)
            {
                foreach (var con in Plugins)
                {
                    con.Plugin.D3D11_Present(device);
                }
            }
        }

        public void Dispose()
        {
            lock (Plugins)
            {
                foreach (var con in Plugins)
                    con.Dispose();
            }
        }

        public class PluginContainer : IDisposable
        {
            public IPlugin Plugin { get; private set; }

            private bool _enabled;
            public bool IsEnabled
            {
                get { return _enabled; }
                set
                {
                    if (_enabled == value)
                        return;

                    _enabled = value;

                    if (_enabled)
                        Plugin.OnEnable();
                    else
                        Plugin.OnDisable();
                }
            }

            public PluginContainer(IPlugin plugin, bool enabled = true)
            {
                Plugin = plugin;
                IsEnabled = enabled;
            }

            public void Update(int ticks)
            {
                Plugin.OnUpdate(_enabled, ticks);
            }

            public void Dispose()
            {
                Plugin.Dispose();
            }
        }

        public static PluginLoader LoadFromDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                throw new IOException("Directory does not exist");

            var loader = new PluginLoader();

            string[] dllFiles = Directory.GetFiles(directory, "*.dll");
            Parallel.ForEach(dllFiles, (pluginFile) =>
            {
                AssemblyName an = AssemblyName.GetAssemblyName(pluginFile);
                Assembly assembly = Assembly.Load(an);
                if (assembly == null)
                    return;

                var types = assembly.GetTypes().Where(s => s.GetInterface(typeof(IPlugin).FullName) != null);

                foreach (var type in types)
                {
                    IPlugin instance = Activator.CreateInstance(type) as IPlugin;
                    if (instance != null)
                        loader.LoadPlugin(instance);
                }
            });

            return loader;
        }
    }
}
