using EffectPipeline.gameObjects.GUI_Elements;
using EffectPipeline.types;
using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.effects
{
    //Maybe turn this into an "Equal split" which creates an n-Mask where each part of the mask has equal representation
    internal class TwoSplit : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Image", Type.GreyscaleImage)];

        public IEnumerable<(string, Type)> Outputs => [("Mask", Type.Mask)];

        public GameObject[] Properties => [new NumberInputProperty() {
            Value = 2,
            Min = 2,
            Max = 100,
        }];

        public IInstance[] applyEffect(IInstance?[] inputs, GameObject[] properties)
        {
            var image = (GreyscaleImage?)inputs[0];
            var split_amount = ((NumberInputProperty)properties[0]).Value;
            if (image == null)
            {
                return [new Mask(1, new int[0], 0, 0)];
            }
            var ordered_image = image.image.Order().ToArray();
            var groups = new float[split_amount - 1];
            for (int i = 1; i < split_amount; i++)
            {
                groups[i - 1] = ordered_image[(ordered_image.Length * i) / (split_amount)];
            }
            var mask = new int[image.image.Length];
            for (int i = 0; i < image.image.Length; i++)
            {
                int group = groups.Length;
                for (int j = 0; j < groups.Length; j++)
                {
                    if (groups[j] > image.image[i])
                    {
                        group = j;
                        break;
                    }
                }
                mask[i] = group;
            }
            return [new Mask(split_amount, mask, image.width, image.height)];
        }
    }
}
