using EffectPipeline.gameObjects.GUI_Elements;
using EffectPipeline.types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wacton.Unicolour;

namespace EffectPipeline.effects
{
    internal class Arithmetic : IEffect
    {
        public required Func<float, float, float> Operation;
        public IEnumerable<(string, Type)> Inputs => [("A", Type.GreyscaleImage), ("B", Type.GreyscaleImage)];

        public IEnumerable<(string, Type)> Outputs => [("Sum", Type.GreyscaleImage)];

        public Property[] Properties => [];

        public IInstance[] applyEffect(IInstance[] inputs, Property[] properties)
        {
            var a = (GreyscaleImage?)inputs[0] ?? throw new ArgumentNullException();
            var b = (GreyscaleImage?)inputs[1] ?? throw new ArgumentNullException();

            if (a.height != b.height || a.width != b.width) throw new Exception("Image dimensions must match");
            var res = new float[a.image.Length];
            for (int i = 0; i < a.image.Length; i++)
            {
                res[i] = Operation(a.image[i], b.image[i]);
            }

            return [new GreyscaleImage(a.width, a.height, res)];
        }

    }
    internal class AdditionSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["image", "addition", "arithmetic", "operator"];

        public string Title => "Add";

        public IEffect CreateEffect() => new Arithmetic() { Operation = (a, b) => a + b };
    }
    internal class SubtractionSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["image", "subtract", "arithmetic", "operator"];

        public string Title => "Sub";

        public IEffect CreateEffect() => new Arithmetic() { Operation = (a, b) => a - b };
    }
    internal class MultiplicationSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["image", "multiply", "arithmetic", "operator"];

        public string Title => "Mul";

        public IEffect CreateEffect() => new Arithmetic() { Operation = (a, b) => a * b };
    }
}
