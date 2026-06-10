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
using System.Xml;
using System.Diagnostics;

namespace EffectPipeline.gameObjects
{
    internal class NodeStateManager : GameObject
    {
        protected List<Node> nodes = new List<Node>();
        public IReadOnlyCollection<Node> Nodes { get => nodes.AsReadOnly(); }

        [GetFrom(Singleton.Mouse)]
        protected Mouse mouse = null!;

        internal Parameter? ClosestDragging = null;
        internal float ClosestDist = float.PositiveInfinity;


        public NodeStateManager()
        {
            
        }


        internal Node CreateNode(IEffect effect, string title)
        {
            var node = new Node(effect, title);
            nodes.Add(node);
            AddChildSpawnQueue(node);
            return node;
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

        bool ContainsPathTo(Node start, Node destination, HashSet<Node>? visited = null)
        {
            visited ??= [];
            if (start == destination)
            {
                return true;
            }
            if (!visited.Add(start))
            {
                return false;
            }
            foreach(Connection connection in start.inputs.SelectMany(p => p.connections))
            {
                if (ContainsPathTo(connection.input.parentNode, destination, visited))
                {
                    return true;
                }
            }
            return false;
        }

        internal void DeleteNode(Node node) {
            nodes.Remove(node);
            AddDespawnQueue(node);
            foreach(var input in node.inputs.SelectMany(n => n.connections))
            {
                DeleteConnection(input);
            }
            foreach (var output in node.outputs.SelectMany(n => n.connections))
            {
                DeleteConnection(output);
            }
        }
        bool CouldMakeConnection(Parameter input, Parameter output)
        {
            if(input.type != output.type)
            {
                return false;
            }
           
            //This connection is already taken
            if(input.connections.Count > 0)
            {
                return false;
            }

            return true;
        }

        void TryCreateConnectionFromInput(Parameter inputPar)
        {
            Node parent = inputPar.parentNode;

            Parameter? outputPar = null;

            foreach (var n in Nodes)
            {

                //If there exists a path from the connection to the output, we cannot make the output dependent on the connection
                if (ContainsPathTo(parent, n))
                {
                    continue;
                }
                foreach (var p in n.outputs)
                {
                    if (((IContainer)p).InContainer(mouse.Position) && CouldMakeConnection(inputPar, p) )
                        outputPar = p;
                }
            }

            if (outputPar == null)
            {
                defaultLogger.Warn("Creation from connection failed!");
                return;
            }

            CreateConnection(inputPar, outputPar);
        }

        void TryCreateConnectionFromOutput(Parameter outputPar)
        {
            Node parent = outputPar.parentNode;

            Parameter? inputPar = null;

            foreach (var n in Nodes)
            {
                //If there exists a path from the connection to the output, we cannot make the output dependent on the connection
                if (ContainsPathTo(n, parent))
                {
                    continue;
                }

                foreach (var p in n.inputs)
                {
                    if (((IContainer)p).InContainer(mouse.Position) && CouldMakeConnection(p, outputPar))
                        inputPar = p;
                }
            }

            if (inputPar == null)
            {
                defaultLogger.Warn("Creation from output failed!");
                return;
            }

            CreateConnection(inputPar, outputPar);
        }


        public void CreateConnection(Parameter inputPar, Parameter outputPar)
        {
            var connection = new Connection(inputPar, outputPar);
            AddChildSpawnQueue(connection);
            if (connection == null)
            {
                defaultLogger.Warn("Creation failed!");
                return;
            }

            Node inNode = inputPar.parentNode;
            Node outNode = outputPar.parentNode;

            inputPar.connections.Add(connection);
            outputPar.connections.Add(connection);
        }


        public void DeleteConnection(Connection connection)
        {
            connection.input.connections.Remove(connection);
            Debug.Assert(connection.input.connections.Count == 0);
            connection.output.connections.Remove(connection);
            AddDespawnQueue(connection);
        }

    }
}
