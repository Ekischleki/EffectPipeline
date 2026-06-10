using EffectPipeline.types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.Effects
{
    internal class ImageSource : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [];

        public IEnumerable<(string, Type)> Outputs => [("Source Image", Type.RGBImage)];


        // Image reference data idek
        RGBImage imageData;

        public ImageSource(RGBImage _imageData)
        {
            imageData = _imageData;
        }


        public IInstance[] applyEffect(IInstance[] inputs)
        {
            if (inputs.Length != 0)
            {
                throw new ArgumentException("Input needs to be length 0");
            }

            return [imageData];
        }
    }
}
