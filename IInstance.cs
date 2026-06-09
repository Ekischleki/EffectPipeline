using EffectPipeline.types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline
{
    internal interface IInstance
    {
        public Type Type { get; }
        public RGBImage? ToImage();
    }
}
