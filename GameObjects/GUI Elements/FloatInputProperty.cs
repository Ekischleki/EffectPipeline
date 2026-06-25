using EffectPipeline.project;
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
using static EffectPipeline.gameObjects.GUI_Elements.DropdownProperty;

namespace EffectPipeline.gameObjects.GUI_Elements
{
    internal class FloatInputProperty : Property
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
        public float Value { get; set; }
        public float Max { get; init; }
        public float Min { get; init; }


        public FloatInputProperty(string title)
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

        string typingText = "";

        void HandleValueChange()
        {
            try
            {
                float inputtedNumber = float.Parse(typingText);

                Value = float.Clamp(inputtedNumber, Min, Max);
                display.Text = Value.ToString();
                manager.UpdateCacheAt(parentNode);
            }
            catch (FormatException)
            {

            }
        }

        protected override void Update()
        {
            HandleMouseInteraction();
            if(!((IContainer)this).InContainer(mouse.Position))
            {
                if (focus) HandleValueChange();

                typingText = "";
                focus = false;
            }
            if (focus)
            {
                for (int i = 0; i < digits.Length; i++)
                {
                    if (keyboard.ClickingKey((SDL.SDL_Keycode)digits[i]))
                    {
                        typingText += i.ToString();
                        display.Text = typingText;

                        Console.WriteLine(typingText);
                    }
                }
                if (keyboard.ClickingKey(SDL.SDL_Keycode.SDLK_PERIOD))
                {
                    typingText += ".";
                    display.Text = typingText;
                }
                if (typingText != "" && keyboard.ClickingKey(SDL.SDL_Keycode.SDLK_BACKSPACE))
                { 
                    typingText.Remove(typingText.Length - 1);
                    display.Text = typingText;
                }
            }
        }

        public override string Save()
        {
            return Value.ToString();
        }

        public override bool TryLoad(string val)
        {
            if (float.TryParse(val, out var parsed))
            {
                if(!float.IsFinite(parsed))
                {
                    return false;
                }
                if (parsed < Min || parsed > Max)
                {
                    return false;
                }
                Value = parsed;
                display.Text = Value.ToString();
                return true;
            }
            return false;
        }
    }
}
