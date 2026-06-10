using EffectPipeline.types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.Effects
{
    internal class MergeChannel : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Channel 0", Type.GreyscaleImage), ("Channel 1", Type.GreyscaleImage), ("Channel 2", Type.GreyscaleImage)];

        public IEnumerable<(string, Type)> Outputs => [("Image", Type.RGBImage)];

        public IInstance[] applyEffect(IInstance[] inputs)
        {
            if(inputs.Length != 3)
            {
                throw new ArgumentException("Input needs to be length 3");
            }

            RGBImage image = new RGBImage((GreyscaleImage[])inputs);
            return [image];
        }
    }
}
