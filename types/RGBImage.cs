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

        public int width { get; }
        public int height { get; }

        public float[] red { get; }
        public float[] green { get; }
        public float[] blue { get; }

        public RGBImage(GreyscaleImage[] channels)
        {
            if(channels.Length != 3)
            {
                throw new ArgumentException("Image needs to have three channels");
            }
            width = channels[0].width;
            height = channels[0].height;
            if (channels[1].width != width || channels[1].height != height || channels[2].width != width || channels[2].height != height)
            {
                throw new ArgumentException("Width and height of channels need to be the same");
            }
            red = channels[0].image;
            green = channels[1].image;
            blue = channels[2].image;
        }

        public RGBImage? ToImage()
        {
            return this;
        }

        public static RGBImage WhiteImage(int width, int height)
        {
            GreyscaleImage white = GreyscaleImage.PureWhite(width, height);
            return new RGBImage([white, white, white]);
        }

    }
}
