using EffectPipeline.gameObjects.GUI_Elements;
using EffectPipeline.types;
using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.Effects
{
    internal class SplitChannel : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Image", Type.RGBImage)];

        public IEnumerable<(string, Type)> Outputs => [("Channel 0", Type.GreyscaleImage), ("Channel 1", Type.GreyscaleImage), ("Channel 2", Type.GreyscaleImage)];

        public GameObject[] Properties => [DropdownProperty.ColorspaceDropdown];

        public IInstance[] applyEffect(IInstance[] inputs, GameObject[] properties)
        {
            if(inputs.Length != 1)
            {
                throw new ArgumentException("Input needs to be length 1");
            }
            RGBImage? image = (RGBImage)inputs[0];
            if(image == null)
            {
                return [
                    new GreyscaleImage(0, 0, []),
                    new GreyscaleImage(0, 0, []),
                    new GreyscaleImage(0, 0, []),
                ];
            }
            return [new GreyscaleImage(image.width, image.height, image.red),
                    new GreyscaleImage(image.width, image.height, image.green),
                    new GreyscaleImage(image.width, image.height, image.blue),  ];
        }
    }
}
