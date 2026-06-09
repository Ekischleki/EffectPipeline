using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.types
{
    internal class RGBImage : IInstance
    {
        public Type Type => Type.RGBImage;
        internal GreyscaleImage[] channels;
        public RGBImage(GreyscaleImage[] channels)
        {
            if(channels.Length != 3)
            {
                throw new ArgumentException("Image needs to have three channels");
            }
            int width = channels[0].width;
            int height = channels[0].height;
            if (channels[1].width != width || channels[1].height != height || channels[2].width != width || channels[2].height != height)
            {
                throw new ArgumentException("Width and height of channels need to be the same");
            }
            this.channels = channels;
        }

        public RGBImage? ToImage()
        {
            return this;
        }
    }
}
