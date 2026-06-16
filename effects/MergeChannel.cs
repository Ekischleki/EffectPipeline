using EffectPipeline.gameObjects.GUI_Elements;
using EffectPipeline.types;
using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Drawing;
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


        private float[] RGBFromHSV(float hue, float saturation, float brightness)
        {
            hue *= 360;
            int hi = Convert.ToInt32(Math.Floor(hue / 60f)) % 6;
            float f = hue / 60 - (float)Math.Floor(hue / 60);

            float v = brightness;
            float p = (brightness * (1 - saturation));
            float q = (brightness * (1 - f * saturation));
            float t = (brightness * (1 - (1 - f) * saturation));

            if (hi == 0)
                return [v, t, p];
            else if (hi == 1)
                return [q, v, p];
            else if (hi == 2)
                return [p, v, t];
            else if (hi == 3)
                return [p, q, v];
            else if (hi == 4)
                return [t, p, v];
            else
                return [v, p, q];
        }

        private RGBImage FromColorChannels(int width, int height, float[][] channels, DropdownProperty.Colorspace colorspace)
        {
            switch (colorspace)
            {
                case DropdownProperty.Colorspace.Rgb: 
                    return new RGBImage(width, height, channels[0], channels[1], channels[2]);
                case DropdownProperty.Colorspace.Hsv:
                    float[][] rgb = new float[width * height][];
                    for (int i = 0; i < width * height; i++)
                        rgb[i] = RGBFromHSV(channels[0][i], channels[1][i], channels[2][i]);

                    float[] redChannel = rgb.Select((color) => color[0]).ToArray();
                    float[] greenChannel = rgb.Select((color) => color[1]).ToArray();
                    float[] blueChannel = rgb.Select((color) => color[2]).ToArray();
                    return new RGBImage(width, height, redChannel, greenChannel, blueChannel);
                default:
                    throw new NotImplementedException();
            }
        }

        public IInstance[] applyEffect(IInstance[] inputs, GameObject[] properties)
        {
            bool set = false;
            int width = 0;
            int height = 0;
            foreach (var input in inputs)
            {
                if(input is GreyscaleImage img && img.image.Length > 0) {
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

            float[][] channelArray = new float[3][];

            for (int i = 0; i < inputs.Length; i++)
            {
                if(inputs[i] == null)
                {
                    channelArray[i] = new float[width * height];
                    continue;
                }

                channelArray[i] = ((GreyscaleImage)inputs[i]).image;
            }

            DropdownProperty colorspaceDropdown = (DropdownProperty)properties[0];
            var colorspace = (DropdownProperty.Colorspace)colorspaceDropdown.Selected;

            RGBImage image = FromColorChannels(width, height, channelArray, colorspace);
            return [image];
            
            
        }
    }
}
