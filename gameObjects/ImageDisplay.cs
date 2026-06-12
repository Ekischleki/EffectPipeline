using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.gameObjects
{
    internal class ImageDisplay : GameObject
    {
        [DependencyCache(InteractionType.Download)]
        internal NodeCanvas node_canvas = null!;
        public override void Init()
        {
            
        }

        protected override void Update()
        {
            RenderTexture = node_canvas.manager.OutputImage;
        }
    }
}
