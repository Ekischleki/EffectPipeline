using EffectPipeline.types;
using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.Effects
{
    internal class ImageSource : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [];

        public IEnumerable<(string, Type)> Outputs => [("Source Image", Type.RGBImage)];

        public Property[] Properties => [/*Something like string or filepath property*/];


        // Image reference data idek
        RGBImage imageData;

        public ImageSource(RGBImage _imageData)
        {
            imageData = _imageData;
        }


        public IInstance[] applyEffect(IInstance?[] inputs, Property[] properties)
        {
            if (inputs.Length != 0)
            {
                throw new ArgumentException("Input needs to be length 0");
            }

            return [imageData];
        }
    }

    internal class ImageSourceSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["image", "source", "load", "file", "input"];

        public string Title => "Image Source";

        //Temporary
        public IEffect CreateEffect() => new ImageSource(RGBImage.LoadFrom("./assets/textures/aquarellebg.png"));
    }
}
