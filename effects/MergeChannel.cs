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
    internal class MergeChannel : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Channel 0", Type.GreyscaleImage), ("Channel 1", Type.GreyscaleImage), ("Channel 2", Type.GreyscaleImage)];

        public IEnumerable<(string, Type)> Outputs => [("Image", Type.RGBImage)];

        public GameObject[] Properties => [DropdownProperty.ColorspaceDropdown];

        public IInstance[] applyEffect(IInstance[] inputs, GameObject[] properties)
        {
            bool set = false;
            int width = 0;
            int height = 0;
            foreach (var input in inputs)
            {
                if(input is GreyscaleImage img) {
                    if(set && (width != img.width || height != img.height))
                    {
                        throw new ArgumentException("Channel sizes must match");
                    }
                    set = true;
                    width = img.width;
                    height = img.height;
                }
            }
            if(inputs.Length != 3)
            {
                throw new ArgumentException("Input needs to be length 3");
            }
            for (int i = 0; i < inputs.Length; i++)
            {
                if(inputs[i] == null)
                {
                    inputs[i] = new GreyscaleImage(width, height, new float[width * height]);
                }
            }
            
            RGBImage image = new RGBImage(inputs.Cast<GreyscaleImage>().ToArray());
            return [image];
            
            
        }
    }
}
