using EffectPipeline.effects;
using EffectPipeline.Effects;
using Pandemonium.Engine;
using Pandemonium.Engine.GameSceneStuff;
using System.Collections.Immutable;

namespace EffectPipeline
{
    internal class Program : Game
    {
        public static IReadOnlyList<IEffectSearch> DefaultEffectSearch { get; private set; } = null!;

        static void Main(string[] args)
        {
            using (var game = new Program())
            {
                game.Start();
            }
        }
        protected override GameScene GetStartScene() => new MainScene();

        protected override bool WindowResizable => true;
        protected override bool BlurryScaling => true;
        protected override void Init()
        {
            DefaultEffectSearch = PluginLoad.LoadEffectSearches(defaultLogger).ToImmutableList();

            AddFontUpload("std", new FileDataUpload("./assets/fonts"));
            AddTextureUpload("std", new FileDataUpload("./assets/textures"));
        }
    }
}
