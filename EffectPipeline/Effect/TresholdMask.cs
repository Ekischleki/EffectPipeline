using EffectPipeline.GameObjects.GUIElements;
using EffectPipeline.types;
using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.Effects
{
    //Maybe turn this into an "Equal split" which creates an n-Mask where each part of the mask has equal representation
    internal class TresholdMask : IEffect
    {
        public string Title => "Treshold Mask";

        public IEnumerable<(string, Type)> Inputs => [("Image", typeof(GreyscaleImage))];

        public IEnumerable<(string, Type)> Outputs => [("Mask", typeof(Mask))];

        public Property[] Properties => [new FloatInputProperty("Treshold") {
            Value = .5f,
            Min = 0f,
            Max = 1f,
        }];

        public async Task<IInstance[]> applyEffect(IInstance?[] inputs, IPropertyState[] properties)
        {
            var image = (GreyscaleImage?)inputs[0];

            float treshold = ((FloatInputPropertyState)properties[0]).Value;
            if (image == null || image.image.Length == 0)
            {
                return [new Mask(1, new int[0], 0, 0)];
            }

            int width = image.width;
            int height = image.height;

            int[] maskLayers = new int[width * height];

            for (int i = 0; i < width * height; i++)
            {
                maskLayers[i] = image.image[i] > treshold ? 1 : 0;
            }


            return [new Mask(2, maskLayers, image.width, image.height)];
        }
    }

    internal class TesholdMaskSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["treshold", "quantize", "mask"];
        public IEffect CreateEffect() => new TresholdMask();
    }
}
