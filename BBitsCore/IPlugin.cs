using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBitsCore
{
    public interface IPlugin
    {
        string Identifier { get; }

        string Name { get; }

        void OnLoad();

        void OnUpdate(bool enabled);
        
        void
    }
}
