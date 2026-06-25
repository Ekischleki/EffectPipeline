using EffectPipeline.gameObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EffectPipeline.project
{
    internal class Project
    {
        public List<NodeInformation> NodeInformation;


        public Project(NodeStateManager stateManager)
        {
            Dictionary<Node, int> nodeIds = new();
            var nodes = stateManager.Nodes.ToList();
            for (int i = 0; i < nodes.Count; i++)
            {
                nodeIds.Add(nodes[i], i);
            }
            NodeInformation = nodes.Select(node => new NodeInformation(node, nodeIds)).ToList();
        }

        public void Save()
        {
            //JsonSerializer.Serialize(this, new JsonSerializerOptions().)
        }
    }


    internal class NodeInformation
    {
        public string Effect;
        public float x, y;
        public List<string> props;
        //
        public List<Connection> Connections; 
        public NodeInformation(Node node, Dictionary<Node, int> connections)
        {
            x = node.offset.X;
            y = node.offset.Y;
            Effect = node.effect.GetType().AssemblyQualifiedName ?? throw new DataException($"Node with effect {node.effect.GetType().Name} cannot be serialized as its type does not have an Assembly qualified name.");
            props = node.properties.Select(prop => prop.Save()).ToList();
            Connections = [];
            foreach (var outgoingConnection in node.outputs)
            {
                foreach (var connection in outgoingConnection.connections)
                {
                    var end_node_idx = connections[connection.end.parentNode];
                    Connections.Add(new Connection() { start_parameter = outgoingConnection.index, end_node = end_node_idx, end_parameter = connection.end.index });
                }
            }
        }
    }

    internal class Connection
    {
        internal int start_parameter;
        internal int end_node;
        internal int end_parameter;
    }



    [JsonPolymorphic] // Optional configuration
    public interface IPropertySave { }
}
