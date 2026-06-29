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

    public class NodeState
    {
        public NodeState(Node node)
        {
            incoming_connections = new Connection[node.effect.Inputs.Count()];
            outgoing_connections = new HashSet<Connection>[node.effect.Outputs.Count()];
            for(int i = 0; i < outgoing_connections.Length; i++)
            {
                outgoing_connections[i] = [];
            }
            property_state = node.properties.ToDictionary(x => x, x => x.GetPropertyState());
            cancellationTokenSource = new();
        }
        public Connection?[] incoming_connections;
        public HashSet<Connection>[] outgoing_connections;
        public Dictionary<Property, IPropertyState> property_state;
        public IInstance[]? cached_data = null;
        public Exception? last_exception = null;
        //The version of the assigned task or of the data the assigned task produced.
        public CancellationTokenSource cancellationTokenSource;
        public Task<IInstance[]?>? assigned_task = null!;
    }

    public class NodeStateManager : GameObject
    {
        internal NodeState OutputNodeState { get; private set; }
        internal ManagedTexture? OutputImage { get; private set; }

        protected Dictionary<Node, NodeState> nodes = [];
        protected Node output_node = null!;
        public IReadOnlyCollection<Node> Nodes { get => nodes.Keys; }

        [GetFrom(Singleton.Mouse)]
        protected Mouse mouse = null!;

        internal Parameter? ClosestDragging = null;
        internal float ClosestDist = float.PositiveInfinity;


        public NodeStateManager()
        {
            
        }

        public void UpdatePropertyState(Node parent_node, Property property, IPropertyState new_state)
        {
            NodeState nodeState;
            lock(nodes)
            {
                nodeState = nodes[parent_node];
            }
            lock(nodeState)
            {
                nodeState.property_state[property] = new_state;
            }
            StartUpdateCacheAt(parent_node);
        }
        internal Node CreateNode(IEffect effect, string title)
        {
            if(output_node != null && effect is ImageOutput)
            {
                throw new ArgumentException("There can only be one end node per canvas");
            }
            var node = new Node(effect, title);
            nodes.Add(node, new(node));
            AddChildSpawnQueue(node, _ => StartUpdateCacheAt(node));
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
            OutputNodeState = nodes[output_node];
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
            NodeState state;
            lock(nodes)
            {
                state = nodes[node];
            
                AddDespawnQueue(node);
                lock(state)
                {
                    foreach (var output in state.outgoing_connections.SelectMany(n => n))
                    {
                        //This doesnt create a deadlock since this thread is allowed to lock the state as often as it needs
                        DeleteConnection(output);
                    }
                    foreach (var input in state.incoming_connections.Where(input => input != null))
                    {                
                        DeleteConnection(input!);
                    }
                }

                nodes.Remove(node);
            }
        }


        private void StartUpdateCacheAt(Node node, HashSet<Node>? visited = null)
        {
            //Only one update cache cycle may be started at a time.
            lock(this) lock (nodes)
            {
                visited ??= [];
                if (visited.Contains(node))
                {
                    return;
                }
                visited.Add(node);
                NodeState nodeState = nodes[node];
                lock (nodeState)
                {
                    nodeState.cancellationTokenSource.Cancel();
                    nodeState.cancellationTokenSource = new();
                    var token = nodeState.cancellationTokenSource.Token;
                    var task = Task.Run(() => UpdateCacheAt(node, token), token);
                    nodeState.assigned_task = task;
                    foreach (var output in nodeState.outgoing_connections)
                    {
                        foreach (var con in output)
                        {
                            StartUpdateCacheAt(con.end.parentNode);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="node">Node whose inputs have changed</param>
        internal async Task<IInstance[]?> UpdateCacheAt(Node node, CancellationToken token)
        {
            NodeState state;
            IInstance?[] inputs;
            Task?[] dependencies;
            IPropertyState[] properties;
            lock (nodes)
            {
                state = nodes[node];
                lock (state)
                {
                    properties = new IPropertyState[node.properties.Length];
                    //Fetch properties
                    int i = 0;
                    foreach (var property in node.properties)
                    {
                        properties[i] = state.property_state[property];
                        i++;
                    }

                    dependencies = new Task[state.incoming_connections.Length];
                    inputs = new IInstance?[state.incoming_connections.Length];

                    var effect_inputs = node.effect.Inputs.ToArray();

                    foreach (var input in state.incoming_connections)
                    {
                        if (input is null)
                        {
                            continue;
                        }
                        var input_parameter = input.end;
                        var originating_param = input.start;
                        //We depend on this node
                        var dependent_node_state = nodes[originating_param.parentNode];
                        lock (dependent_node_state)
                        {
                            var dependent_task = dependent_node_state.assigned_task;
                            if (dependent_task is null)
                            {
                                continue;
                            }
                            dependencies[input_parameter.index] = Task.Run(async () =>
                            {
                                inputs[input_parameter.index] = (await dependent_task)?[originating_param.index];
                            }, token);
                        }
                    }
                }
            }
            foreach (var dependency in dependencies.Where(d => d is not null))
            {
                await dependency!;
            }
            if (node.effect is ImageOutput)
            {
                var image = inputs[0]?.ToImage();
                if (image == null || image.width == 0 || image.height == 0)
                {
                    OutputImage = null;
                }
                else
                {
                    OutputImage = image.ToTexture(Game.Canvas);
                }
                return [];
            }
            IInstance[]? node_outputs = null;
            Exception? last_exception = null;
            try
            {
                node_outputs = await node.effect.applyEffect(inputs, properties);
            }
            catch (Exception ex)
            {
                defaultLogger.Error($"Exception happened in {node.GetType().Name}: {ex}");
                last_exception = ex;
            }
            lock(state)
            {
                token.ThrowIfCancellationRequested();
                state.cached_data = node_outputs;
                state.last_exception = last_exception;
            }
            return node_outputs;
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
            NodeState state;
            lock(nodes)
            {
                state = nodes[start];
            }
            lock(state)
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
                foreach(Connection connection in state.outgoing_connections.SelectMany(p => p))
                {
                    if (ContainsPathTo(connection.end.parentNode, destination, visited))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        bool CouldMakeConnection(Parameter end, Parameter start)
        {
            if (!start.type.IsAssignableTo(end.type))
            {
                return false;
            }

            return true;
        }

        void TryCreateConnectionFromInput(Parameter inputPar)
        {
            Node parent = inputPar.parentNode;

            lock(nodes)
            {
                var state = nodes[parent];
                
                Parameter? outputPar = null;

                foreach (var n in Nodes)
                {
                    //If there exists a path from the connection to the end, we cannot make the end dependent on the connection
                    if (ContainsPathTo(parent, n)) continue;


                    foreach (var p in parent.outputs)
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


        public void CreateConnection(Parameter end, Parameter start)
        {
            lock(nodes)
            {
                var source_state = nodes[start.parentNode];
                var dest_state = nodes[end.parentNode];
                lock (source_state) lock (dest_state)
                {
                    var connection = new Connection(end, start);
                    AddChildSpawnQueue(connection);
                    if (connection == null)
                    {
                        defaultLogger.Warn("Creation failed!");
                        return;
                    }

                    source_state.outgoing_connections[start.index].Add(connection);
                    dest_state.incoming_connections[end.index] = connection;

                    StartUpdateCacheAt(end.parentNode);
                }
            }
        }


        public void DeleteConnection(Connection connection)
        {
            NodeState start_state;
            NodeState end_state;
            lock(nodes)
            {
                start_state = nodes[connection.start.parentNode];
                end_state = nodes[connection.end.parentNode];
            }
            lock (start_state) lock (end_state)
            {
                start_state.outgoing_connections[connection.start.index].Remove(connection);
                end_state.incoming_connections[connection.end.index] = null;
                StartUpdateCacheAt(connection.end.parentNode);
                AddDespawnQueue(connection);
            }
        }

    }
}
