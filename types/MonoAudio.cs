using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.types
{
    internal class MonoAudio : IInstance
    {
        public Type Type => Type.MonoAudio;
        public int sampleRate { get; }
        public float[] samples { get; }

        public MonoAudio(int _samplerate, float[] _samples)
        {
            this.sampleRate = _samplerate;
            this.samples = _samples;
        }


        public GreyscaleImage ToGreyscale()
        {
            return new GreyscaleImage(samples.Length, 1, samples);
        }

        public RGBImage? ToImage()
        {
            return ToGreyscale().ToImage();
        }

        public bool SupportInto(Type type) => type switch { Type.RGBImage => true, Type.GreyscaleImage => true, _ => false };

        public IInstance Into(Type type)
        {
            switch (type)
            {
                case Type.GreyscaleImage:
                    return this.ToGreyscale();
                case Type.RGBImage:
                    return this.ToImage()!;
                default:
                    throw new NotImplementedException();
            }
        }

    }
}
