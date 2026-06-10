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
    internal abstract class GUIElement : GameObject
    {
        [GetFrom(Singleton.Mouse)]
        protected Mouse mouse = null!;

        [GetFrom(Singleton.Keyboard)]
        protected Keyboard keyboard = null!;

        protected Vector2? mouseDragStart = null;

        static protected GUIElement Focus = null!;
        public bool IsFocus { get => Focus == this; }


        private bool wasFocus = false;
        
        protected void HandleMouseInteraction()
        {
            if (IsFocus)
            {
                mouseDragStart ??= mouse.Position;
                if (!wasFocus) OnClick();

                wasFocus = true;

                if (mouse.ReleasedThisFrame)
                {
                    OnRelease();

                    GUIElement.Focus = null!;

                    return;
                }


                if (mouseDragStart != null) OnDrag();

            }
            else
            {
                mouseDragStart = null;
                wasFocus = false;
            }

            if (mouse.ClickedThisFrame && !keyboard.HoldingKey(SDL2.SDL.SDL_Keycode.SDLK_LALT) && ((IContainer)this).InContainer(mouse.Position))
            {
                GUIElement.Focus = this;
            }
        }


        protected abstract void OnClick();

        protected abstract void OnDrag();

        protected abstract void OnRelease();

    }
}
