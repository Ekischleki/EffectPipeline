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
        }

        protected override void Render()
        {
            base.Render();

            Draw.DrawLine(beginPos, endPos, 7, System.Drawing.Color.White, Game.Canvas);
        }

        protected override void OnClick()
        {
        }

        protected override void OnRelease()
        {
        }

        protected override void OnDrag()
        {
        }


    }
}
