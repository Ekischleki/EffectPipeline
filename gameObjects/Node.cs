using EffectPipeline.GameObjects;
using Pandemonium.Engine;
using Pandemonium.Engine.GameObjectStuff;
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
    internal class Node : GameObject
    {
        public Node(IEffect effect, string title) 
        {
            this.effect = effect;
            title_text = title;
        }
        const int HEIGHT_PER_PARAM = 20;
        internal string title_text;
        internal IEffect effect;
        internal List<Parameter> inputs = [];
        internal List<Parameter> outputs = [];
        internal int width;
        internal int height;
        internal TextGameObject title = null!;
        Vector2? mouseDragLast = null;
        [GetFrom(StoreType.FontStore, "std:oxanium.ttf@15")]
        internal RenderedFont font = null!;
        [GetFrom(Singleton.Mouse)]
        Mouse mouse = null!;
        public override void Init()
        {
            title = new TextGameObject()
            {
                anchor = IPositioning.TopCenter,
                origin = IPositioning.TopCenter,
                offset = IPositioning.Absolute(0, 5.0f),
                Font = font,
                Text = title_text,
            };
            float y = -15;
            foreach (var input in effect.Inputs)
            {
                Parameter param = new Parameter() { is_input = true, name = input.Item1, offset = IPositioning.Absolute(0, y) };
                y -= HEIGHT_PER_PARAM;
                inputs.Add(param);
                AddChildSpawnQueue(param);
            }
            y = -15;
            foreach (var output in effect.Outputs)
            {
                Parameter param = new Parameter() { is_input = false, name = output.Item1, offset = IPositioning.Absolute(0, y) };
                y -= HEIGHT_PER_PARAM;
                outputs.Add(param);
                AddChildSpawnQueue(param);
            }
            height = Math.Max(inputs.Count, outputs.Count) * HEIGHT_PER_PARAM + 15;
            AddChildSpawnQueue(title);
            width = title.ContentDimensions.w + 30;
            height += title.ContentDimensions.h;
            RenderTexture = (ManagedTexture)GetFrom(StoreType.PlaceholderTextureStore, $"generated/box/white/{width}/{height}");
        }
 

        protected override void Update()
        {
            if (mouse.MouseEvent.HasFlag(MouseEvent.Left) && ((IContainer)this).InContainer(mouse.Position)) {
                if(mouseDragLast != null)
                {
                    Vector2 new_offset = mouse.Position - (Vector2)mouseDragLast;
                    offset = new Absolute(((Absolute)offset).position + new_offset);
                }
                mouseDragLast = mouse.Position;
            } else
            {
                mouseDragLast = null;
            }
        }
    }
}
