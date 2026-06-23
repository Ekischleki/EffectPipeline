using EffectPipeline.types;
using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.Effects
{
    internal class ChannelAverage : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Image", Type.GreyscaleImage)];

        public IEnumerable<(string, Type)> Outputs => [("Image", Type.GreyscaleImage)];

        public GameObject[] Properties => [];

        public IInstance[] applyEffect(IInstance?[] inputs, GameObject[] properties)
        {
            var image = (GreyscaleImage?)inputs[0];
            if(image == null || image.image.Length == 0)
            {
                return [new GreyscaleImage(0, 0, [])];
            }
            var moving_avg = image.image[0];
            float[] res = new float[image.image.Length];
            for (var i = 0; i < image.image.Length; i++)
            {
                moving_avg = 0.9f * moving_avg + 0.1f * image.image[i];
                res[i] = moving_avg;
            }
            return [new GreyscaleImage(image.width, image.height, res)];
        }
    }

    internal class ChannelAverageSearch : IEffectSearch 
    {
        public IEnumerable<string> Tags => ["moving average", "smear", "motion", "blur", "reverb"];

        public string Title => "Moving Average";

        public IEffect CreateEffect() => new ChannelAverage();
    }
}
