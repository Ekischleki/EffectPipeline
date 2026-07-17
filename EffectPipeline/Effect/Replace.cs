using EffectPipeline.types;
using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.Effects
{
    internal class Replace : IEffect
    {
        public string Title => "Replace";

        public IEnumerable<(string, Type)> Inputs => [("Image a", typeof(RGBImage)), ("Image b", typeof(RGBImage)), ("Mask", typeof(Mask))];

        public IEnumerable<(string, Type)> Outputs => [("Merged image", typeof(RGBImage))];

        public Property[] Properties => [];

        public async Task<IInstance[]> applyEffect(IInstance?[] inputs, IPropertyState[] properties)
        {
            var imageA = (RGBImage?)inputs[0];
            var imageB = (RGBImage?)inputs[1];
            var mask = (Mask?)inputs[2];
            if (imageA == null || imageB == null || mask == null)
            {
                return [new RGBImage(0, 0, [], [], [])];
            }
            if(mask.layer_count != 2)
            {
                throw new Exception("Mask needs to have exactly 2 layers for a split");
            }
            if (imageA.width != imageB.width || imageA.height != imageB.height || imageA.width != mask.width || imageA.height != mask.height)
            {
                throw new Exception("Parameters need to share length");
            }
            var r = new float[imageA.red.Length];
            var g = new float[imageA.red.Length];
            var b = new float[imageA.red.Length];

            for (var i = 0; i < imageA.red.Length; i++)
            {
                r[i] = mask.layers[i] == 0 ? imageA.red[i] : imageB.red[i];
                g[i] = mask.layers[i] == 0 ? imageA.green[i] : imageB.green[i];
                b[i] = mask.layers[i] == 0 ? imageA.blue[i] : imageB.blue[i];
            }
            return [new RGBImage(imageA.width, imageA.height, r, g, b)];
        }
    }
    internal class ReplaceSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["replace", "fuse", "merge", "image", "mask"];
        public IEffect CreateEffect() => new Replace();
    }
}
