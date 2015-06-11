using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BBitsCore
{
    using Hook;
    using System.IO;
    using System.Reflection;

    internal class BlackBits :  IDisposable
    {
        internal static volatile bool IsRunning = false;
        private static BlackBits _instance = null;
        private static Thread bbThread;
        public static void RunBlackBits()
        {
            if (IsRunning == true && _instance != null)
                return;

            BlackBits bcore = new BlackBits();

            bbThread = new Thread(new ThreadStart(() =>
            {
                using (BlackBits core = bcore)
                {
                    _instance = core;
                    IsRunning = true;
                    core.DoLoop();
                }
            }));
        }

        public static PluginLoader Loader { get; private set; }

        public int DesiredTPS { get; set; }

        public int Ticks { get; private set; }

        long lastTickTime;
        long tickTime;

        protected BlackBits()
        {
            IHook hook = Direct3D9.Instance;
            if (hook == null)
                hook = Direct3D11.Instance;
            if (hook == null)
                hook = Direct3D10.Instance;

            string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Loader = PluginLoader.LoadFromDirectory(appPath + @"\plugins");

            Direct3D9.OnReset += Loader.Reset;
            Direct3D9.OnEndScene += Loader.EndScene;

            Direct3D10.OnResize += Loader.Resize;
            Direct3D10.OnPresent += Loader.Present;

            Direct3D11.OnResize += Loader.Resize;
            Direct3D11.OnPresent += Loader.Present;

            lastTickTime = DateTime.UtcNow.Ticks;
            tickTime = 0L;
        }

        protected void DoLoop()
        {
            while (IsRunning)
            {
                DoTick();
            }
        }

        protected void DoTick()
        {
            long TimePerTick = (TimeSpan.TicksPerSecond / DesiredTPS);
            long currentTime = DateTime.UtcNow.Ticks;
            long deltaTime = currentTime - lastTickTime;

            if (deltaTime >= TimePerTick || deltaTime < 0L)
            {
                if (deltaTime >= TimeSpan.TicksPerSecond)
                {
                    // Cannot Keep  Up
                    deltaTime = TimePerTick;
                }

                if (deltaTime < 0L)
                {
                    //Time going backwards
                    deltaTime = 0L;
                }

                tickTime += deltaTime;
                lastTickTime = currentTime;
                while (tickTime >= TimePerTick)
                {
                    tickTime -= TimePerTick;
                    ++Ticks;
                    try
                    {
                        Loader.Update(Ticks);
                    }
                    catch
                    {
                        //Mask Exception
                    }
                }
            }

            Thread.Sleep(Math.Max(Convert.ToInt32((TimePerTick - TimePerTick) / TimeSpan.TicksPerMillisecond) - 1, 0));
        }

        public void Dispose()
        {
            Loader.Dispose();
            foreach(var hook in IHook.Hooks)
            {
                hook.Dispose();
            }
        }
    }
}
