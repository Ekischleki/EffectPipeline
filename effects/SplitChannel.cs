using EffectPipeline.types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.Effects
{
    internal class SplitChannel : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Image", Type.RGBImage)];

        public IEnumerable<(string, Type)> Outputs => [("Channel 0", Type.GreyscaleImage), ("Channel 1", Type.GreyscaleImage), ("Channel 2", Type.GreyscaleImage)];

        public IInstance[] applyEffect(IInstance[] inputs)
        {
            if(inputs.Length != 1)
            {
                throw new ArgumentException("Input needs to be length 1");
            }
            RGBImage image = (RGBImage)inputs[0];
            return image.channels;
        }
    }
}
