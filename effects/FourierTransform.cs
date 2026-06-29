using EffectPipeline.types;
using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.Effects
{
    internal class FourierTransform : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Real", typeof(GreyscaleImage)), ("Imag", typeof(GreyscaleImage))];

        public IEnumerable<(string, Type)> Outputs => [("Real", typeof(GreyscaleImage)), ("Imag", typeof(GreyscaleImage))];

        public Property[] Properties => [];

        public async Task<IInstance[]> applyEffect(IInstance?[] inputs, IPropertyState[] properties)
        {
            var real = (GreyscaleImage?)inputs[0];
            var imag = (GreyscaleImage?)inputs[1];

            if (real == null || imag == null || real.image.Length <= 0 || imag.image.Length <= 0)
            {
                return [new GreyscaleImage(0, 0, []), new GreyscaleImage(0, 0, [])]; 
            }

            var fft = new FftFlat.FastFourierTransform(real.image.Length);
            var res = real.image.Zip(imag.image).Select(x => new System.Numerics.Complex(x.First, x.Second)).ToArray();
            fft.Forward(res.AsSpan());
            return [new GreyscaleImage(real.width, real.height, res.Select(x => (float)x.Real / real.image.Length).ToArray()), new GreyscaleImage(real.width, real.height, res.Select(x => (float)x.Imaginary / real.image.Length).ToArray())];

        }
    }
    internal class FourierTransformSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["complex", "fourier transform", "fft", "frequency", "frequencies"];

        public string Title => "Fourier Transform";

        public IEffect CreateEffect() => new FourierTransform();
    }
}
