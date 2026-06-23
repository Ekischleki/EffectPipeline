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
    internal class ToAudio : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Image Input", Type.GreyscaleImage)];

        public IEnumerable<(string, Type)> Outputs => [("Audio Output", Type.MonoAudio)];

        public GameObject[] Properties => [new NumberInputProperty("Sample Rate") { Value = 44100, Min = 1, Max = Int32.MaxValue }];

        public IInstance[] applyEffect(IInstance?[] inputs, GameObject[] properties)
        {
            if (inputs[0] == null)
            {
                return [new MonoAudio(1, [])];
            }

            int samplerate = ((NumberInputProperty)properties[0]).Value;
            float[] samples = ((GreyscaleImage)inputs[0]!).image;

            return [new MonoAudio(samplerate, samples)];
        }

    }

    internal class ToAudioSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["audio", "mono", "to audio", "sample", "linearize"];

        public string Title => "Image To Mono Audio";

        public IEffect CreateEffect() => new ToAudio();
    }

}
