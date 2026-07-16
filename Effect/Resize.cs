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
    internal class Resize : IEffect
    {
        public string Title => "Rescale Image";

        public IEnumerable<(string, Type)> Inputs => [("Input Image", typeof(RGBImage))];

        public IEnumerable<(string, Type)> Outputs => [("Resized Image", typeof(RGBImage))];

        public Property[] Properties => [new NumberInputProperty("Width") { Value = 512, Min = 1, Max = Int32.MaxValue}, new NumberInputProperty("Height") { Value = 512, Min = 1, Max = Int32.MaxValue }];


        public async Task<IInstance[]> applyEffect(IInstance[] inputs, IPropertyState[] properties)
        {
            RGBImage inputImage = (RGBImage)inputs[0];
            if (inputImage == null)
                return [new RGBImage(0, 0, [], [], [])];

            

            int newWidth = ((NumberInputPropertyState)properties[0]).Value;
            int newHeight = ((NumberInputPropertyState)properties[1]).Value;

            

            return [ResizeImage(inputImage, newWidth, newHeight)];
        }
        public static RGBImage ResizeImage(RGBImage input, int newWidth, int newHeight)
        {
            int oldWidth = input.width;
            int oldHeight = input.height;
            float[] redArray = new float[newWidth * newHeight];
            float[] greenArray = new float[newWidth * newHeight];
            float[] blueArray = new float[newWidth * newHeight];

            for (int i = 0; i < newWidth; i++)
            {
                int oldI = (int)Math.Floor(((double)i / (double)newWidth) * (double)oldWidth);

                for (int j = 0; j < newHeight; j++)
                {
                    int oldJ = (int)Math.Floor(((double)j / (double)newHeight) * (double)oldHeight);

                    redArray[j * newWidth + i] = input.red[oldJ * oldWidth + oldI];
                    greenArray[j * newWidth + i] = input.green[oldJ * oldWidth + oldI];
                    blueArray[j * newWidth + i] = input.blue[oldJ * oldWidth + oldI];
                }
            }

            return new RGBImage(newWidth, newHeight, redArray, greenArray, blueArray);
        }
    }



    internal class ResizeSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["resize", "rescale", "size", "scale", "aspect ratio"];
        public IEffect CreateEffect() => new Resize();
    }

}
