using Pandemonium.Engine;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using Pandemonium.Engine.UIOI;
using SDL2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.GameObjects
{
    internal class ProjNameEntry : GameObject
    {
        const int MAX_NAME_LENGTH = 32;
        [GetFrom(StoreType.FontStore, "std:oxanium.ttf@25")]
        RenderedFont font = null!;

        const string READ_KEYS = "abcdefghijklmnopqrstuvwxyz0123456789_- ";

        [GetFrom(Singleton.Keyboard)]
        Keyboard keyboard = null!;

        TextGameObject input = null!;

        public required Action<string> OnConfirm;

        public override Vector2 ContainerSize { get => parent.ContainerSize; protected set => base.ContainerSize = value; }
        public override void Init()
        {
            AddChildSpawnQueue([
                new TextGameObject() {
                    Font = font,
                    Text = "Enter the name of your project",
                    origin = IPositioning.Center,
                    anchor = IPositioning.Center,
                    offset = new(0, -128),
                    Color = Color.White
                },
                input = new TextGameObject() {
                    Font = font,
                    origin = IPositioning.Center,
                    anchor = IPositioning.Center,
                    Color = Color.White
                }
                ]);
        }

        protected override void Update()
        {
            foreach(char k in READ_KEYS)
            {
                var key = k;
                if (!keyboard.ClickingKey((SDL.SDL_Keycode)key))
                {
                    continue;
                }
                if (keyboard.HoldingKey(SDL.SDL_Keycode.SDLK_LSHIFT))
                {
                    key = key.ToString().ToUpper()[0];
                }
                if(input.Text.Length < MAX_NAME_LENGTH)
                input.Text += key;
            }
            if (keyboard.ClickingKey(SDL.SDL_Keycode.SDLK_BACKSPACE) && input.Text != string.Empty)
            {
                input.Text = input.Text[..(input.Text.Length - 1)];
            }
            if(keyboard.ClickingKey(SDL.SDL_Keycode.SDLK_RETURN))
            {
                OnConfirm(input.Text);
            }
        }
    }
}
