using EffectPipeline.types;
using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.Effects
{
    internal class ImageOutput : IEffect
    {
        public string Title => "Output";

        public IEnumerable<(string, Type)> Inputs => [("Output Image", typeof(RGBImage))];

        public IEnumerable<(string, Type)> Outputs => [];

        public Property[] Properties => [];

        public async Task<IInstance[]> applyEffect(IInstance[] inputs, IPropertyState[] properties)
        {
            throw new NotImplementedException("Image output effect is hardcoded");
        }
    }

    //There is no search for image output because there always only exists one
}
