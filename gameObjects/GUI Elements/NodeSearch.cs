using Pandemonium.Engine;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.Positioning;
using Pandemonium.Engine.SetupAttributes;
using Pandemonium.Engine.UIOI;
using Pupilmonium.Framework;
using SDL2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.GameObjects.GUIElements
{
    internal class NodeSearch : GameObject
    {
        [DependencyCache(InteractionType.Upload)]
        NodeSearch node_search;
        [DependencyCache(InteractionType.Upload)]
        NodeSearch clippingContainer;
        [DependencyCache(InteractionType.Upload)]
        internal required Action<IEffect> OnSelect;
        internal SearchTextElement search_text_elem = null!;
        List<SearchElement> searchElements = [];
        internal NodeSearch()
        {
            node_search = this;
            clippingContainer = this;
        }
        public required int width { get; init; }
        public required int height { get; init; }
        public required SearchIndex searchIndex { get; init; }
        public override void Init()
        {
            clipBehavior = ClipBehavior.Cut;
            showBehavior = ShowBehavior.Inherit;
            RenderTexture = (ManagedTexture)GetFrom(StoreType.PlaceholderTextureStore, $"generated/box/white/{width}/{height}");
            AddChildSpawnQueue(search_text_elem = new SearchTextElement() { width = width - 10, anchor = IPositioning.TopCenter, origin = IPositioning.TopCenter, offset = new(0, 5) });
            search_text_elem.OnTextUpdate += ((_, text) =>
            {
                defaultLogger.Warn("-------");
                foreach(var elem in searchElements)
                {
                    AddDespawnQueue(elem);
                }
                searchElements.Clear();
                int y = 30;
                foreach (var effect in searchIndex.Search(text))
                {
                    SearchElement elem;
                    searchElements.Add(elem = new() { Title = effect.Title, OnClicked = () => {
                        defaultLogger.Info($"Clicked {effect.Title}: {effect}");
                        OnSelect(effect);
                    },
                    anchor = IPositioning.TopCenter,
                    origin = IPositioning.TopCenter,
                    });
                    AddChildSpawnQueue(elem, _ => {
                        elem.offset = new(0, y);
                        y += (int)elem.ContainerSize.Y + 5;
                    });
                }
            });
        }

        protected override void Update()
        {
            
        }
    }

    internal class SearchElement : GUIElement
    {
        [DependencyCache(InteractionType.Download)]
        NodeSearch node_search;
        [GetFrom(StoreType.FontStore, "std:oxanium.ttf@15")]
        RenderedFont font = null!;

        ManagedTexture focus_texture = null!;
        ManagedTexture no_focus_texture = null!;
        internal required string Title { get; init; }

        internal Action? OnClicked { get; init; }

        TextGameObject text = null!;
        public override void Init()
        {

            AddChildSpawnQueue(text = new() { Font = font, Text = Title, Color = Color.White, anchor = IPositioning.LeftCenter, origin = IPositioning.LeftCenter, offset = new(5, 0)});
            no_focus_texture = (ManagedTexture)GetFrom(StoreType.PlaceholderTextureStore, $"generated/box/DarkGray/{parent.ContainerSize.X - 10}/{text.ContentDimensions.h + 5}");
            focus_texture = (ManagedTexture)GetFrom(StoreType.PlaceholderTextureStore, $"generated/box/DarkRed/{parent.ContainerSize.X - 10}/{text.ContentDimensions.h + 5}");
            RenderTexture = no_focus_texture;
        }
        protected override void OnDrag() { }
        protected override void OnRelease() { }

        protected override void OnClick()
        {
            OnClicked?.Invoke();
        }


        protected override void Update()
        {
            HandleMouseInteraction();
            RenderTexture = MouseHovering ? focus_texture : no_focus_texture;
        }
    }

    internal class SearchTextElement : GameObject
    {
        static char[] listenKeys = "abcdefghijklmnopqrstuvwxyz ".ToCharArray();
        [GetFrom(StoreType.FontStore, "std:oxanium.ttf@15")]
        RenderedFont font = null!;
        [GetFrom(Singleton.Keyboard)]
        Keyboard keyboard = null!;
        internal TextGameObject text = null!;
        public required int width { get; init; }
        public event EventHandler<string>? OnTextUpdate;
        public override void Init()
        {
            text = new() { Font = font, Color = Color.White, origin = IPositioning.LeftCenter, anchor = IPositioning.LeftCenter, offset = new(5, 0) };
            RenderTexture = (ManagedTexture)GetFrom(StoreType.PlaceholderTextureStore, $"generated/box/gray/{width}/20");
            clipBehavior = ClipBehavior.Cut;
            AddChildSpawnQueue(text);
        }

        protected override void Update()
        {
            bool changed = false;
            foreach (var key in listenKeys)
            {
                //Magic number idc
                if (keyboard.ClickingKey((SDL.SDL_Keycode)key) && text.Text.Length < 100){
                    text.Text += key;
                    changed = true;
                }
            }
            if(keyboard.ClickingKey(SDL.SDL_Keycode.SDLK_BACKSPACE) && text.Text.Length > 0)
            {
                text.Text = text.Text[..(text.Text.Length - 1)];
                changed = true;
            }
            if(changed)
            {
                if (text.ContentDimensions.w > ContainerSize.X - 5)
                {
                    text.origin = IPositioning.RightCenter;
                    text.anchor = IPositioning.RightCenter;
                    text.offset = new(-5, 0);
                }
                else
                {
                    text.origin = IPositioning.LeftCenter;
                    text.anchor = IPositioning.LeftCenter;
                    text.offset = new(5, 0);
                }
                OnTextUpdate?.Invoke(this, text.Text);
            }
        }
    }
}
