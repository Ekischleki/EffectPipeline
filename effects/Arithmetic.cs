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
        public IEnumerable<(string, Type)> Inputs => [("A", typeof(GreyscaleImage)), ("B", typeof(GreyscaleImage))];

        public IEnumerable<(string, Type)> Outputs => [("Res", typeof(GreyscaleImage))];

        public Property[] Properties => [new DropdownProperty(["Add", "Sub", "Mul"], "Operation")];

        public async Task<IInstance[]> applyEffect(IInstance[] inputs, IPropertyState[] properties)
        {
            var operation = ((DropdownPropertyState)properties[0]).Selected;
            var a = (GreyscaleImage?)inputs[0] ?? throw new ArgumentNullException();
            var b = (GreyscaleImage?)inputs[1] ?? throw new ArgumentNullException();

            if (a.height != b.height || a.width != b.width) throw new Exception("Image dimensions must match");
            var res = new float[a.image.Length];
            for (int i = 0; i < a.image.Length; i++)
            {
                switch (operation)
                {
                    case 0:
                        res[i] = a.image[i] + b.image[i];
                        break;
                    case 1:
                        res[i] = a.image[i] - b.image[i];
                        break;
                    case 2:
                        res[i] = a.image[i] * b.image[i];
                        break;
                    default: throw new NotImplementedException();
                }
            }

            return [new GreyscaleImage(a.width, a.height, res)];
        }

    }
    internal class AdditionSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["image", "operator", "addition", "add", "arithmetic", "sub", "subtraction", "mul", "multiplication"];

        public string Title => "Arithmetic";

        public IEffect CreateEffect() => new Arithmetic();
    }

}
