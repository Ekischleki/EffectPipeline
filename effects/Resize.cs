using EffectPipeline.gameObjects.GUI_Elements;
using EffectPipeline.types;
using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.effects
{
    internal class Resize : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Input Image", Type.RGBImage)];

        public IEnumerable<(string, Type)> Outputs => [("Resized Image", Type.RGBImage)];

        public GameObject[] Properties => [new NumberInputProperty("Width", 512), new NumberInputProperty("Height", 512)];


        public IInstance[] applyEffect(IInstance[] inputs, GameObject[] properties)
        {
            RGBImage inputImage = (RGBImage)inputs[0];

            int oldWidth = inputImage.width;
            int oldHeight = inputImage.height;

            int newWidth = ((NumberInputProperty)properties[0]).Value;
            int newHeight = ((NumberInputProperty)properties[1]).Value;

            float[] redArray = new float[newWidth * newHeight];
            float[] greenArray = new float[newWidth * newHeight];
            float[] blueArray = new float[newWidth * newHeight];

            for (int i = 0; i < newWidth; i++) 
            {
                for (int j = 0; j < newHeight; j++)
                {
                    int oldI = (int)Math.Floor(((double)i / (double)newWidth) * (double)oldWidth);
                    int oldJ = (int)Math.Floor(((double)j / (double)newHeight) * (double)oldHeight);

                    redArray[i * newHeight + j] = inputImage.red[oldI * oldHeight + oldJ];
                    greenArray[i * newHeight + j] = inputImage.green[oldI * oldHeight + oldJ];
                    blueArray[i * newHeight + j] = inputImage.blue[oldI * oldHeight + oldJ];
                }
            }

            return [new RGBImage(newWidth, newHeight, redArray, greenArray, blueArray)];
        }
    }
}
