using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.types
{
    public class GreyscaleImage : RGBImage
    {
        public int width { get; }
        public int height { get; }
        public float[] image { get; }

        public GreyscaleImage(int _width, int _height, float[] _image) : base(_width, _height, _image, _image, _image)
        {
            if (_image.Length != _width * _height)
            {
                throw new ArgumentException("Image array not of the right length");
            }


            width = _width;
            height = _height;

            image = _image;
        }

        public RGBImage? ToImage()
        {
            return new RGBImage([this, this, this]);
        }


        public static GreyscaleImage PureWhite(int width, int height)
        {
            float[] whiteArray = new float[width * height];
            for (int i = 0; i < width * height; i++) whiteArray[i] = 1;

            return new GreyscaleImage(width, height, whiteArray);
        }

        
    }
}
