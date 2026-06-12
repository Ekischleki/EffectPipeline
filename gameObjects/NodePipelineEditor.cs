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
    internal class NodePipelineEditor : GameObject
    {
        internal Vector2 size = new(800);
        internal int canvas_width = 800;
        public override Vector2 ContainerSize { get => size; protected set { } }
        [DependencyCache(InteractionType.Upload)]
        internal NodeCanvas node_canvas = new() { origin = IPositioning.TopLeft, anchor = IPositioning.TopLeft };
        internal ImageDisplay display = new() { origin = IPositioning.TopRight, anchor = IPositioning.TopRight };
        public override void Init()
        {
            node_canvas.size = new(canvas_width, size.Y);
            AddChildSpawnQueue([node_canvas, display]);
        }

        protected override void Update()
        {
        }
    }
}
