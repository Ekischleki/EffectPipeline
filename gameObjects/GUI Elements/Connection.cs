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
    public class Connection : GUIElement
    {
        [DependencyCache(InteractionType.Download)]
        internal NodeStateManager manager = null!;
        public Parameter end { get; }
        public Parameter start { get; }


        public Connection(Parameter start, Parameter end)
        {
            this.end = start;
            this.start = end;
        }

        public override void Init()
        {

        }

        Vector2 beginPos = Vector2.Zero;
        Vector2 endPos = Vector2.Zero;

        protected override void Update()
        {
            beginPos = start.ContainerPosition + new Vector2(5, 5);
            endPos = end.ContainerPosition + new Vector2(5, 5);

            if(mouse.MouseEvent.HasFlag(MouseEvent.Right))
            {
                if (PointInsideLine(mouse.Position))
                {
                    Delete();
                }
            }
        }

        internal bool PointInsideLine(Vector2 point)
        {
            // quick and dirty check
            if (point.X < Math.Min(endPos.X, beginPos.X) || point.X > Math.Max(endPos.X, beginPos.X)
                || point.Y < Math.Min(endPos.Y, beginPos.Y) || point.Y > Math.Max(endPos.Y, beginPos.Y))
                return false;

            var line_dir = Vector2.Normalize(endPos - beginPos);
            var bm = point - beginPos;
            var project_dist = Vector2.Dot(bm, line_dir);
            if (project_dist < 0 || project_dist > (endPos - beginPos).Length())
            {
                return false;
            }
            var foot = beginPos + project_dist * line_dir;
            var dist = (mouse.Position - foot).Length();
            defaultLogger.Info(dist);

            return dist < 5;
        }

        internal void Delete()
        {
            manager.DeleteConnection(this);
        }

        protected override void Render()
        {
            base.Render();

            Draw.DrawLine(beginPos, endPos, (byte)float.Clamp(7 * AbsoluteSize, 0, 255), System.Drawing.Color.White, Game.Canvas);
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
