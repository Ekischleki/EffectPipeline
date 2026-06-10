using EffectPipeline.gameObjects;
using Pandemonium.Engine;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using Pupilmonium.Framework;
using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.GameObjects
{
    internal class Parameter : GUIElement
    {
        internal required bool is_input;
        internal required string name;

        [GetFrom(StoreType.PlaceholderTextureStore, "generated/box/red/10/10")]
        internal ManagedTexture texture = null!;

        [GetFrom(StoreType.FontStore, "std:oxanium.ttf@10")]
        internal RenderedFont font = null!;


        public override void Init()
        {
            var text = new TextGameObject() { Font = font, Text = name };
            RenderTexture = texture;
            if (is_input)
            {
                text.anchor = IPositioning.RightCenter;
                text.origin = IPositioning.LeftCenter;
                origin = IPositioning.RightCenter;
                anchor = IPositioning.BottomLeft;
            } else
            {
                text.anchor = IPositioning.LeftCenter;
                text.origin = IPositioning.RightCenter;
                origin = IPositioning.LeftCenter;
                anchor = IPositioning.BottomRight;
            }
            AddChildSpawnQueue(text);
        }

        protected override void Update()
        {
            HandleMouseInteraction();
            ((MainScene)ParentScene).ConnectionManager.RegisterDist(this);
        }


        protected override void OnClick()
        {
            Size *= 1.02;
        }

        protected override void OnRelease()
        {
            Size /= 1.02;
        }


        protected override void Render()
        {
            base.Render();
        }

        protected override void OnDrag()
        {
        }

    }
}
