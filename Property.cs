using EffectPipeline.gameObjects;
using Pandemonium.Engine.GameObjectStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline
{
    public abstract class Property : GUIElement
    {
        public abstract string Save();
        public abstract bool TryLoad(string val);

        public abstract IPropertyState GetPropertyState();
    }
    public interface IPropertyState { }

}
