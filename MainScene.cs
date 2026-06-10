using EffectPipeline.Effects;
using EffectPipeline.gameObjects;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.GameSceneStuff;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using Pandemonium.Engine.UIOI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline
{
    internal class MainScene : GameScene
    {
        internal ConnectionManager ConnectionManager = null!;
        protected override IEnumerable<GameObject> GetStartingGameObjects()
        {
            yield return new Node(new SplitChannel(), "Split channels RGB")
            { 
                anchor = IPositioning.Center,
                origin = IPositioning.Center,
            };
            yield return new Node(new MergeChannel(), "Merge channels RGB")
            {
                anchor = IPositioning.Center,
                offset = IPositioning.Absolute(10, 20),
                origin = IPositioning.Center,
            };
            yield return ConnectionManager = new ConnectionManager();
        }

        protected override bool OnExitRequest()
        {
            return true;
        }
    }
}
