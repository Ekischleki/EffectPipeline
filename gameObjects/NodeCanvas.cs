using EffectPipeline.effects;
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
            canvas = this;
            node_search = new() { height = 400, width = 300, searchIndex = new(Program.DefaultEffectSearch), Show = false, Pause = true, OnSelect = CreateNode, pauseBehavior = PauseBehavior.Inherit, showBehavior = ShowBehavior.Inherit };
        }
        [GetFrom(Singleton.Keyboard)]
        Keyboard keyboard = null!;
        [GetFrom(Singleton.Mouse)]
        Mouse mouse = null!;
        [DependencyCache(InteractionType.Upload)]
        protected IContainer clippingContainer;
        [DependencyCache(InteractionType.Upload)]
        protected NodeCanvas canvas;
        [DependencyCache(InteractionType.Upload)]
        protected NodeSearch node_search;
        
        [DependencyCache(InteractionType.Upload)]
        internal NodeStateManager manager = new();
        [DependencyCache(InteractionType.Upload)]
        NodeCanvasCamera camera = new() { pauseBehavior = PauseBehavior.Inherit, showBehavior = ShowBehavior.Inherit };
        private Vector2 size = new(500);
        new internal Vector2 Size { 
            get => size; 
            set
            {
                size = value;
                InitSized();
            }
        }

        private void InitSized()
        {
            RenderTexture = (ManagedTexture)GetFrom(StoreType.PlaceholderTextureStore, $"generated/box/gray/{(int)Size.X}/{(int)Size.Y}");
        }

        public override Vector2 ContainerSize { get => Size; protected set {} }
        public override void Init()
        {
            InitSized();
            clipBehavior = ClipBehavior.Cut;
            camera.WithChildren([
                manager,
            ]);
            manager.CreateNode(new ImageSource(RGBImage.LoadFrom(@".\assets\textures\aquarellebg.png")), "Image Source Spacey");
            manager.CreateNode(new ImageSource(RGBImage.LoadFrom(@".\assets\textures\SpOoKy.png")), "Image Source SpOoKy");
            manager.CreateNode(new ImageSource(RGBImage.LoadFrom(@".\assets\textures\adhd mix.png")), "Image Source ADHD MIX");

            manager.CreateNode(new ImageSource(RGBImage.LoadFrom(@".\assets\textures\tree.png")), "Image Source Tree");

            AddChildSpawnQueue([camera, node_search]);
        }

        public void CreateNode(string title, IEffect effect)
        {
            HideSearch();
            var node = manager.CreateNode(effect, title);
            GUIElement.Focus = node;
            node.offset = camera.Cam_mouse_pos - new Vector2(30, 5);
        }
        internal void ShowSearch()
        {
            node_search.offset = mouse.Position - new Vector2(40, 40);
            node_search.Pause = false;
            node_search.Show = true;
            camera.Pause = true;
        }
        internal void HideSearch()
        {
            node_search.Pause = true;
            node_search.Show = false;
            camera.Pause = false;
        }
        //public override Vector2 ContainerPosition { get => position; protected set { } }
        protected override void Update()
        {
            if(keyboard.ReleasedKey((SDL2.SDL.SDL_Keycode)'a'))
            {
                ShowSearch();
            } else if (keyboard.ClickingKey(SDL2.SDL.SDL_Keycode.SDLK_ESCAPE) || !((IContainer)node_search).InContainer(mouse.Position))
            {
                HideSearch();
            }
            if(node_search.Pause)
            {
                node_search.search_text_elem.text.Text = "";
            }
        }
    }


    internal class NodeCanvasCamera : GameObject
    {
        [DependencyCache(InteractionType.Download)]
        protected IContainer clippingContainer = null!;
        [GetFrom(Singleton.Mouse)]
        Mouse mouse = null!;
        [DependencyCache(InteractionType.Download)]
        protected NodeCanvas canvas = null!;

        internal Vector2 Cam_mouse_pos => (mouse.Position - offset) / AbsoluteSize;

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
            if (clippingContainer.InContainer(mouse.Position))
            {
                Size *= float.Pow(1.1f, mouse.MouseWheel);
            }
            if(drag_start == null && IsClickingDragButton && !clippingContainer.InContainer(mouse.Position))
            {
                return;
            }
            if(IsClickingDragButton)
            {
                drag_start ??= mouse.Position;
                var new_offset = (mouse.Position - drag_start.Value) / AbsoluteSize;
                offset = cam_pos + new_offset;
            } else
            {
                if (drag_start != null)
                {
                    var new_offset = (mouse.Position - drag_start.Value) / AbsoluteSize;
                    if (new_offset.Length() < 5)
                    {
                        //This is probably not supposed to be a drag, but a click
                        defaultLogger.Log("Center mouse click event");
                        canvas.ShowSearch();
                    }
                    cam_pos += new_offset;
                    offset = cam_pos;
                }
                drag_start = null;
            }
        }
    }
}
