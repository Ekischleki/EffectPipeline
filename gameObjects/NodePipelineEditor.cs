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

namespace EffectPipeline.GameObjects
{
    internal class NodePipelineEditor : GameObject
    {
        [GetFrom(Singleton.ScreenContainer)]
        IContainer screenContainer = null!;
        internal Vector2 size = new(800);
        internal int canvas_width = 500;
        public override Vector2 ContainerSize { get => size; protected set { } }
        [DependencyCache(InteractionType.Upload)]
        internal NodeCanvas node_canvas = new() { origin = IPositioning.TopLeft, anchor = IPositioning.TopLeft };
        internal ImageDisplayContainer display = new() { origin = IPositioning.TopRight, anchor = IPositioning.TopRight };
        public override void Init()
        {
            AddChildSpawnQueue([node_canvas, display]);
        }

        protected override void Update()
        {
            size = screenContainer.AbsoluteContainerSize;
            canvas_width = (int)(size.X * .5);
            node_canvas.Size = new(canvas_width, size.Y);
            display.size = new(size.X - canvas_width, size.Y);
        }
    }
}
