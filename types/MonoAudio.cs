using EffectPipeline.types;
using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.types
{
    public class MonoAudio : IInstance
    {
        public Type Type => Type.MonoAudio;
        public int sampleRate { get; }
        public float[] samples { get; }

        public MonoAudio(int _samplerate, float[] _samples)
        {
            this.sampleRate = _samplerate;
            this.samples = _samples;
        }

        public RGBImage? ToImage()
        {
            return RGBImage.WhiteImage(1, 1);
        }

        public bool SupportInto(Type type) => type switch { _ => false };

        public IInstance Into(Type type)
        {
            switch (type)
            {
                default:
                    throw new NotImplementedException();
            }
        }

    }
}
