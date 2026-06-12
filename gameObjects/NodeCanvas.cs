using EffectPipeline.Effects;
using EffectPipeline.gameObjects.GUI_Elements;
using EffectPipeline.types;
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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.gameObjects
{
    internal class NodeCanvas : GameObject
    {
        internal NodeCanvas() {
            clippingContainer = this;
        }
        [DependencyCache(InteractionType.Upload)]
        protected IContainer clippingContainer;
        [DependencyCache(InteractionType.Upload)]
        NodeStateManager manager = new();
        [DependencyCache(InteractionType.Upload)]
        NodeCanvasCamera camera = new();
        internal Vector2 size = new(500);
        public override void Init()
        {
            RenderTexture = (ManagedTexture)GetFrom(StoreType.PlaceholderTextureStore, $"generated/box/gray/{(int)size.X}/{(int)size.Y}");
            clipBehavior = ClipBehavior.Cut;
            camera.WithChildren([
                manager   , new DropdownProperty(["RGB", "HSV", "OkLab"], "Color space")
            {
                origin = IPositioning.TopLeft,
                anchor = IPositioning.Center,
            }
            ]);
            manager.CreateNode(new ImageSource(RGBImage.WhiteImage(256, 256)), "Image Source");
            manager.CreateNode(new ImageSource(RGBImage.WhiteImage(256, 256)), "Image Source");
            Node n = manager.CreateNode(new ImageOutput(), "Output");
            n.offset = new Vector2(250, 0);

            manager.CreateNode(new SplitChannel(), "Split RGB Channel");
            manager.CreateNode(new MergeChannel(), "Split RGB Channel");

            manager.CreateNode(new SplitChannel(), "Split RGB Channel");
            manager.CreateNode(new MergeChannel(), "Split RGB Channel");

            AddChildSpawnQueue(camera);
        }
        public override Vector2 ContainerSize { get => size; protected set {} }
        //public override Vector2 ContainerPosition { get => position; protected set { } }
        protected override void Update()
        {
            
        }
    }


    internal class NodeCanvasCamera : GameObject
    {
        [DependencyCache(InteractionType.Download)]
        protected IContainer clippingContainer = null!;
        [GetFrom(Singleton.Mouse)]
        Mouse mouse = null!;

        internal Vector2 Cam_mouse_pos { private set; get; }

        [GetFrom(Singleton.Keyboard)]
        Keyboard keyboard = null!;

        Vector2? drag_start = null;
        Vector2 cam_pos = new();

        internal bool IsDragging => drag_start != null;
        public override void Init()
        {
        }

        internal bool IsClickingDragButton => mouse.MouseEvent.HasFlag(Pupilmonium.Framework.MouseEvent.Middle) ||
                (mouse.MouseEvent.HasFlag(Pupilmonium.Framework.MouseEvent.Left) && keyboard.HoldingKey(SDL2.SDL.SDL_Keycode.SDLK_LALT));

        protected override void Update()
        {
            if(drag_start == null && IsClickingDragButton && !clippingContainer.InContainer(mouse.Position))
            {
                return;
            }
            if(IsClickingDragButton)
            {
                drag_start ??= mouse.Position;
                var new_offset = mouse.Position - drag_start.Value;
                offset = cam_pos + new_offset;
            } else
            {
                if (drag_start != null)
                {
                    var new_offset = mouse.Position - drag_start.Value;
                    if (new_offset.Length() < 5)
                    {
                        //This is probably not supposed to be a drag, but a click
                        defaultLogger.Log("Center mouse click event");
                    }
                    cam_pos += new_offset;
                    offset = cam_pos;
                }
                drag_start = null;
            }
            Cam_mouse_pos = mouse.Position - offset;
        }
    }
}
