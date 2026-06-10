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
    internal class PipelineManager : GameObject
    {

        protected List<Node> nodes = new List<Node>();
        public IReadOnlyCollection<Node> Nodes { get => nodes.AsReadOnly(); }


        protected List<Connection> connections = new List<Connection>();
        public IReadOnlyCollection<Connection> Connections { get => connections.AsReadOnly(); }

        public ConnectionManager ConnectionManager { get;  }

        public PipelineManager()
        {
            ConnectionManager = new ConnectionManager(this);
            AddChildSpawnQueue(ConnectionManager);
        }

        public override void Init()
        {
        }

        protected override void Update()
        {
        }


        public Node InstantiateNewNode(IEffect _effect, string _title)
        {
            Node newNode = new Node(_effect, _title, this)
            {
                anchor = IPositioning.Center,
                origin = IPositioning.Center
            };

            nodes.Add(newNode);
            ConnectionManager.AddNode(newNode);

            AddChildSpawnQueue(newNode);

            return newNode;
        }


        public void DeleteNode(Node node)
        {
            if (!nodes.Contains(node))
                return;

            nodes.Remove(node);
            ConnectionManager.RemoveNode(node);
        }


        public Connection? InstantiateNewConnection(Parameter _input, Parameter _output)
        {
            if (_input.type != _output.type ||
                connections.Find((Connection x) => x.input == _input) != null) 
                return null;

            Connection newConnection = new Connection(_input, _output);

            connections.Add(newConnection);

            AddChildSpawnQueue(newConnection);

            return newConnection;
        }

    }
}
