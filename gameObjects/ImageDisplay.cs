using Pandemonium.Engine;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using Pandemonium.Engine.Stores;
using Pupilmonium.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.GameObjects
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
            AddChildSpawnQueue(new NodeCanvasCamera().WithChildren([new ImageDisplay() { anchor = IPositioning.Center, origin = IPositioning.Center}]));
        }

        protected override void Update()
        {
            
        }
    }
    internal class ImageDisplay : GameObject
    {
        [DependencyCache(InteractionType.Download)]
        internal NodeCanvas node_canvas = null!;

        [GetFrom(StoreType.TextureStore, "std:loading.png")]
        ManagedTexture loading = null!;
        ManagedTexture? known_texture = null!;
        [GetFrom(Singleton.DeltaTime)]
        protected DeltaTime dt = null!;
        public override void Init()
        {
            node_canvas.manager.OutputImageChanged += (image) =>
            {
                known_texture = image?.ToTexture(Game.Canvas);
            };
        }

        protected override void Update()
        {
            var output_node_state = node_canvas.manager.OutputNodeState;
            lock (output_node_state)
            {
                if(!output_node_state.assigned_task?.IsCompleted ?? false)
                {
                    RenderTexture = loading;
                    Rotation += dt.Multiplier * 10;
                } else
                {
                    Rotation = 0;
                    RenderTexture = known_texture;
                }
            }
        }
    }
}
