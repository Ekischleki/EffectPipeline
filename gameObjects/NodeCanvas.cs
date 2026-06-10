using EffectPipeline.Effects;
using EffectPipeline.types;
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

namespace EffectPipeline.gameObjects
{
    internal class NodeCanvas : GameObject
    {
        public override void Init()
        {
            var p = new PipelineManager();

            var cam = new NodeCanvasCamera().WithChildren([
                p
            ]);
            AddChildSpawnQueue(cam);


            Node src = p.InstantiateNewNode(new ImageSource(RGBImage.WhiteImage(200, 200)), "Image Source");
            Node outp = p.InstantiateNewNode(new ImageOutput(), "Image Output");
            outp.offset = new Vector2(250, 0);
        }

        protected override void Update()
        {
            
        }
    }


    internal class NodeCanvasCamera : GameObject
    {
        [GetFrom(Singleton.Mouse)]
        Mouse mouse = null!;

        [GetFrom(Singleton.Keyboard)]
        Keyboard keyboard = null!;

        Vector2? drag_start = null;
        Vector2 cam_pos = new();
        public override void Init()
        {
        }

        protected override void Update()
        {
            if(mouse.MouseEvent.HasFlag(Pupilmonium.Framework.MouseEvent.Middle) || 
                (mouse.MouseEvent.HasFlag(Pupilmonium.Framework.MouseEvent.Left) && keyboard.HoldingKey(SDL2.SDL.SDL_Keycode.SDLK_LALT))
                )
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
        }
    }
}
