using EffectPipeline.GameObjects;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.SetupAttributes;
using Pandemonium.Engine.UIOI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.gameObjects
{
    internal class ConnectionManager : GameObject
    {

        internal Parameter? ClosestDragging = null;
        internal float ClosestDist = float.PositiveInfinity;
        [GetFrom(Singleton.Mouse)]
        Mouse mouse = null!;

        internal void RegisterDist(Parameter parameter)
        {

        }

        protected override void Update()
        {

            ClosestDragging = null;
            ClosestDist = float.PositiveInfinity;
        }

        public override void Init()
        {
        }
    }
}
