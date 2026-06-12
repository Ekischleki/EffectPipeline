using EffectPipeline.Effects;
using EffectPipeline.gameObjects;
using EffectPipeline.gameObjects.GUI_Elements;
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

        protected override IEnumerable<GameObject> GetStartingGameObjects()
        {
            yield return new NodePipelineEditor() {
                origin = IPositioning.Center,
                anchor = IPositioning.Center,
            };
        }

        protected override bool OnExitRequest()
        {
            return true;
        }
    }
}
