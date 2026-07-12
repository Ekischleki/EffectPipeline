using EffectPipeline.effects;
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
    internal class Desaturate : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Image", typeof(RGBImage))];

        public IEnumerable<(string, Type)> Outputs => [("Desaturated image", typeof(GreyscaleImage))];

        public Property[] Properties => [new DropdownProperty(["Lightness", "Average", "Min", "Max"], "Desaturation Type")];


        public async Task<IInstance[]> applyEffect(IInstance[] inputs, IPropertyState[] properties)
        {
            var desaturationType = ((DropdownPropertyState)properties[0]).Selected;
            var im = (RGBImage?)inputs[0] ?? throw new ArgumentNullException();

            Color[] imColors = im.ToColors();

            var res = new float[im.width * im.height];
            for (int i = 0; i < im.width * im.height; i++)
            {
                switch (desaturationType)
                {
                    case 0:
                        res[i] = imColors[i].GetBrightness();
                        break;
                    case 1:
                        res[i] = (imColors[i].R / 255f + imColors[i].G / 255f + imColors[i].B / 255f) / 3f;
                        break;
                    case 2:
                        res[i] = MathF.Min(imColors[i].R / 255f, MathF.Min(imColors[i].G / 255f, imColors[i].B / 255f));
                        break;
                    case 3:
                        res[i] = MathF.Max(imColors[i].R / 255f, MathF.Max(imColors[i].G / 255f, imColors[i].B / 255f));
                        break;
                    default: throw new NotImplementedException();
                }
            }

            return [new GreyscaleImage(im.width, im.height, res)];
        }

    }


    internal class DesatureSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["desat", "desature", "greyscale", "to greyscale", "convert"];

        public string Title => "Desature";

        public IEffect CreateEffect() => new Desaturate();
    }

}
