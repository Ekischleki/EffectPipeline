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
    internal class AudioToImage : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Input Audio", Type.MonoAudio)];

        public IEnumerable<(string, Type)> Outputs => [("Output Image", Type.GreyscaleImage)];


        public Property[] Properties => [new NumberInputProperty("Width") { Value = 256, Min = 1, Max = Int32.MaxValue }, new NumberInputProperty("Height") { Value = 256, Min = 1, Max = Int32.MaxValue }];


        public IInstance[] applyEffect(IInstance?[] inputs, Property[] properties)
        {
            if (inputs[0] == null)
            {
                return [GreyscaleImage.PureWhite(1, 1)];
            }

            int width = ((NumberInputProperty)properties[0]).Value;
            int height = ((NumberInputProperty)properties[1]).Value;

            float[] values = ((MonoAudio)inputs[0]!).samples;
            float[] paddedValues = new float[width * height];
            for (int i = 0; i < width * height; i++)
            {
                if (i < values.Length)
                {
                    paddedValues[i] = values[i];
                    continue;
                }
                paddedValues[i] = 0;
            }

            return [new GreyscaleImage(width, height, paddedValues)];
        }

    }

    internal class AudioToImageSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["image", "greyscale", "audio", "convert", "from"];

        public string Title => "Mono Audio To Image";

        public IEffect CreateEffect() => new AudioToImage();

    }
}
