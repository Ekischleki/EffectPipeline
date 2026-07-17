using EffectPipeline.GameObjects.GUIElements;
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
    internal class SplitChannel : IEffect
    {
        public string Title => "Channel Split";

        public IEnumerable<(string, Type)> Inputs => [("Image", typeof(RGBImage))];

        public IEnumerable<(string, Type)> Outputs => [("Channel 0", typeof(GreyscaleImage)), ("Channel 1", typeof(GreyscaleImage)), ("Channel 2", typeof(GreyscaleImage))];

        public Property[] Properties => [DropdownProperty.ColorspaceDropdown];


        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        private float[] ConvertColorToColorspaceChannels(ColourSpace colorSpace, Unicolour color)
        {
            switch (colorSpace)
            {
                case ColourSpace.Hsb:
                    var hsb = color.Hsb;
                    return [(float)hsb.H, (float)hsb.S, (float)hsb.B];
                case ColourSpace.Oklab:
                    var oklab = color.Oklab;
                    return [(float)oklab.L, (float)oklab.A, (float)oklab.B];
                case ColourSpace.Oklch:
                    var oklch = color.Oklch;
                    return [(float)oklch.L, (float)oklch.C, (float)oklch.H];
                default:
                    throw new NotImplementedException();
            }
        }

        private float[][] ToColorspace(ColourSpace colorSpace, int width, int height, float[][] channels, float channelZeroMin, float channelZeroMax, float channelOneMin, float channelOneMax, float channelTwoMin, float channelTwoMax)
        {
            var c0 = new float[width* height];
            var c1 = new float[width * height];
            var c2 = new float[width * height];

            for (int i = 0; i < width * height; i++)
            {
                var color = new Unicolour(ColourSpace.Rgb, channels[0][i], channels[1][i], channels[2][i]);
                var convertedChannels = ConvertColorToColorspaceChannels(colorSpace, color);

                c0[i] = (convertedChannels[0] - channelZeroMin) / (channelZeroMax - channelZeroMin);
                c1[i] = (convertedChannels[1] - channelOneMin) / (channelOneMax - channelOneMin);
                c2[i] = (convertedChannels[2] - channelTwoMin) / (channelTwoMax - channelTwoMin);
            }
            return [c0, c1, c2];
        }

        private float[][] ToColorChannels(RGBImage image, DropdownProperty.Colorspace colorspace)
        {
            switch (colorspace)
            {
                case DropdownProperty.Colorspace.Rgb:
                    return [image.red, image.green, image.blue];
                case DropdownProperty.Colorspace.Hsv:
                    return ToColorspace(ColourSpace.Hsb, image.width, image.height, [image.red, image.green, image.blue], 0, 360, 0, 1, 0, 1);
                case DropdownProperty.Colorspace.OkLab:
                    return ToColorspace(ColourSpace.Oklab, image.width, image.height, [image.red, image.green, image.blue], 0, 1, -0.2339f, 0.2762f, -0.3115f, 0.1986f);
                case DropdownProperty.Colorspace.OkLch:
                    return ToColorspace(ColourSpace.Oklch, image.width, image.height, [image.red, image.green, image.blue], 0, 1, 0, 0.5002f, 0, 360);

                default:
                    throw new NotImplementedException();
            }
        }


        public async Task<IInstance[]> applyEffect(IInstance[] inputs, IPropertyState[] properties)
        {
            DropdownPropertyState colorspaceDropdown = (DropdownPropertyState)properties[0];
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
                    new GreyscaleImage(image.width, image.height, channels[2]),];
        }
    }

    internal class SplitChannelSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["channel", "split", "converted", "hsv", "oklab", "image", "channels"];
        public IEffect CreateEffect() => new SplitChannel();
    }
}
