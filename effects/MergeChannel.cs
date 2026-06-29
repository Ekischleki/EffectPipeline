using EffectPipeline.gameObjects.GUI_Elements;
using EffectPipeline.types;
using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wacton.Unicolour;

namespace EffectPipeline.Effects
{
    internal class MergeChannel : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Channel 0", typeof(GreyscaleImage)), ("Channel 1", typeof(GreyscaleImage)), ("Channel 2", typeof(GreyscaleImage))];

        public IEnumerable<(string, Type)> Outputs => [("Image", typeof(RGBImage))];

        public Property[] Properties => [DropdownProperty.ColorspaceDropdown];


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


        private RGBImage FromColorspace(ColourSpace colorSpace, int width, int height, float[][] channels, float channelZeroMin, float channelZeroMax, float channelOneMin, float channelOneMax, float channelTwoMin, float channelTwoMax)
        {
            var rgb = new float[width * height][];
            for (int i = 0; i < width * height; i++)
            {
                var first = channels[0][i] * (channelZeroMax - channelZeroMin) + channelZeroMin;
                var second = channels[1][i] * (channelOneMax - channelOneMin) + channelOneMin;
                var third = channels[2][i] * (channelTwoMax - channelTwoMin) + channelTwoMin;
                var color = new Unicolour(colorSpace, (first, second, third));
                 var rgb_col = color.Rgb.Clipped;
                rgb[i] = [(float)rgb_col.R, (float)rgb_col.G, (float)rgb_col.B];
            }
            var redChannel = rgb.Select((color) => color[0]).ToArray();
            var greenChannel = rgb.Select((color) => color[1]).ToArray();
            var blueChannel = rgb.Select((color) => color[2]).ToArray();
            return new RGBImage(width, height, redChannel, greenChannel, blueChannel);
        }

        private RGBImage FromColorChannels(int width, int height, float[][] channels, DropdownProperty.Colorspace colorspace)
        {
            switch (colorspace)
            {
                case DropdownProperty.Colorspace.Rgb: 
                    return new RGBImage(width, height, channels[0], channels[1], channels[2]);
                case DropdownProperty.Colorspace.Hsv:
                    return FromColorspace(ColourSpace.Hsb, width, height, channels, 0, 360, 0, 1, 0, 1);
                case DropdownProperty.Colorspace.OkLab:
                    return FromColorspace(ColourSpace.Oklab, width, height, channels, 0, 1, -0.2339f, 0.2762f, -0.3115f, 0.1986f);
                case DropdownProperty.Colorspace.OkLch:
                    return FromColorspace(ColourSpace.Oklch, width, height, channels, 0, 1, 0, 0.5002f, 0, 360);
                default:
                    throw new NotImplementedException();
            }
        }

        public async Task<IInstance[]> applyEffect(IInstance[] inputs, IPropertyState[] properties)
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

            DropdownPropertyState colorspaceDropdown = (DropdownPropertyState)properties[0];
            var colorspace = (DropdownProperty.Colorspace)colorspaceDropdown.Selected;

            RGBImage image = FromColorChannels(width, height, channelArray, colorspace);
            return [image];
            
            
        }

    }
        internal class MergeChannelSearch : IEffectSearch
        {
            public IEnumerable<string> Tags => ["channel", "merge", "rgb", "hsv", "oklab", "image"];

            public string Title => "Channel Merge";

            public IEffect CreateEffect() => new MergeChannel();
        }
}
