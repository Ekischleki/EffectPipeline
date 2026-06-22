using Pandemonium.Engine;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using Pandemonium.Engine.UIOI;
using Pupilmonium.Framework;
using SDL2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.gameObjects.GUI_Elements
{
    internal class NumberInputProperty : GUIElement
    {
        [DependencyCache(InteractionType.Download)]
        internal Node parentNode = null!;
        [DependencyCache(InteractionType.Download)]
        internal NodeStateManager manager = null!;

        static char[] digits = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];

        TextGameObject display = null!;
        [GetFrom(StoreType.FontStore, "std:oxanium.ttf@10")]
        internal RenderedFont font = null!;

        private string title;

        bool focus = false;
        public int Value { get; set; }
        public int Max { get; init; }
        public int Min { get; init; }


        public NumberInputProperty(string title)
        {
            this.title = title;
        }

        public override void Init()
        {
            if (Value < Min)
                Value = Min;

            AddChildSpawnQueue(display = new() {
                Font = font, 
                Color = Color.White, 
                origin = IPositioning.Center, 
                anchor = IPositioning.Center,
                Text = Value.ToString()
            });
            RenderTexture = (ManagedTexture)GetFrom(StoreType.PlaceholderTextureStore, $"generated/box/gray/70/{display.ContentDimensions.h + 10}");
        }

        protected override void OnClick()
        {
            focus = true;
        }

        protected override void OnDrag()
        {
        }

        protected override void OnRelease()
        {
        }

        protected override void Update()
        {
            HandleMouseInteraction();
            if(!((IContainer)this).InContainer(mouse.Position)) {
                if (focus)
                {
                    Value = int.Clamp(Value, Min, Max);
                    display.Text = Value.ToString();
                    manager.UpdateCacheAt(parentNode);
                }
                focus = false;
            }
            if (focus)
            {
                for (int i = 0; i < digits.Length; i++)
                {
                    if (keyboard.ClickingKey((SDL.SDL_Keycode)digits[i]))
                    {
                        Value *= 10;
                        Value += i;
                        display.Text = Value.ToString();
                    }
                }
                if (keyboard.ClickingKey(SDL.SDL_Keycode.SDLK_BACKSPACE))
                {
                    Value /= 10;
                    display.Text = Value.ToString();
                }
            }
        }
    }
}
