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
    internal class Resize : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Input Image", Type.RGBImage)];

        public IEnumerable<(string, Type)> Outputs => [("Resized Image", Type.RGBImage)];

        public GameObject[] Properties => [new NumberInputProperty("Width") { Value = 512, Min = 1, Max = Int32.MaxValue}, new NumberInputProperty("Height") { Value = 512, Min = 1, Max = Int32.MaxValue }];


        public IInstance[] applyEffect(IInstance[] inputs, GameObject[] properties)
        {
            RGBImage inputImage = (RGBImage)inputs[0];
            if (inputImage == null)
                return [new RGBImage(0, 0, [], [], [])];

            int oldWidth = inputImage.width;
            int oldHeight = inputImage.height;

            int newWidth = ((NumberInputProperty)properties[0]).Value;
            int newHeight = ((NumberInputProperty)properties[1]).Value;

            float[] redArray = new float[newWidth * newHeight];
            float[] greenArray = new float[newWidth * newHeight];
            float[] blueArray = new float[newWidth * newHeight];

            for (int i = 0; i < newWidth; i++)
            {
                int oldI = (int)Math.Floor(((double)i / (double)newWidth) * (double)oldWidth);

                for (int j = 0; j < newHeight; j++)
                {
                    int oldJ = (int)Math.Floor(((double)j / (double)newHeight) * (double)oldHeight);

                    redArray[j * newWidth + i] = inputImage.red[oldJ * oldWidth + oldI];
                    greenArray[j * newWidth + i] = inputImage.green[oldJ * oldWidth + oldI];
                    blueArray[j * newWidth + i] = inputImage.blue[oldJ * oldWidth + oldI];
                }
            }

            return [new RGBImage(newWidth, newHeight, redArray, greenArray, blueArray)];
        }
    }


    internal class ResizeSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["resize", "rescale", "size", "scale", "aspect ratio"];

        public string Title => "Rescale Image";

        public IEffect CreateEffect() => new Resize();
    }

}
