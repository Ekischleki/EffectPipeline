using EffectPipeline.persist;
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
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.GameObjects
{
    internal class ProjectFileDisplay : GameObject
    {
        public ProjectFileDisplay(Project project)
        {
            thumbnail = project.thumbnail;
            title = project.Title;
        }

        public ProjectFileDisplay(string title, RGBImage? thumbnail)
        {
            this.title = title;
            this.thumbnail = thumbnail;
        }

        [GetFrom(Singleton.Mouse)]
        Mouse mouse = null!;

        [GetFrom(StoreType.FontStore, "std:oxanium.ttf@15")]
        RenderedFont font = null!;

        string title;
        RGBImage? thumbnail;

        public event Action? OnClick;
        public override void Init()
        {
            AddChildSpawnQueue([
                new TextureGameObject() {
                    origin = IPositioning.LeftCenter,
                    anchor = IPositioning.LeftCenter,
                    offset = new(20, 0),
                    Size = 0.18f
                }.WithTexture(thumbnail?.ToTexture(Game.Canvas)!),
                new TextGameObject() {
                    origin = IPositioning.RightCenter,
                    anchor = IPositioning.RightCenter,
                    offset = new(-10, 0),
                    Font = font,
                    Text = title
                }
                ]
            );
        }

        protected override void Update()
        {
            var parent_size = parent.ContainerSize;
            RenderTexture = (ManagedTexture)GetFrom(StoreType.PlaceholderTextureStore, $"generated/box/white/{parent_size.X - 10}/50");
            if (((IContainer)this).InContainer(mouse.Position) && mouse.ClickedThisFrame)
            {
                OnClick?.Invoke();
            }
        }
    }
}
