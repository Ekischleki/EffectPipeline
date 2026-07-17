using EffectPipeline.GameObjects.GUIElements;
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
        public string Title => "Image Source";

        public IEnumerable<(string, Type)> Inputs => [];

        public IEnumerable<(string, Type)> Outputs => [("Source Image", typeof(RGBImage))];

        public Property[] Properties => [new DropdownProperty(["Adhd mix", "Aquarelle", "Consider", "Sheets", "Spooky", "Tree", "Color theory", "Quantized Roses", "Gay test", "Fireworks", "Covverrr", "Covverrrflipped", "Mask"], "Image")];


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
            10 => "covverrr.png",
            11 => "covverrrflipped.png",
            12 => "mask.png",
            _ => "Consider.png"
        };


        public ImageSource()
        {
        }


        public async Task<IInstance[]> applyEffect(IInstance?[] inputs, IPropertyState[] properties)
        {
            var selected = ((DropdownPropertyState)properties[0]).Selected;
            var imageData = RGBImage.LoadFrom($"./assets/textures/{SelectedToName(selected)}");
            return [imageData];
        }
    }

    internal class ImageSourceSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["image", "source", "load", "file", "input"];
        //Temporary
        public IEffect CreateEffect() => new ImageSource();
    }
}
