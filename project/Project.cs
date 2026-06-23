using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.project
{
    internal class Project
    {
        public int Nodes;
        public Connections Connections;
    }


    internal class NodeInformation
    {
        public string EffectSearch;
        public float x, y;
        public List<IPropertySave> props;
    }

    internal class Connections
    {

    }

    public interface IPropertySave { }
}
