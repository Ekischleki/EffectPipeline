using Pandemonium.Engine;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using Pupilmonium.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.gameObjects.GUI_Elements
{
    internal class DropdownProperty : GUIElement
    {
        [DependencyCache(InteractionType.Download)]
        internal Node parentNode = null!;
        [DependencyCache(InteractionType.Download)]
        internal NodeStateManager manager = null!;

        internal enum Colorspace
        {
            Rgb,
            Hsv,
            Lab,
        }
        public static DropdownProperty ColorspaceDropdown => new DropdownProperty(["RGB", "HSV", "OkLab"], "Color space");


        public bool open = false;
        public int Selected {  get; set; }
        private string[] properties;
        private string title;
        private TextGameObject[] elements = null!;
        [GetFrom(StoreType.FontStore, "std:oxanium.ttf@10")]
        internal RenderedFont font = null!;
        private TextGameObject open_text = null!;
        private TextGameObject closed_text = null!;
        private ManagedTexture open_texture = null!;
        private ManagedTexture closed_texture = null!;

        public DropdownProperty(string[] properties, string title)
        {
            this.properties = properties;
            this.title = title;
        }

        private void SetOpenClosedText()
        {
            open_text.Text = $"v {properties[Selected]}";
            closed_text.Text = $"> {properties[Selected]}";
        }

        public override void Init()
        {
            open_text = new()
            {
                Font = font,
                origin = IPositioning.TopLeft,
                anchor = IPositioning.TopLeft,
                offset = new(5),
                Show = false
            };
            closed_text = new()
            {
                Font = font,
                origin = IPositioning.TopLeft,
                anchor = IPositioning.TopLeft,
                offset = new(5),
                Show = true
            };
            SetOpenClosedText();
            AddChildSpawnQueue([open_text, closed_text]);
            elements = properties.Select(prop => new TextGameObject()
            {
                Font = font,
                Text = prop,
                Color = Color.Black,
                anchor = IPositioning.TopLeft,
                origin = IPositioning.TopLeft,
                Show = false
            }).ToArray();
            AddChildSpawnQueue(elements);
            int y = open_text.ContentDimensions.h + 10;
            int max_width = open_text.ContentDimensions.w + 10;
            foreach (var element in elements)
            {
                var size = element.ContentDimensions;
                max_width = int.Max(size.w + 10, max_width);
                element.offset = new(5, y);
                y += size.h + 5;
            }
            open_texture = (ManagedTexture)GetFrom(StoreType.PlaceholderTextureStore, $"generated/box/gray/{max_width + 10}/{y}");
            closed_texture = (ManagedTexture)GetFrom(StoreType.PlaceholderTextureStore, $"generated/box/gray/{max_width + 10}/{closed_text.ContentDimensions.h + 10}");

            this.open = true;
            Open(false);
        }

        protected override void Update()
        {
            HandleMouseInteraction();
            if(!((IContainer)this).InContainer(mouse.Position))
            {
                Open(false);
            }
        }

        private void Open(bool open)
        {
            if (this.open == open)
            {
                return;
            }
            this.open = open;
            if (open)
            {
                RenderTexture = open_texture;
                foreach (var element in elements)
                {
                    element.Show = true;
                }
                open_text.Show = true;
                closed_text.Show = false;
            }
            else
            {
                RenderTexture = closed_texture;
                foreach (var element in elements)
                {
                    element.Show = false;
                }
                open_text.Show = false;
                closed_text.Show = true;
            }
        }

        protected override void OnClick()
        {
            if(open)
            {
                if(((IContainer)closed_text).InContainer(mouse.Position))
                {
                    Open(false);
                    return;
                }
                int i = 0;
                foreach(var element in elements)
                {
                    if (((IContainer)element).InContainer(mouse.Position))
                    {
                        Open(false);
                        Selected = i;
                        manager.UpdateCacheAt(parentNode);
                        SetOpenClosedText();
                        return;
                    }
                    i++;
                }
            } else
            {
                Open(true);   

            }
        }

        protected override void OnDrag()
        {
        }

        protected override void OnRelease()
        {
            
        }
    }
}
