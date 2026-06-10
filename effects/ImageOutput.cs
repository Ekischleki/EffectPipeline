using EffectPipeline.types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.Effects
{
    internal class ImageOutput : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Output Image", Type.RGBImage)];

        public IEnumerable<(string, Type)> Outputs => [];


        public IInstance[] applyEffect(IInstance[] inputs)
        {
            if (inputs.Length != 1)
            {
                throw new ArgumentException("Input needs to be length 1");
            }

            return [];
        }
    }
}
