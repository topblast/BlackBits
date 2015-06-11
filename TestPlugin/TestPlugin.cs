using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin
{
    using SharpDX;
    using SharpDX.DXGI;
    using SharpDX.Direct3D9;
    using SharpDX.Direct3D11;
    using Surface = SharpDX.DXGI.Surface;
    using SwapChain = SharpDX.DXGI.SwapChain;
    using Device9 = SharpDX.Direct3D9.Device;
    using Device10 = SharpDX.Direct3D10.Device;
    using Device11 = SharpDX.Direct3D11.Device;
    using SharpDX.Direct2D1;

    public class TestPlugin :BBitsCore.IPlugin
    {
        public string Name { get { return "TestPlugin"; } }

        public string Author { get { return "Topblast"; } }

        private SharpDX.Direct2D1.Factory Factory;

        private SharpDX.DirectWrite.Factory DWFactory;
        private SharpDX.DirectWrite.TextFormat Format;

        public void OnLoad()
        {
            System.Windows.Forms.MessageBox.Show("Hey, TestPlugin was loaded into BlackBits.", Name);
            Factory = new SharpDX.Direct2D1.Factory();
            DWFactory = new SharpDX.DirectWrite.Factory();
            Format = new SharpDX.DirectWrite.TextFormat(DWFactory, "Arial", 20);
        }

        public void OnUpdate(bool enabled, int ticks)
        { }

        public void OnEnable()
        { }

        public void OnDisable()
        { }
        public void D3D9_Endscene(SharpDX.Direct3D9.Device device)
        {

            using (SharpDX.Direct3D9.Font font = new SharpDX.Direct3D9.Font(device, new SharpDX.Direct3D9.FontDescription(){
                FaceName = "Arial",
                Height = 20,
                CharacterSet = SharpDX.Direct3D9.FontCharacterSet.Ansi,
                OutputPrecision = SharpDX.Direct3D9.FontPrecision.TrueType,
                PitchAndFamily = SharpDX.Direct3D9.FontPitchAndFamily.Default,
                Quality = SharpDX.Direct3D9.FontQuality.ClearType,
                Weight = SharpDX.Direct3D9.FontWeight.Bold}))
            {
                font.DrawText(null, "BlackBits Plugin: " + Name + "\n By " + Author, 10, 10, new ColorBGRA(0xFFFF0000));
            }
        }

        public void D3D9_Reset(SharpDX.Direct3D9.Device device)
        { }

        public void D3D10_Present(SharpDX.Direct3D10.Device device, SwapChain swapChain)
        {
            using (SharpDX.Direct3D10.Font font = new SharpDX.Direct3D10.Font(device, new SharpDX.Direct3D10.FontDescription()
                {
                    FaceName = "Arial",
                    Height = 20,
                    CharacterSet = SharpDX.Direct3D10.FontCharacterSet.Ansi,
                    OutputPrecision = SharpDX.Direct3D10.FontPrecision.TrueType,
                    PitchAndFamily = SharpDX.Direct3D10.FontPitchAndFamily.Default,
                    Quality = SharpDX.Direct3D10.FontQuality.ClearType,
                    Weight = SharpDX.Direct3D10.FontWeight.Bold
                }))
            {
                font.DrawText(null, "BlackBits Plugin: " + Name + "\n By " + Author, 10, 10, new Color4(0xFF0000FF));
            }
        }

        public void D3D10_Resize(SharpDX.Direct3D10.Device device, SwapChain swapChain)
        { }

        public void D3D11_Present(SharpDX.Direct3D11.Device device, SwapChain swapChain)
        {
            //Dx11 Font is annoying...
        }

        public void D3D11_Resize(SharpDX.Direct3D11.Device device, SwapChain swapChain)
        { }

        public void Dispose()
        { }
    }
}
