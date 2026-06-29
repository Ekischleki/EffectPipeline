using EffectPipeline.Effects;
using EffectPipeline.gameObjects.GUI_Elements;
using EffectPipeline.types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.effects
{
    internal class WhiteNoise : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [];

        public IEnumerable<(string, Type)> Outputs => [("Noise", typeof(GreyscaleImage))];

        public Property[] Properties => [
            new NumberInputProperty("Seed") { Max = int.MaxValue, Min = 0},
            new NumberInputProperty("Width") { Max = 9999, Min = 1, Value = 100}, 
            new NumberInputProperty("Height") { Max = 9999, Min = 1, Value = 100 }, 
            new FloatInputProperty("Bound 1") {Max = 1, Min = 0},
            new FloatInputProperty("Bound 2") {Max = 1, Min = 0, Value = 1}];

        public async Task<IInstance[]> applyEffect(IInstance?[] inputs, IPropertyState[] properties)
        {
            var seed = ((NumberInputPropertyState)properties[0]).Value;
            var width = ((NumberInputPropertyState)properties[1]).Value;
            var height = ((NumberInputPropertyState)properties[2]).Value;
            var bound_1 = ((FloatInputPropertyState)properties[3]).Value;
            var bound_2 = ((FloatInputPropertyState)properties[4]).Value;
            var lower_bound = float.Min(bound_2, bound_1);
            var upper_bound = float.Max(bound_2, bound_1);
            var bound_range = upper_bound - lower_bound;
            var image = new float[width * height];

            var rand = new Random(seed);
            for (int i = 0; i < image.Length; i++)
            {
                var val = rand.NextSingle() * bound_range + lower_bound;
                image[i] = val;
            }
            return [new GreyscaleImage(width, height, image)];
        }
    }
    internal class WhiteNoiseSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["noise", "white", "random", "static"];

        public string Title => "White Noise";

        public IEffect CreateEffect() => new WhiteNoise();
    }
}
