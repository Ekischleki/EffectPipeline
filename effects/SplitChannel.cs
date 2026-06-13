using EffectPipeline.gameObjects.GUI_Elements;
using EffectPipeline.types;
using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Drawing;
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


        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        private float[][] ToColorChannels(RGBImage image, DropdownProperty.Colorspace colorspace)
        {
            switch (colorspace)
            {
                case DropdownProperty.Colorspace.Rgb:
                    return [image.red, image.green, image.blue];
                case DropdownProperty.Colorspace.Hsv:
                    var colors = image.ToColors();
                    var hsv = colors.Select<Color, float[]>(col => { ColorToHSV(col, out double h, out double s, out double v); return [(float)h / 365f, (float)s, (float)v]; });
                    var h = new float[colors.Length];
                    var s = new float[colors.Length];
                    var v = new float[colors.Length];

                    var i = 0;
                    foreach (var p in hsv)
                    {
                        h[i] = p[0];
                        s[i] = p[1];
                        v[i] = p[2];

                        i++;
                    }

                    return [h, s, v];
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
