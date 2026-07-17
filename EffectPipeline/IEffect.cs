using Pandemonium.Engine.GameObjectStuff;
using EffectPipeline.types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline
{
    /// <summary>
    /// An effect implementation. This must be stateless.
    /// </summary>
    public interface IEffect
    {
        public string Title { get; }
        public IEnumerable<(string, Type)> Inputs { get; }

        public IEnumerable<(string, Type)> Outputs { get; }

        /// <summary>
        /// Apply the efffect returning types and amounts specified in <see cref="Outputs"/>.
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public Task<IInstance[]> applyEffect(IInstance?[] inputs, IPropertyState[] properties);
        public Property[] Properties { get; }
    }
}
