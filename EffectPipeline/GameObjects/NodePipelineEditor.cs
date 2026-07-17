using EffectPipeline.GameObjects.PipelineManagers;
using EffectPipeline.persist;
using Pandemonium.Engine;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using Pandemonium.Engine.UIOI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.GameObjects
{
    internal class NodePipelineEditor(NodeStateManager manager, string title, string path) : GameObject
    {
        [GetFrom(Singleton.ScreenContainer)]
        IContainer screenContainer = null!;
        internal Vector2 size = new(800);
        internal int canvas_width = 500;


        NodeStateManager manager = manager;

        [GetFrom(Singleton.Keyboard)]
        Keyboard keyboard = null!;

        string title = title;
        string path = path;
        public override Vector2 ContainerSize { get => size; protected set { } }
        [DependencyCache(InteractionType.Upload)]
        internal NodeCanvas node_canvas = new(manager) { origin = IPositioning.TopLeft, anchor = IPositioning.TopLeft };
        internal ImageDisplayContainer display = new() { origin = IPositioning.TopRight, anchor = IPositioning.TopRight };
        public override void Init()
        {
            AddChildSpawnQueue([node_canvas, display]);
        }

        protected override void Update()
        {
            if(keyboard.ClickingKey(SDL2.SDL.SDL_Keycode.SDLK_ESCAPE))
            {
                ChangeScene(new MainMenu());
            }
            if (keyboard.HoldingKey(SDL2.SDL.SDL_Keycode.SDLK_LCTRL))
            {
                if(keyboard.ClickingKey((SDL2.SDL.SDL_Keycode)'s')) {
                    using FileStream f = new(path, FileMode.Create);
                    Project.SaveTo(f, manager, title).Wait();
                    defaultLogger.Log("Saved project!");
                }
                if (keyboard.ClickingKey((SDL2.SDL.SDL_Keycode)'e'))
                { 
                    var dateTime = DateTime.Now;
                    var location = $"./exported/{dateTime.Year:0000}{dateTime.Month:00}{dateTime.Day:00}{dateTime.Hour:00}{dateTime.Minute:00}{dateTime.Second:00}{dateTime.Millisecond:000}{dateTime.Nanosecond:000}.png";
                    if (!TrySaveOutput(location).Result)
                    {
                        defaultLogger.Error("Couldn't export output image");
                    }
                }
            }
            size = screenContainer.AbsoluteContainerSize;
            canvas_width = (int)(size.X * .5);
            node_canvas.Size = new(canvas_width, size.Y);
            display.size = new(size.X - canvas_width, size.Y);
        }

        private async Task<bool> TrySaveOutput(string location)
        {
            if (!Directory.Exists(location))
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(location)));
                }
                catch (Exception ex)
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


            await outputImage.SaveTo(file);

            file.Dispose();

            return true;
        }
    }
}
