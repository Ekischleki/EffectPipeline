using EffectPipeline.GameObjects;
using Pandemonium.Engine;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using Pandemonium.Engine.UIOI;
using Pupilmonium.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.gameObjects
{
    internal class Connection : GUIElement
    {
        [DependencyCache(InteractionType.Download)]
        internal NodeStateManager manager = null!;
        public Parameter input { get; }
        public Parameter output { get; }


        public Connection(Parameter _input, Parameter _output)
        {
            input = _input;
            output = _output;
        }

        public override void Init()
        {

        }

        Vector2 beginPos = Vector2.Zero;
        Vector2 endPos = Vector2.Zero;

        protected override void Update()
        {
            beginPos = output.ContainerPosition + new Vector2(5, 5);
            endPos = input.ContainerPosition + new Vector2(5, 5);
            if(mouse.MouseEvent.HasFlag(MouseEvent.Right))
            {
                var line_dir = Vector2.Normalize(endPos - beginPos);
                var bm = mouse.Position - beginPos;
                var mouse_project_dist = Vector2.Dot(bm, line_dir);
                if(mouse_project_dist < 0 || mouse_project_dist > (endPos - beginPos).Length())
                {
                    return;
                }
                var mouse_foot = beginPos + mouse_project_dist * line_dir;
                var mouse_dist = (mouse.Position - mouse_foot).Length();
                defaultLogger.Info(mouse_dist);
                if (mouse_dist < 5)
                {
                    Delete();
                }
            }
        }

        internal void Delete()
        {
            manager.DeleteConnection(this);
        }

        protected override void Render()
        {
            base.Render();

            Draw.DrawLine(beginPos, endPos, 7, System.Drawing.Color.White, Game.Canvas);
        }

        protected override void OnClick()
        {
            Console.WriteLine("Bwa");
        }

        protected override void OnRelease()
        {
        }

        protected override void OnDrag()
        {
        }


    }
}
