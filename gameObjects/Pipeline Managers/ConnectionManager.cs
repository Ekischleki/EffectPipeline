using EffectPipeline.Effects;
using EffectPipeline.GameObjects;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using Pandemonium.Engine.UIOI;
using Pupilmonium.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.gameObjects
{
    internal class ConnectionManager : GameObject
    {
        [GetFrom(Singleton.Mouse)]
        protected Mouse mouse = null!;

        internal Parameter? ClosestDragging = null;
        internal float ClosestDist = float.PositiveInfinity;

        PipelineManager pipelineManager;


        private Dictionary<Node, List<Node>> dependencies = new Dictionary<Node, List<Node>>();


        public ConnectionManager(PipelineManager _pipelineManager)
        {
            pipelineManager = _pipelineManager;
        }


        internal void RegisterDist(Parameter parameter)
        {

        }

        protected override void Update()
        {
            ClosestDragging = null;
            ClosestDist = float.PositiveInfinity;

            if (mouse.MouseEvent.HasFlag(MouseEvent.Right))
            {
                var n = pipelineManager.InstantiateNewNode(new SplitChannel(), "AAAAAAAAAAAAAAAAAAAAAAa");
                n.offset = mouse.Position;
            }
        }

        public override void Init()
        {

        }

        public void AddNode(Node newNode)
        {
            dependencies.Add(newNode, [newNode]);
        }

        public void RemoveNode(Node node)
        {
            dependencies.Remove(node);

            foreach(var n in dependencies.Keys) dependencies[n].Remove(node);
        }
    }
}
