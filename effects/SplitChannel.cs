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


        private float[][] ToColorChannels(RGBImage image, DropdownProperty.Colorspace colorspace)
        {
            switch (colorspace)
            {
                case DropdownProperty.Colorspace.Rgb:
                    return [image.red, image.green, image.blue];
                case DropdownProperty.Colorspace.Hsv:
                    var colors = image.ToColors();
                    return [colors.AsEnumerable().Select(col => col.GetHue() / 360f).ToArray(),
                        colors.AsEnumerable().Select(col => col.GetSaturation()).ToArray(),
                        colors.AsEnumerable().Select(col => col.GetBrightness()).ToArray()];
                default:
                    throw new NotImplementedException();
            }
        }


        public IInstance[] applyEffect(IInstance[] inputs, GameObject[] properties)
        {
            DropdownProperty colorspaceDropdown = (DropdownProperty)properties[0];
            var colorspace = (DropdownProperty.Colorspace)colorspaceDropdown.Selected;
            RGBImage? image = (RGBImage)inputs[0];
            if(image == null)
            {
                return [
                    new GreyscaleImage(0, 0, []),
                    new GreyscaleImage(0, 0, []),
                    new GreyscaleImage(0, 0, []),
                ];
            }
            var channels = ToColorChannels(image, colorspace);
            return [new GreyscaleImage(image.width, image.height, channels[0]),
                    new GreyscaleImage(image.width, image.height, channels[1]),
                    new GreyscaleImage(image.width, image.height, channels[2]),  ];
        }
    }
}
