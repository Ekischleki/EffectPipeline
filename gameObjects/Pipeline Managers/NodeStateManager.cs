using EffectPipeline.Effects;
using EffectPipeline.types;
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
        internal ManagedTexture? OutputImage { get; private set; }

        protected Dictionary<Node, IInstance[]?> nodes = [];
        protected Node output_node = null!;
        public IReadOnlyCollection<Node> Nodes { get => nodes.Keys; }

        [GetFrom(Singleton.Mouse)]
        protected Mouse mouse = null!;

        internal Parameter? ClosestDragging = null;
        internal float ClosestDist = float.PositiveInfinity;


        public NodeStateManager()
        {
            
        }


        internal Node CreateNode(IEffect effect, string title)
        {
            if(output_node != null && effect is ImageOutput)
            {
                throw new ArgumentException("There can only be one end node per canvas");
            }
            var node = new Node(effect, title);
            nodes.Add(node, null);
            AddChildSpawnQueue(node, _ => UpdateCacheAt(node));
            
            return node;
        }


        protected override void Update()
        {
            ClosestDragging = null;
            ClosestDist = float.PositiveInfinity;
        }

        public override void Init()
        {
            output_node = CreateNode(new ImageOutput(), "Output");
        }


        protected override void Render()
        {
            base.Render();

            if (isCreatingConnection)
            {
                Draw.DrawLine(referenceParameter!.ContainerPosition + new Vector2(5, 5), mouse.Position, (byte)float.Clamp(7 * AbsoluteSize, 0, 255), System.Drawing.Color.White, Game.Canvas);
            }
        }



        bool isCreatingConnection = false;
        Parameter? referenceParameter = null;
        
        public void StartCreatingConnection(Parameter par)
        {
            isCreatingConnection = true;
            referenceParameter = par;
        }

        internal void DeleteNode(Node node)
        {
            nodes.Remove(node);
            AddDespawnQueue(node);
            foreach (var input in node.inputs.SelectMany(n => n.connections))
            {
                DeleteConnection(input);
            }
            foreach (var output in node.outputs.SelectMany(n => n.connections))
            {
                DeleteConnection(output);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node">Node whose inputs have changed, meaning we must recompute subsequent nodes</param>
        public void UpdateCacheAt(Node node)
        {
            IInstance?[] inputs = new IInstance?[node.inputs.Count];
            var effect_inputs = node.effect.Inputs.ToArray();
            foreach (var input in node.inputs)
            {
                Debug.Assert(input.connections.Count <= 1);
                if(input.connections.Count == 0)
                {
                    continue;
                }
                var input_param = input.connections.AsEnumerable().First().start;
                var input_value = nodes[input_param.parentNode]![input_param.index];
                if (input_value == null)
                {
                    continue;
                }
                var expected_type = input.type;
                if (input_value.Type != expected_type)
                {
                    input_value = input_value.Into(expected_type);
                }
                inputs[input.index] = input_value;
            }
            if(node.effect is ImageOutput)
            {
                var image = inputs[0]?.ToImage();
                if(image == null || image.width == 0 || image.height == 0)
                {
                    OutputImage = null;
                } else
                {
                    OutputImage = image.ToTexture(Game.Canvas);
                }
            }
            try
            {
                nodes[node] = node.effect.applyEffect(inputs, node.properties);

            } catch (Exception ex)
            {
                nodes[node] = node.effect.Outputs.Select(x => (IInstance?)null).ToArray()!;
                defaultLogger.Error($"Exception happened in {node.GetType().Name}: {ex}");
            }
            foreach(var output in node.outputs)
            {
                foreach (var con in output.connections)
                {
                    UpdateCacheAt(con.end.parentNode);
                }
            }
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
                if (ContainsPathTo(connection.end.parentNode, destination, visited))
                {
                    return true;
                }
            }
            return false;
        }

        bool CouldMakeConnection(Parameter end, Parameter start)
        {
            if (end.type != start.type && !nodes[start.parentNode]![start.index].SupportInto(end.type))
            {
                return false;
            }
           
            //This connection is already taken
            if(end.connections.Count > 0)
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
                //If there exists a path from the connection to the end, we cannot make the end dependent on the connection
                if (ContainsPathTo(parent, n)) continue;


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
                //If there exists a path from the connection to the end, we cannot make the end dependent on the connection
                if (ContainsPathTo(n, parent)) continue;

                foreach (var p in n.inputs)
                {
                    if (((IContainer)p).InContainer(mouse.Position) && CouldMakeConnection(p, outputPar))
                        inputPar = p;
                }
            }

            if (inputPar == null)
            {
                defaultLogger.Warn("Creation from end failed!");
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

            UpdateCacheAt(inputPar.parentNode);
        }


        public void DeleteConnection(Connection connection)
        {
            connection.end.connections.Remove(connection);
            Debug.Assert(connection.end.connections.Count == 0);
            connection.start.connections.Remove(connection);
            UpdateCacheAt(connection.end.parentNode);
            AddDespawnQueue(connection);
        }

    }
}
