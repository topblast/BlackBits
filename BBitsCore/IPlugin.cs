using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;
using Device9 = SharpDX.Direct3D9.Device;
using Device10 = SharpDX.Direct3D10.Device;
using Device11 = SharpDX.Direct3D11.Device;

namespace BBitsCore
{
    public interface IPlugin : IDisposable
    {
        string Name { get; }

        string Author { get; }

        void OnLoad();

        void OnUpdate(bool enabled, int ticks);

        void OnEnable();

        void OnDisable();

        void D3D9_Endscene(Device9 device);

        void D3D9_Reset(Device9 device);

        void D3D10_Present(Device10 device, SwapChain swapChain);

        void D3D10_Resize(Device10 device, SwapChain swapChain);

        void D3D11_Present(Device11 device, SwapChain swapChain);

        void D3D11_Resize(Device11 device, SwapChain swapChain);
    }
}
