using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.types
{
    public interface IInstance
    {
        public Type Type { get; }
        public bool SupportInto(Type type);
        public IInstance Into(Type type);
        public RGBImage? ToImage();
    }
}
