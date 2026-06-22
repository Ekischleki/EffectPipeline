using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline
{
    public interface IEffect
    {
        public IEnumerable<(string, Type)> Inputs { get; }

        public IEnumerable<(string, Type)> Outputs { get; }
        public IInstance[] applyEffect(IInstance?[] inputs, GameObject[] properties);
        public GameObject[] Properties { get; }
    }

    public enum Type
    {
        Mask,
        RGBImage,
        GreyscaleImage,
    }
}
