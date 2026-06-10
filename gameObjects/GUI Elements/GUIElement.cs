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

        [DependencyCache(InteractionType.Download)]
        protected NodeCanvasCamera camera = null!;

        [GetFrom(Singleton.Mouse)]
        protected Mouse mouse = null!;

        [GetFrom(Singleton.Keyboard)]
        protected Keyboard keyboard = null!;

        protected Vector2? mouseDragStart = null;

        static protected GUIElement Focus = null!;
        public bool IsFocus { get => Focus == this; }
        
        protected void HandleMouseInteraction()
        {
            if (IsFocus)
            {
                mouseDragStart ??= camera.Cam_mouse_pos;

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
            }

            if (mouse.ClickedThisFrame && !keyboard.HoldingKey(SDL2.SDL.SDL_Keycode.SDLK_LALT) && ((IContainer)this).InContainer(mouse.Position))
            {
                GUIElement.Focus = this;
                OnClick();
            }
        }


        protected abstract void OnClick();

        protected abstract void OnDrag();

        protected abstract void OnRelease();

    }
}
