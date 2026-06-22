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
        public IEnumerable<(string, Type)> Inputs => [("Output Image", Type.RGBImage)];

        public IEnumerable<(string, Type)> Outputs => [];

        public GameObject[] Properties => [];

        public IInstance[] applyEffect(IInstance[] inputs, GameObject[] properties)
        {
            if (inputs.Length != 1)
            {
                throw new ArgumentException("Input needs to be length 1");
            }

            return [];
        }
    }

    //There is no search for image output because there always only exists one
}
