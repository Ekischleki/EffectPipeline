using Pandemonium.Engine.GameObjectStuff;
using EffectPipeline.types;
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
        public Task<IInstance[]> applyEffect(IInstance?[] inputs, IPropertyState[] properties);
        public Property[] Properties { get; }
    }
}
