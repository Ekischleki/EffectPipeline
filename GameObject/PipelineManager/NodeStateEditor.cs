using EffectPipeline.GameObjects.GUIElements;
using Pandemonium.Engine;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.SetupAttributes;
using Pandemonium.Engine.UIOI;
using Pupilmonium.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.GameObjects.PipelineManagers
{
    public class NodeStateEditor(NodeStateManager manager) : GameObject
    {
        public NodeStateManager manager = manager;

        internal readonly Dictionary<NodeState, GuiNode> nodes = [];

        public GuiNode OutputNode { get; private set; } = null!;
        public IReadOnlyCollection<GuiNode> Nodes { get => nodes.Values; }

        internal ManagedTexture? OutputImageTexture { get; private set; }
        [GetFrom(Singleton.Mouse)]
        protected Mouse mouse = null!;
        bool isCreatingConnection = false;
        GUIElements.GuiParameter? referenceParameter = null;

        public void StartCreatingConnection(GuiParameter par)
        {
            isCreatingConnection = true;
            referenceParameter = par;
        }
        public override void Init()
        {
            foreach (var nodeState in manager.NodeStates)
            {
                var node = new GuiNode(nodeState);
                AddChildSpawnQueue(node);
                nodes.Add(nodeState, node);
            }
        }

        protected override void Update()
        {
            //throw new NotImplementedException();
        }
        public void TryCreateConnection()
        {
            if (referenceParameter == null) return;

            if (referenceParameter.is_input) TryCreateConnectionFromInput(referenceParameter);
            else TryCreateConnectionFromOutput(referenceParameter);

            isCreatingConnection = false;
            referenceParameter = null;
        }

        Parameter ToParameter(GuiParameter parameter)
        {
            return new(parameter.parentNode.state, parameter.index);
        }

        void TryCreateConnectionFromInput(GuiParameter guiInputPar)
        {
            GuiNode node = guiInputPar.parentNode;
            NodeState nodeState = node.state;

            Parameter inputParameter = ToParameter(guiInputPar);
            Parameter? outputPar = null;

            foreach (var n in Nodes)
            {
                //If there exists a path from the connection to the end, we cannot make the end dependent on the connection
                //if (manager.ContainsPathTo(nodeState, n.state)) continue;


                foreach (var guiP in n.outputs)
                {
                    var param = ToParameter(guiP);
                    if (((IContainer)guiP).InContainer(mouse.Position) && manager.CouldMakeConnection(inputParameter, param))
                        outputPar = param;
                }
            }

            if (outputPar == null)
            {
                defaultLogger.Warn("Creation from connection failed!");
                return;
            }

            manager.CreateConnection(inputParameter, outputPar);

        }

        void TryCreateConnectionFromOutput(GuiParameter guiOutputPar)
        {
            GuiNode node = guiOutputPar.parentNode;
            NodeState nodeState = node.state;
            Parameter outputParameter = ToParameter(guiOutputPar);

            Parameter? inputPar = null;

            foreach (var n in Nodes)
            {
                //If there exists a path from the connection to the end, we cannot make the end dependent on the connection
                //if (manager.ContainsPathTo(nodeState, n.state)) continue; //Currently broken


                foreach (var guiP in n.inputs)
                {
                    var param = ToParameter(guiP);
                    if (((IContainer)guiP).InContainer(mouse.Position) && manager.CouldMakeConnection(param, outputParameter))
                        inputPar = param;
                }
            }

            if (inputPar == null)
            {
                defaultLogger.Warn("Creation from connection failed!");
                return;
            }

            manager.CreateConnection(inputPar, outputParameter);
        }

        protected override void Render()
        {
            base.Render();

            if (isCreatingConnection)
            {
                Draw.DrawLine(referenceParameter!.ContainerPosition + new Vector2(5, 5), mouse.Position, (byte)float.Clamp(7 * AbsoluteSize, 0, 255), System.Drawing.Color.White, Game.Canvas);
            }

            foreach(var connection in manager.connections)
            {
                var start_param = nodes[connection.start.Node].outputs[connection.start.Idx];
                var end_param = nodes[connection.end.Node].inputs[connection.end.Idx];
                Draw.DrawLine(start_param.AbsolutePosition, end_param.AbsolutePosition, (byte)float.Clamp(7 * AbsoluteSize, 0, 255), System.Drawing.Color.White, Game.Canvas);
            }
        }

        static internal bool PointInsideLine(Vector2 startPos, Vector2 endPos, Vector2 point)
        {
            // quick and dirty check
            if (point.X < Math.Min(endPos.X, startPos.X) || point.X > Math.Max(endPos.X, startPos.X)
                || point.Y < Math.Min(endPos.Y, startPos.Y) || point.Y > Math.Max(endPos.Y, startPos.Y))
                return false;

            var line_dir = Vector2.Normalize(endPos - startPos);
            var bm = point - startPos;
            var project_dist = Vector2.Dot(bm, line_dir);
            if (project_dist < 0 || project_dist > (endPos - startPos).Length())
            {
                return false;
            }
            var foot = startPos + project_dist * line_dir;
            var dist = (point - foot).Length();
            return dist < 5;
        }

        internal GuiNode CreateNode(IEffect effect)
        {
            var nodeState = manager.CreateNode(effect);
            var node = new GuiNode(nodeState);
            AddChildSpawnQueue(node);
            nodes.Add(nodeState, node);
            return node;
        }

        internal void DeleteNode(GuiNode guiNode)
        {
            AddDespawnQueue(guiNode);
            manager.DeleteNode(guiNode.state);
            nodes.Remove(guiNode.state);
        }

        internal void UpdatePropertyState(GuiNode parentNode, Property property, IPropertyState propertyState)
        {
            manager.UpdatePropertyState(parentNode.state, Array.IndexOf(parentNode.properties, property), propertyState);
        }
    }
}
