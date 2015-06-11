using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin
{
    public class TestPlugin :BBitsCore.IPlugin
    {
        public string Name { get { return "TestPlugin"; } }

        public string Identifier { get { return "XXXX____TEST____XXXX"; } }

        public string Author { get { return "Topblast"; } }

        public void OnLoad()
        {
            System.Windows.Forms.MessageBox.Show(this.Name + " has loaded!", this.Identifier);
        }

        public void OnUpdate(bool enabled)
        {

        }

        public void OnEnable()
        {
            System.Windows.Forms.MessageBox.Show(this.Name + " is Enable!", this.Identifier);
        }

        public void OnDisable()
        {
            System.Windows.Forms.MessageBox.Show(this.Name + " was Disabled!", this.Identifier);
        }
        
        public void D3D9_Endscene(IntPtr pDevice)
        {
        }
    }
}
