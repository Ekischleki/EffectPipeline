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
    internal class ImageSource : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [];

        public IEnumerable<(string, Type)> Outputs => [("Source Image", Type.RGBImage)];

        public Property[] Properties => [new DropdownProperty(["Adhd mix", "Aquarelle", "Consider", "Sheets", "Spooky", "Tree", "Color theory", "Quantized Roses", "Gay test", "Fireworks"], "Image")];


        public string SelectedToName(int selected) => selected switch
        {
            0 => "adhd mix.png",
            1 => "aquarellebg.png",
            2 => "Consider.png",
            3 => "sheets.png",
            4 => "SpOoKy.png",
            5 => "tree.png",
            6 => "color theory all.png",
            7 => "Roses.png",
            8 => "Gay Test.png",
            9 => "Fireworks.png",
            _ => "Consider.png"
        };

        // Image reference data idek
        RGBImage imageData;

        public ImageSource(RGBImage _imageData)
        {
            imageData = _imageData;
        }


        public IInstance[] applyEffect(IInstance?[] inputs, Property[] properties)
        {
            var selected = ((DropdownProperty)properties[0]).Selected;
            imageData = RGBImage.LoadFrom($"./assets/textures/{SelectedToName(selected)}");
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
