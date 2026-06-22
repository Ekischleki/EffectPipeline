using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.types
{
    public class Mask : IInstance
    {
        internal readonly int width;
        internal readonly int height;
        internal readonly int layer_count;
        internal readonly int[] layers;

        public Mask(int layer_count, int[] layers, int width, int height)
        {
            this.layer_count = layer_count;
            this.layers = layers;
            this.width = width;
            this.height = height;
        }

        public Type Type => Type.Mask;


        private GreyscaleImage ToGreyscale()
        {
            var img = layers.Select(i => (float)i / (float)(layer_count -1)).ToArray();
            return new(width, height, img);
        }

        public IInstance Into(Type type)
        {
            switch (type)
            {
                case Type.GreyscaleImage:
                    return ToGreyscale();
                case Type.RGBImage:
                    return ToImage()!;
                default:
                    throw new NotImplementedException();
            }
        }

        public bool SupportInto(Type type) => type switch { Type.RGBImage => true, Type.GreyscaleImage => true, _ => false };

        public RGBImage? ToImage()
        {
            return ToGreyscale().ToImage();
        }
    }
}
