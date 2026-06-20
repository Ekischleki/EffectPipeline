using EffectPipeline.effects;
using EffectPipeline.Effects;
using EffectPipeline.gameObjects;
using EffectPipeline.gameObjects.GUI_Elements;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.GameSceneStuff;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using Pandemonium.Engine.Stores;
using Pandemonium.Engine.UIOI;
using Pupilmonium.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline
{
    internal class TestObject : GameObject
    {
        [GetFrom(StoreType.TextureStore, "std:adhd mix.png")]
        ManagedTexture texture = null!;

        public override void Init()
        {
            RenderTexture = texture;
            Size = 1.5f;
            AddChildSpawnQueue(new TestObject2()
            {
                origin = IPositioning.Center,
                anchor = IPositioning.Center,
            });
        }

        protected override void Update()
        {
        }
    }
    internal class TestObject2 : GameObject
    {
        [GetFrom(StoreType.TextureStore, "std:SpOoky.png")]
        ManagedTexture texture = null!;
        [GetFrom(Singleton.DeltaTime)]
        DeltaTime dt = null!;
        float timer = 0;
        public override void Init()
        {
            RenderTexture = texture;
        }

        protected override void Render()
        {
            base.Render();
        }

        protected override void Update()
        {
            timer += (float)dt.MultiplierF;
            if(timer > 100)
            {
                timer -= 100;
                Show = !Show;
            }
        }
    }

    internal class TestScene : GameScene
    {
        protected override IEnumerable<GameObject> GetStartingGameObjects()
        {
            yield break;
        }

        protected override bool OnExitRequest()
        {
            return true;
        }
    }

    internal class MainScene : GameScene
    {


        protected override IEnumerable<GameObject> GetStartingGameObjects()
        {
            /*
            yield return new TestObject()
            {
                origin = IPositioning.Center,
                anchor = IPositioning.Center,
            };
            yield break;*/
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
