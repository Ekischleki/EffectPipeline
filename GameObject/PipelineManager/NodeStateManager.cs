using EffectPipeline.Effects;
using EffectPipeline.GameObjects;
using EffectPipeline.types;
using Pandemonium.Engine;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using Pandemonium.Engine.UIOI;
using Pupilmonium.Framework;
using SimpleBinaryFormat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EffectPipeline.GameObjects.PipelineManagers
{

    public class NodeState : ISerializable
    {
        public NodeState()
        {
            effect = null!;
            property_state = null!;
            cancellationTokenSource = new();
        }
        public NodeState(IEffect effect)
        {
            this.effect = effect;
            property_state = effect.Properties.Select(x => x.GetPropertyState()).ToArray();
            cancellationTokenSource = new();
        }
        public IEffect effect;
        public IPropertyState[] property_state;
        public Vector2 position;
        //The version of the assigned task or of the data the assigned task produced.


        public IInstance[]? cached_data = null;
        public Exception? last_exception = null;
        public CancellationTokenSource cancellationTokenSource;
        public Task<IInstance[]?>? assigned_task = null;

        public async Task WriteToWriter(Writer writer)
        {
            await writer.WriteString("Effect", effect.GetType().FullName!);
            await writer.WriteArray("PropertyState", property_state);
            await writer.WriteFloat("X", position.X);
            await writer.WriteFloat("Y", position.Y);
        }

        public void FromReader(Region reader)
        {
            var effectName = reader.ReadString("Effect");
            var effect = Program.AllEffects.FirstOrDefault(x => x.Key.FullName == effectName).Value;
            if (effect == null)
            {
                //All effect information like property state, connections and original effect type will be dropped.
                this.effect = new EffectOffline();
            } else
            {
                this.effect = effect;
            }
            try
            {
                this.property_state = Array.ConvertAll(reader.ReadObjectArr("PropertyState", this.effect.Properties.Select(prop => prop.GetPropertyState().GetType()).ToArray()), x => (IPropertyState)x);
            }
            catch (Exception)
            {
                //This happens when there's a mismatch between expected amount of property states vs parsed amount of property states, i.e. on Effect change. 
                //In this case we revert all properties to their default value.
                property_state = this.effect.Properties.Select(prop => prop.GetPropertyState()).ToArray();
            }
            float x = reader.ReadFloat("X");
            float y = reader.ReadFloat("Y");
            position = new(x, y);
        }
    }

    public record Parameter : ISerializable
    {
        public NodeState Node;
        public int Idx;

        public Type Type(bool isOutput)
        {
            var typeEnum = isOutput ? Node.effect.Outputs : Node.effect.Inputs;
            //I dont care
            return typeEnum.ToArray()[Idx].Item2;
        }
        public Parameter()
        {
            Node = null!;
            Idx = 0;
        }

        public Parameter(NodeState node, int parameterIdx)
        {
            Node = node;
            Idx = parameterIdx;
        }

        public void FromReader(Region reader)
        {
            Node = reader.ReadObject<NodeState>("Node");
            Idx = reader.ReadInt("Idx");
        }

        public async Task WriteToWriter(Writer writer)
        {
            await writer.WriteObject("Node", Node);
            await writer.WriteInt("Idx", Idx);
        }
    }
    public class Connection : ISerializable
    {
        public Parameter start;
        public Parameter end;

        public Connection(Parameter start, Parameter end)
        {
            this.start = start;
            this.end = end;
        }

        public Connection(NodeState start, int startIdx, NodeState end,  int endIdx) 
            : this(new Parameter(start, startIdx), new Parameter(end, endIdx))
        {}

        public Connection()
        {
            start = null!;
            end = null!;
        }

        public void FromReader(Region reader)
        {
            start = reader.ReadObject<Parameter>("Start");
            end = reader.ReadObject<Parameter>("End");
        }

        public async Task WriteToWriter(Writer writer)
        {
            await writer.WriteObject("Start", start);
            await writer.WriteObject("End", end);
        }
    }

    public class NodeStateManager : ISerializable
    {
        private List<NodeState> Nodes = [];

        internal IReadOnlyList<NodeState> NodeStates => Nodes.AsReadOnly();
        //Maps an input parameter to the connection it is connected to. This is useful because every input Parameter can only have one connection associated with it.
        private Dictionary<Parameter, Connection> connection_ends = [];
        private RGBImage? outputImage;

        public IEnumerable<Connection> connections => connection_ends.Values;

        internal NodeState OutputNodeState { get; private set; }

        internal RGBImage? OutputImage
        {
            get => outputImage; 
            private set
            {
                outputImage = value;
                OutputImageChanged?.Invoke(value);
            }
        }

        public event Action<RGBImage?>? OutputImageChanged;
        public NodeStateManager()
        {
            OutputNodeState = CreateNode(new ImageOutput());
        }

        public void UpdatePropertyState(NodeState parent_node, int property, IPropertyState new_state)
        {
            lock(parent_node)
            {
                parent_node.property_state[property] = new_state;
            }
            StartUpdateCacheAt(parent_node);
        }
        internal NodeState CreateNode(IEffect effect)
        {
            var node = new NodeState(effect);
            Nodes.Add(node);
            StartUpdateCacheAt(node);
            return node;
        }

        internal void DeleteNode(NodeState node)
        {
            lock(Nodes)
            {       
                

                Nodes.Remove(node);
            }
        }


        private void StartUpdateCacheAt(NodeState node, HashSet<NodeState>? visited = null)
        {
            visited ??= [];
            if (visited.Contains(node))
            {
                return;
            }
            visited.Add(node);
            //Only one update cache cycle may be started at a time.
            lock(this)
            {
                lock (node)
                {
                    node.cancellationTokenSource.Cancel();
                    node.cancellationTokenSource = new();
                    var token = node.cancellationTokenSource.Token;
                    var task = Task.Run(() => UpdateCacheAt(node, token), token);
                    node.assigned_task = task;
                    foreach (var con in getOutgoingConnections(node))
                    {
                        StartUpdateCacheAt(con.end.Node);
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="node">Node whose inputs have changed</param>
        internal async Task<IInstance[]?> UpdateCacheAt(NodeState node, CancellationToken token)
        {
            IInstance?[] inputs;
            Task?[] dependencies;
            IPropertyState[] properties;
            Connection[] incomingConnections;
            lock (node)
            {
                properties = (IPropertyState[])node.property_state.Clone();

                incomingConnections = getIncomingConnections(node).ToArray();

                dependencies = new Task[incomingConnections.Length];
                inputs = new IInstance?[node.effect.Inputs.Count()];
            }

            foreach (var input in incomingConnections)
            {
                var input_parameter = input.end;
                var originating_param = input.start;
                //We depend on this node
                var dependent_node_state = originating_param.Node;
                lock (dependent_node_state)
                {
                    var dependent_task = dependent_node_state.assigned_task;
                    if (dependent_task is null)
                    {
                        continue;
                    }
                    dependencies[input_parameter.Idx] = Task.Run(async () =>
                    {
                        inputs[input_parameter.Idx] = (await dependent_task)?[originating_param.Idx];
                    }, token);
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
                    OutputImage = image;
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
                last_exception = ex;
            }
            lock(node)
            {
                token.ThrowIfCancellationRequested();
                node.cached_data = node_outputs;
                node.last_exception = last_exception;
            }
            return node_outputs;
        }

        IEnumerable<Connection> getIncomingConnections(NodeState state)
        {
            lock(connection_ends)
            {
                int i = 0;
                foreach (var _ in state.effect.Inputs)
                {
                    if (connection_ends.TryGetValue(new Parameter(state, i), out Connection? connection))
                    {
                        yield return connection;
                    }
                    i++;
                }
            }
        }

        IEnumerable<Connection> getOutgoingConnections(NodeState state)
        {
            lock (connection_ends)
            {
                foreach (var connection in connection_ends.Values)
                {
                    if (connection.start.Node == state)
                    {
                        yield return connection;
                    }
                }
            }
        }

        public bool ContainsPathTo(NodeState start, NodeState destination, HashSet<NodeState>? visited = null)
        {
            lock(start) lock(connection_ends)
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

                foreach(var connection in getIncomingConnections(start))
                {
                    if (ContainsPathTo(connection.end.Node, destination, visited))
                    {
                        return true;
                    }
                }
                return false;
            }
        }



        public bool CouldMakeConnection(Parameter end, Parameter start)
        {
            if (!start.Type(true).IsAssignableTo(end.Type(false)))
            {
                return false;
            }

            return true;
        }

        public void CreateConnection(Parameter end, Parameter start)
        {
            var source_state = start.Node;
            var dest_state = end.Node;
            lock (source_state) lock (dest_state) lock(connection_ends)
            {
                var connection = new Connection(start, end);
                connection_ends[end] = connection;
            }
            StartUpdateCacheAt(end.Node);
        }
        public void DeleteConnection(Connection connection)
        {
            lock(connection_ends)
            {
                connection_ends.Remove(connection.end);
            }
            StartUpdateCacheAt(connection.end.Node);
        }

        public async Task WriteToWriter(Writer writer)
        {
            await writer.WriteArray("Connections", connection_ends.Values.ToArray());
            await writer.WriteArray("Nodes", Nodes.ToArray());
        }

        public void FromReader(Region reader)
        {
            throw new NotImplementedException();
        }
    }
}
