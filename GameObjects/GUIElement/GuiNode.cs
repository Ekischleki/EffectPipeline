using EffectPipeline.Effects;
using EffectPipeline.GameObjects.GUIElements;
using EffectPipeline.GameObjects;
using EffectPipeline.GameObjects.PipelineManagers;
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

namespace EffectPipeline.GameObjects.GUIElements
{
    public class GuiNode : GUIElement
    {
        [DependencyCache(InteractionType.Upload)]
        internal GuiNode parentNode;
        [DependencyCache(InteractionType.Download)]
        internal NodeStateEditor editor = null!;
        public GuiNode(NodeState state) 
        {
            parentNode = this;
            this.state = state;
            title_text = state.effect.Title;
            properties = state.effect.Properties;
            position = state.position;
        }
        const int HEIGHT_PER_PARAM = 20;
        internal string title_text;
        internal readonly NodeState state;
        internal List<GuiParameter> inputs = [];
        internal List<GuiParameter> outputs = [];
        internal int width;
        internal int height;
        internal TextGameObject title = null!;
        internal readonly Property[] properties;
        [GetFrom(StoreType.FontStore, "std:oxanium.ttf@15")]
        internal RenderedFont font = null!;

        /// <summary>
        /// The internally stored position differs from the rendered position. Internally stored position is used for saving and loading
        /// and only updated when a dragging operation is complete
        /// </summary>
        internal Vector2 position;
        public override void Init()
        {
            offset = position;

            title = new TextGameObject()
            {
                anchor = IPositioning.TopCenter,
                origin = IPositioning.TopCenter,
                offset = new(0, 5.0f),
                Font = font,
                Text = title_text,
            };
            float prop_offset = 20.0f;
            for (int j = 0; j < state.property_state.Length; j++)
            {
                var propertyState = state.property_state[j];
                var property = properties[j];
                property.anchor = IPositioning.TopCenter;
                property.origin = IPositioning.TopCenter;
                AddChildSpawnQueue(property, prop => {
                    height += (int)((IContainer)prop).ContainerSize.Y + 5;
                    prop.offset = new(0, prop_offset);
                    prop_offset += (int)prop.ContainerSize.Y + 5;
                    property.TryLoad(propertyState);
                });
            }
            float y = -15;
            int i = 0;
            foreach (var input in state.effect.Inputs)
            {
                GuiParameter param = new () { parentNode = this, is_input = true, name = input.Item1, offset = new Vector2(0, y), type = input.Item2, index = i };
                inputs.Add(param);
                y -= HEIGHT_PER_PARAM;
                AddChildSpawnQueue(param);
                i++;
            }
            y = -15;
            i = 0;
            foreach (var output in state.effect.Outputs)
            {
                GuiParameter param = new () { parentNode = this,  is_input = false, name = output.Item1, offset = new Vector2(0, y), type = output.Item2, index = i };
                outputs.Add(param);
                y -= HEIGHT_PER_PARAM;
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
            state.position = position;
        }

        protected override void OnRelease()
        {
            position += camera.Cam_mouse_pos - mouseDragStart!.Value;
            //Doesn't need locking because we're the only one reading it anyways
            state.position = position;
            offset = position;
        }

        protected override void OnDrag()
        {
            Vector2 new_offset = camera.Cam_mouse_pos - mouseDragStart!.Value;
            offset = position + new_offset;
            if(state.effect is not ImageOutput && (
                mouse.MouseEvent.HasFlag(MouseEvent.Right) ||
                keyboard.ClickingKey(SDL2.SDL.SDL_Keycode.SDLK_BACKSPACE) || 
                keyboard.ClickingKey(SDL2.SDL.SDL_Keycode.SDLK_ESCAPE) ||
                keyboard.ClickingKey(SDL2.SDL.SDL_Keycode.SDLK_DELETE)
                ))
            {
                editor.DeleteNode(this);
            }
        }

    }
}
