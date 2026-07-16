using EffectPipeline.Effects;
using EffectPipeline.gameObjects.GUI_Elements;
using EffectPipeline.persist;
using EffectPipeline.types;
using Pandemonium.Engine;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using Pandemonium.Engine.UIOI;
using Pupilmonium.Framework;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EffectPipeline.gameObjects
{
    public class NodeCanvas : GameObject
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
        NodeSearch node_search;
        
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
            AddChildSpawnQueue([camera, node_search]);
        }

        public void CreateNode(string title, IEffect effect, IEffectSearch origin)
        {
            HideSearch();
            var node = manager.CreateNode(effect, title, origin);
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
            if (keyboard.HoldingKey(SDL2.SDL.SDL_Keycode.SDLK_LCTRL) && keyboard.ClickingKey((SDL2.SDL.SDL_Keycode)'s'))
            {
                var dateTime = DateTime.Now;
                var location = $"./exported/{dateTime.Year:0000}{dateTime.Month:00}{dateTime.Day:00}{dateTime.Hour:00}{dateTime.Minute:00}{dateTime.Second:00}{dateTime.Millisecond:000}{dateTime.Nanosecond:000}.png";
                if (!TrySaveOutput(location))
                {
                    defaultLogger.Error("Couldn't save output image");
                }
                var file_location = $"./exported/{dateTime.Year:0000}{dateTime.Month:00}{dateTime.Day:00}{dateTime.Hour:00}{dateTime.Minute:00}{dateTime.Second:00}{dateTime.Millisecond:000}{dateTime.Nanosecond:000}.ep";
                var stream = new FileStream(file_location, FileMode.CreateNew);
                Task.Run(() => Project.SaveTo(stream, this.manager));
            }
            if (keyboard.HoldingKey(SDL2.SDL.SDL_Keycode.SDLK_LCTRL) && keyboard.ClickingKey((SDL2.SDL.SDL_Keycode)'l'))
            {
                var stream = new FileStream("C:\\Users\\ewolf\\repos\\EffectPipeline\\bin\\Debug\\net8.0\\exported\\20260715012459076900.ep", FileMode.Open);
                Task.Run(() => Project.LoadFrom(stream, this.manager));

            }
            if (keyboard.ReleasedKey((SDL2.SDL.SDL_Keycode)'a'))
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
        private bool TrySaveOutput(string location)
        {
            if (!Directory.Exists(location)) {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(location)));
                } catch (Exception ex)
                {
                    defaultLogger.Warn($"Couldn't create path: {ex}");
                    return false;
                }
            }
            var outputImage = manager.OutputImage;
            if (outputImage == null)
            {
                defaultLogger.Warn($"Output image doesn't exist right now");
                return false;
            }

            var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgb24>(outputImage.width, outputImage.height);
            for (int x = 0; x < outputImage.width; x++)
            {
                for (int y = 0; y < outputImage.height; y++)
                {
                    var i = y * outputImage.width + x;
                    byte r = (byte)float.Clamp(outputImage.red[i] * 255, 0f, 255f);
                    byte g = (byte)float.Clamp(outputImage.green[i] * 255, 0f, 255f);
                    byte b = (byte)float.Clamp(outputImage.blue[i] * 255, 0f, 255f);


                    image[x, y] = new(r, g, b);
                }
            }

            FileStream file;

            try
            {
                file = new FileStream(location, FileMode.CreateNew);
            }
            catch (Exception ex)
            {
                defaultLogger.Warn($"Couldn't create file: {ex}");
                return false;
            }


            image.Save(file, new PngEncoder());

            return true;
        }
    }


    public class NodeCanvasCamera : GameObject
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

                        if(canvas != null)
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
