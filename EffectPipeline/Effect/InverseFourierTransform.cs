using EffectPipeline.types;
using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.Effects
{
    internal class InverseFourierTransform : IEffect
    {
        public string Title => "Inverse Fourier Transform";

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
            var res = real.image.Zip(imag.image).Select(x => new System.Numerics.Complex(x.First * real.image.Length, x.Second * real.image.Length)).ToArray();
            fft.Inverse(res.AsSpan());
            return [new GreyscaleImage(real.width, real.height, res.Select(x => (float)x.Real).ToArray()), new GreyscaleImage(real.width, real.height, res.Select(x => (float)x.Imaginary).ToArray())];

        }
    }

    internal class InverseFourierTransformSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["inverse", "complex", "fourier transform", "fft", "frequency", "frequencies"];
        public IEffect CreateEffect() => new InverseFourierTransform();
    }
}
