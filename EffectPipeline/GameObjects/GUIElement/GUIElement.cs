using EffectPipeline.GameObjects;
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

namespace EffectPipeline.GameObjects.GUIElements
{
    public abstract class GUIElement : GameObject
    {

        [DependencyCache(InteractionType.Download)]
        protected NodeCanvasCamera camera = null!;

        [DependencyCache(InteractionType.Download)]
        protected IContainer clippingContainer = null!;

        [GetFrom(Singleton.Mouse)]
        protected Mouse mouse = null!;

        [GetFrom(Singleton.Keyboard)]
        protected Keyboard keyboard = null!;

        protected Vector2? mouseDragStart = null;

        public static GUIElement Focus = null!;
        public bool IsFocus { get => Focus == this; }
        
        protected void HandleMouseInteraction()
        {
            if (IsFocus)
            {
                if(mouseDragStart == null)
                {
                    OnClick();
                }
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

            if (mouse.ClickedThisFrame && !keyboard.HoldingKey(SDL2.SDL.SDL_Keycode.SDLK_LALT) && MouseHovering)
            {
                GUIElement.Focus = this;
            }
        }

        public bool MouseHovering => ((IContainer)this).InContainer(mouse.Position) && clippingContainer.InContainer(mouse.Position);


        protected abstract void OnClick();

        protected abstract void OnDrag();

        protected abstract void OnRelease();


    }
}
