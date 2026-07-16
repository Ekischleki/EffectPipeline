using EffectPipeline.Effects;
using EffectPipeline.gameObjects;
using SimpleBinaryFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.persist
{
    public class Project : ISerializable
    {
        NodeStateManager nodeStateManager;
        Dictionary<Node, NodeState> nodes_mapping = [];
        Dictionary<NodeState, Node> state_mapping = [];
        NodeState[]? nodes;
        ConnectionState[]? connections;
        public Project() { }

        internal Project(NodeStateManager nodeStateManager)
        {
            this.nodeStateManager = nodeStateManager;
        }

        public static async Task SaveTo(Stream stream, NodeStateManager state)
        {
            var proj = new Project(state);
            await Writer.EncodeTo(stream, proj);
            await stream.FlushAsync();
        }
        public static async Task LoadFrom(Stream stream, NodeStateManager state)
        {
            var proj = await Reader.Deserialize<Project>(stream);
            proj.nodeStateManager = state;
            proj.Apply();
        }

        public void FromReader(Region reader)
        {
            nodes = reader.ReadObjectArr<NodeState>("Nodes");
            connections = reader.ReadObjectArr<ConnectionState>("Connections");
        }


        public void Apply()
        {
            for (int i = 0; i < nodes!.Length; i++)
            {
                NodeState? node = nodes![i];
                node.nodeStateManager = nodeStateManager;
                node.state_mapping = state_mapping;
                if(i == nodes!.Length - 1)
                {
                    node.Apply(_ =>
                    {
                        foreach (var connection in connections!)
                        {
                            connection.state_mapping = state_mapping;
                            connection.nodeStateManager = nodeStateManager;
                            connection.Apply();
                        }
                    });
                } else
                {
                    node.Apply();
                }
            }

           
        }

        public async Task WriteToWriter(Writer writer)
        {
            
            var nodes = nodeStateManager.Nodes.Select(node => new NodeState() { node = node, nodeStateManager = nodeStateManager }).ToArray();
            this.nodes_mapping = nodes.ToDictionary(x => x.node!);
            foreach (var node in nodes)
            {
                node.node_mapping = this.nodes_mapping;
                node.nodeStateManager = nodeStateManager;
            }
            await writer.WriteArray("Nodes", nodes);
            var connections = nodeStateManager.nodes.Values.SelectMany(x => x.incoming_connections.Where(c => c is not null)).Select(x => new ConnectionState() { node_mapping = nodes_mapping, connection = x, nodeStateManager = nodeStateManager });
            await writer.WriteArray("Connections", connections.ToArray());
        }

        class ConnectionState : ISerializable
        {
            public Connection? connection;
            public Dictionary<Node, NodeState>? node_mapping;
            public Dictionary<NodeState, Node>? state_mapping;
            public NodeStateManager? nodeStateManager;
            NodeState? startNode;
            NodeState? endNode;
            int startIdx, endIdx;
            public void FromReader(Region reader)
            {
                if(reader.ReadInt("IsNull") == 1)
                {
                    return;
                }
                startIdx = reader.ReadInt("StartIdx");
                endIdx = reader.ReadInt("EndIdx");
                startNode = reader.ReadObject<NodeState>("StartNode");
                endNode = reader.ReadObject<NodeState>("EndNode");
            }

            public void Apply()
            {
                if (this.startNode is null)
                {
                    return;
                }
                Node startNode = state_mapping![this.startNode!];
                Node endNode = state_mapping![this.endNode!];
                nodeStateManager!.CreateConnection(endNode.inputs[endIdx], startNode.outputs[startIdx]);
            }

            public async Task WriteToWriter(Writer writer)
            {
                if(connection is null)
                {
                    await writer.WriteInt("IsNull", 1);
                } else
                {
                    await writer.WriteInt("IsNull", 0);
                }
                await writer.WriteInt("StartIdx", connection!.start.index);
                await writer.WriteObject("StartNode", node_mapping![connection.start.parentNode]);
                await writer.WriteInt("EndIdx", connection.end.index);
                await writer.WriteObject("EndNode", node_mapping![connection.end.parentNode]);
            }
        }

        class NodeState : ISerializable
        {
            public Node? node;
            public NodeStateManager nodeStateManager = null!;
            public Dictionary<Node, NodeState>? node_mapping;
            public Dictionary<NodeState, Node>? state_mapping;

            ISerializable[]? states;
            Vector2 position = new();
            string? search;
            public void FromReader(Region reader)
            {
                search = reader.ReadString("Search");
                
                float x = reader.ReadFloat("X");
                float y = reader.ReadFloat("Y");
                position = new(x, y);
                if (search != "Output")
                {
                    var effect = Program.DefaultEffectSearch.First(s => s.GetType().FullName == search).CreateEffect();
                    states = reader.ReadObjectArr("PropertyStates", effect.Properties.Select(prop => prop.GetPropertyState().GetType()).ToArray());
                } else
                {
                    states = [];
                }
            }

            public void Apply(Action<Node>? callback = null)
            {
                if (search == "Output")
                {
                    node = nodeStateManager.OutputNode;
                    node.offset = position;
                }
                else
                {
                    var effectSearch = Program.DefaultEffectSearch.First(s => s.GetType().FullName == search);
                    node = nodeStateManager.CreateNode(effectSearch.CreateEffect(), effectSearch.Title, effectSearch, node =>
                    {
                        foreach (var (property, state) in node!.properties.Zip(states!))
                        {
                            if (property.TryLoad((IPropertyState)state))
                            {
                                nodeStateManager.UpdatePropertyState(node, property, (IPropertyState)state);
                            }
                        }
                        callback?.Invoke(node);
                    });
                }

                node.position = position;
                state_mapping!.Add(this, node);


            }

            public async Task WriteToWriter(Writer writer)
            {
                await writer.WriteString("Search", node!.originSearch is null ? "Output" : node.originSearch.FullName!);
                await writer.WriteFloat("X", node.position.X);
                await writer.WriteFloat("Y", node.position.Y);
                var propertyStates = node.properties.Select(p => p.GetPropertyState()).ToArray();
                await writer.WriteArray("PropertyStates", propertyStates);
                await writer.WriteArray("Connections", nodeStateManager.nodes[node].incoming_connections.Select(c => new ConnectionState() { connection = c, nodeStateManager = nodeStateManager, node_mapping = node_mapping}).ToArray());
            }
        }
    }

}
