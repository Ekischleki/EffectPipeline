using EffectPipeline.gameObjects;
using Pandemonium.Engine.GameObjectStuff;
using SimpleBinaryFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline
{
    public abstract class Property : GUIElement
    {
        public abstract bool TryLoad(IPropertyState val);

        public abstract IPropertyState GetPropertyState();
    }
    public interface IPropertyState : ISerializable { }

}
