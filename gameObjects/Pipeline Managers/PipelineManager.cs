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
        public Node[] Nodes { get => nodes.ToArray(); }


        ConnectionManager connectionManager;

        public PipelineManager()
        {
            connectionManager = new ConnectionManager(this);
            AddChildSpawnQueue(connectionManager);
        }

        public override void Init()
        {
        }

        protected override void Update()
        {
        }


        public Node InstantiateNewNode(IEffect _effect, string _title)
        {
            Node newNode = new Node(_effect, _title)
            {
                anchor = IPositioning.Center,
                origin = IPositioning.Center
            };

            nodes.Add(newNode);
            connectionManager.AddNode(newNode);

            AddChildSpawnQueue(newNode);

            return newNode;
        }

    }
}
