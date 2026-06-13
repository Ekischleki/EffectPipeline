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
    internal class Node : GUIElement
    {
        [DependencyCache(InteractionType.Upload)]
        internal Node parentNode;
        public Node(IEffect effect, string title) 
        {
            parentNode = this;
            this.effect = effect;
            title_text = title;
            properties = effect.Properties;
        }
        const int HEIGHT_PER_PARAM = 20;
        internal string title_text;
        internal IEffect effect;
        internal List<Parameter> inputs = [];
        internal List<Parameter> outputs = [];
        internal int width;
        internal int height;
        internal TextGameObject title = null!;
        internal readonly GameObject[] properties;
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
            float prop_offset = 20.0f;
            foreach (var property in properties)
            {
                property.anchor = IPositioning.TopCenter;
                property.origin = IPositioning.TopCenter;
                AddChildSpawnQueue(property, prop => {
                    height += (int)prop.ContainerSize.Y + 5;
                    prop.offset = new(0, prop_offset);
                    prop_offset += (int)prop.ContainerSize.Y + 5;
                });
            }
            float y = -15;
            int i = 0;
            foreach (var input in effect.Inputs)
            {
                Parameter param = new () { parentNode = this, is_input = true, name = input.Item1, offset = new Vector2(0, y), type = input.Item2, index = i };
                y -= HEIGHT_PER_PARAM;
                inputs.Add(param);
                AddChildSpawnQueue(param);
                i++;
            }
            y = -15;
            i = 0;
            foreach (var output in effect.Outputs)
            {
                Parameter param = new () { parentNode = this,  is_input = false, name = output.Item1, offset = new Vector2(0, y), type = output.Item2, index = i };
                y -= HEIGHT_PER_PARAM;
                outputs.Add(param);
                AddChildSpawnQueue(param);
                i++;
            }
            height = Math.Max(inputs.Count, outputs.Count) * HEIGHT_PER_PARAM + 15;
            AddChildSpawnQueue(title);
            width = title.ContentDimensions.w + 30;
            height += title.ContentDimensions.h;
        }
 

        protected override void Update()
        {
            RenderTexture = (ManagedTexture)GetFrom(StoreType.PlaceholderTextureStore, $"generated/box/white/{width}/{height}");
            HandleMouseInteraction();
        }


        protected override void OnClick()
        {
            position = offset;

            Size *= 1.02;
        }

        protected override void OnRelease()
        {
            Size /= 1.02;
            position += camera.Cam_mouse_pos - mouseDragStart!.Value;
            offset = position;
        }

        protected override void OnDrag()
        {
            Vector2 new_offset = camera.Cam_mouse_pos - mouseDragStart!.Value;
            offset = position + new_offset;
            
        }

    }
}
