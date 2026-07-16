using EffectPipeline.types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.Effects
{
    internal class EffectOffline : IEffect
    {
        public string Title => "Effect Offline";

        public IEnumerable<(string, Type)> Inputs => [];

        public IEnumerable<(string, Type)> Outputs => [];

        public Property[] Properties => [];

        public async Task<IInstance[]> applyEffect(IInstance?[] inputs, IPropertyState[] properties)
        {
            return [];
        }
    }
}
