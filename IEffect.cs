using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline
{
    internal interface IEffect
    {
        public IEnumerable<(string, Type)> Inputs { get; }
        public IEnumerable<(string, Type)> Outputs { get; }
        public IInstance[] applyEffect(IInstance[] inputs);
    }

    enum Type
    {
        RGBImage,
        GreyscaleImage,
    }
}
