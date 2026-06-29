using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.types
{
    public class Mask : GreyscaleImage
    {
        internal readonly int width;
        internal readonly int height;
        internal readonly int layer_count;
        internal readonly int[] layers;

        public Mask(int layer_count, int[] layers, int width, int height) : base(width, height, layers.Select(i => (float)i / (float)(layer_count - 1)).ToArray())
        {
            this.layer_count = layer_count;
            this.layers = layers;
            this.width = width;
            this.height = height;
        }
    }
}
