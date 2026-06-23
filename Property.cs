using EffectPipeline.gameObjects;
using EffectPipeline.project;
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
        public abstract IPropertySave Save();
        public abstract bool TryLoad(IPropertySave val);
    }
}
