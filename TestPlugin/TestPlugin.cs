using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin
{
    using BBitsCore;
    public class TestPlugin : IPlugin
    {
        public string Name { get { return "TestPlugin"; } }

        public string Identifier { get { return "XXXX____TEST____XXXX"; } }

        public void OnLoad()
        {
            System.Windows.Forms.MessageBox.Show("Test Plugin has loaded!", this.Identifier);
        }
    }
}
