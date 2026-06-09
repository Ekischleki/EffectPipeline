using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.types
{
    internal class GreyscaleImage : IInstance
    {
        public Type Type => Type.GreyscaleImage;
        internal int width;
        internal int height;
        internal float[] image;

        public RGBImage? ToImage()
        {
            return new RGBImage([this, this, this]);
        }
    }
}
