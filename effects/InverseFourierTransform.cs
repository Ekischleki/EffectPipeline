using EffectPipeline.types;
using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.effects
{
    internal class InverseFourierTransform : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Real", Type.GreyscaleImage), ("Imag", Type.GreyscaleImage)];

        public IEnumerable<(string, Type)> Outputs => [("Real", Type.GreyscaleImage), ("Imag", Type.GreyscaleImage)];

        public GameObject[] Properties => [];

        public IInstance[] applyEffect(IInstance?[] inputs, GameObject[] properties)
        {
            var real = (GreyscaleImage?)inputs[0];
            var imag = (GreyscaleImage?)inputs[1];

            if (real == null || imag == null || real.image.Length <= 0 || imag.image.Length <= 0)
            {
                return [new GreyscaleImage(0, 0, []), new GreyscaleImage(0, 0, [])];
            }
            var fft = new FftFlat.FastFourierTransform(real.image.Length);
            var res = real.image.Zip(imag.image).Select(x => new System.Numerics.Complex(x.First, x.Second)).ToArray();
            fft.Inverse(res.AsSpan());
            return [new GreyscaleImage(real.width, real.height, res.Select(x => (float)x.Real).ToArray()), new GreyscaleImage(real.width, real.height, res.Select(x => (float)x.Imaginary).ToArray())];

        }
    }

    internal class InverseFourierTransformSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["inverse", "complex", "fourier transform", "fft", "frequency", "frequencies"];

        public string Title => "Inverse Fourier Transform";

        public IEffect CreateEffect() => new InverseFourierTransform();
    }
}
