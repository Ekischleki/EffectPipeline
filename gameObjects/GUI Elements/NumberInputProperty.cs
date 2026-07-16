using Pandemonium.Engine;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using Pandemonium.Engine.UIOI;
using Pupilmonium.Framework;
using SDL2;
using SimpleBinaryFormat;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.gameObjects.GUI_Elements
{
    internal class NumberInputProperty : Property
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

        string typingText = "";

        void HandleValueChange()
        {
            try
            {
                int inputtedNumber = int.Parse(typingText);

                Value = int.Clamp(inputtedNumber, Min, Max);
                display.Text = Value.ToString();
                manager.UpdatePropertyState(parentNode, this, GetPropertyState());
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
                if (typingText != "" && keyboard.ClickingKey(SDL.SDL_Keycode.SDLK_BACKSPACE))
                {
                    typingText.Remove(typingText.Length - 1);
                    display.Text = typingText;
                }
            }
        }
        public override bool TryLoad(IPropertyState val)
        {
            if (val is NumberInputPropertyState state)
            {
                if (state.Value < Min || state.Value > Max)
                {
                    return false;
                }
                Value = state.Value;
                display.Text = Value.ToString();
                return true;

            }
            return false;
        }

        public override IPropertyState GetPropertyState() => new NumberInputPropertyState(Value);
    }

    public class NumberInputPropertyState : IPropertyState
    {
        public int Value { get; private set; }

        public NumberInputPropertyState(int value)
        {
            Value = value;
        }
        public NumberInputPropertyState() { }

        public void FromReader(Region reader)
        {
            Value = reader.ReadInt("Value");
        }

        public async Task WriteToWriter(Writer writer)
        {
            await writer.WriteInt("Value", Value);
        }
    }
}
