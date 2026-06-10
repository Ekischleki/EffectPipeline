using EffectPipeline.Effects;
using EffectPipeline.GameObjects;
using Pandemonium.Engine;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using Pandemonium.Engine.UIOI;
using System.Numerics;
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

        // Node n is in dependencies[m] whenever an input of n is (possibly indirectly) connected to an output of m
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
        }

        public override void Init()
        {

        }


        protected override void Render()
        {
            base.Render();

            if (isCreatingConnection)
            {
                Draw.DrawLine(referenceParameter!.ContainerPosition + new Vector2(5, 5), mouse.Position, 7, System.Drawing.Color.White, Game.Canvas);
            }
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


        bool isCreatingConnection = false;
        Parameter? referenceParameter = null;
        
        public void StartCreatingConnection(Parameter par)
        {
            isCreatingConnection = true;
            referenceParameter = par;
        }


        public void TryCreateConnection()
        {
            if (referenceParameter == null) return;

            if (referenceParameter.is_input) TryCreateConnectionFromInput(referenceParameter);
            else TryCreateConnectionFromOutput(referenceParameter);

            isCreatingConnection = false;
            referenceParameter = null;
        }


        void TryCreateConnectionFromInput(Parameter inputPar)
        {
            Node parent = inputPar.parentNode;

            Parameter? outputPar = null;

            foreach (var n in pipelineManager.Nodes)
            {
                if (dependencies[n].Contains(parent))
                    continue;

                foreach (var p in n.outputs)
                {
                    if (!((IContainer)p).InContainer(mouse.Position) || p.type != inputPar.type)
                        continue;

                    outputPar = p;
                }
            }

            if (outputPar == null)
            {
                Console.WriteLine("Creation from input failed!");
                return;
            }

            CreateConnection(inputPar, outputPar);
        }


        void TryCreateConnectionFromOutput(Parameter outputPar)
        {
            Node parent = outputPar.parentNode;

            Parameter? inputPar = null;

            foreach (var n in pipelineManager.Nodes)
            {
                if (dependencies[parent].Contains(n))
                    continue;

                foreach (var p in n.inputs)
                {
                    if (!((IContainer)p).InContainer(mouse.Position) || p.type != outputPar.type)
                        continue;

                    inputPar = p;
                }
            }

            if (inputPar == null)
            {
                Console.WriteLine("Creation from output failed!");
                return;
            }

            CreateConnection(inputPar, outputPar);
        }


        public void CreateConnection(Parameter inputPar, Parameter outputPar)
        {
            var _ = pipelineManager.InstantiateNewConnection(inputPar, outputPar);
            if (_ == null)
            {
                Console.WriteLine("Creation failed!");
                return;
            }

            Node inNode = inputPar.parentNode;
            Node outNode = outputPar.parentNode;

            foreach (var n in pipelineManager.Nodes)
            {
                if (!dependencies[n].Contains(inNode)) continue;

                dependencies[n].AddRange(dependencies[outNode].FindAll(x => !dependencies[n].Contains(x)));
            }
        }
        
    }
}
