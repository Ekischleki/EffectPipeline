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
    internal class AudioDelay : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Input Audio", typeof(MonoAudio))];

        public IEnumerable<(string, Type)> Outputs => [("Output Audio", typeof(MonoAudio))];

        public Property[] Properties => [new FloatInputProperty("Time") { Value = .25f, Min = 0.0001f, Max = float.MaxValue }, new FloatInputProperty("Decay") { Value = .75f, Min = 0, Max = float.MaxValue }];


        public async Task<IInstance[]> applyEffect(IInstance?[] inputs, IPropertyState[] properties)
        {
            if (inputs[0] == null)
            {
                return [new MonoAudio(1, [])];
            }

            MonoAudio audioSample = (MonoAudio)inputs[0]!;

            float delayTime = ((FloatInputPropertyState)properties[0]).Value;
            int numberOfSamples = (int)Math.Floor(audioSample.sampleRate * delayTime);

            float decay = ((FloatInputPropertyState)properties[1]).Value;

            float[] newSamples = new float[audioSample.samples.Length];

            for (int i = 0; i < audioSample.samples.Length; i++)
            {
                float value = audioSample.samples[i];
                int currentIndex = i;

                while (currentIndex < audioSample.samples.Length)
                {
                    newSamples[currentIndex] += value;

                    value *= decay;

                    currentIndex += numberOfSamples;
                }
            }

            return [new MonoAudio(audioSample.sampleRate, newSamples)];
        }

    }

    internal class AudioDelaySearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["delay", "audio", "echo", "audio delay", "repeat"];

        public string Title => "Audio Delay";

        public IEffect CreateEffect() => new AudioDelay();
    }
}
