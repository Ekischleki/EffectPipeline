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
    internal class Node : GUIElement
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

        [GetFrom(StoreType.FontStore, "std:oxanium.ttf@15")]
        internal RenderedFont font = null!;

        /// <summary>
        /// The internally stored position differs from the rendered position. Internally stored position is used for saving and loading
        /// and only updated when a dragging operation is complete
        /// </summary>
        internal Vector2 position;
        public override void Init()
        {
            position = offset;

            title = new TextGameObject()
            {
                anchor = IPositioning.TopCenter,
                origin = IPositioning.TopCenter,
                offset = new(0, 5.0f),
                Font = font,
                Text = title_text,
            };
            float y = -15;
            foreach (var input in effect.Inputs)
            {
                Parameter param = new Parameter() { parentNode = this, is_input = true, name = input.Item1, offset = new Vector2(0, y) };
                y -= HEIGHT_PER_PARAM;
                inputs.Add(param);
                AddChildSpawnQueue(param);
            }
            y = -15;
            foreach (var output in effect.Outputs)
            {
                Parameter param = new Parameter() { parentNode = this,  is_input = false, name = output.Item1, offset = new Vector2(0, y) };
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
            HandleMouseInteraction();
        }


        protected override void OnClick()
        {
            Size *= 1.02;
        }

        protected override void OnRelease()
        {
            Size /= 1.02;
            position += mouse.Position - mouseDragStart!.Value;
            offset = position;
        }

        protected override void OnDrag()
        {
            Vector2 new_offset = mouse.Position - mouseDragStart!.Value;
            offset = position + new_offset;
        }

    }
}
