using Pandemonium.Engine;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.gameObjects
{
    internal class ImageDisplayContainer : GameObject
    {
        internal ImageDisplayContainer()
        {
            clippingContainer = this;
        }
        [DependencyCache(InteractionType.Upload)]
        protected IContainer clippingContainer = null!;
        internal Vector2 size = new(500);
        public override Vector2 ContainerSize { get => size; protected set { } }

        public override void Init()
        {
            clipBehavior = ClipBehavior.Cut;
            AddChildSpawnQueue(new NodeCanvasCamera().WithChildren([new ImageDisplay()]));
        }

        protected override void Update()
        {
            
        }
    }
    internal class ImageDisplay : GameObject
    {
        [DependencyCache(InteractionType.Download)]
        internal NodeCanvas node_canvas = null!;

       
        public override void Init()
        {
            
        }

        protected override void Update()
        {
            RenderTexture = node_canvas.manager.OutputImage;
        }
    }
}
