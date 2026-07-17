using EffectPipeline.Effects;
using Pandemonium.Engine;
using Pandemonium.Engine.GameSceneStuff;
using System.Collections.Immutable;

namespace EffectPipeline
{
    internal class Program : Game
    {
        public static IReadOnlyList<IEffectSearch> DefaultEffectSearch { get; private set; } = null!;
        public static IReadOnlyDictionary<Type, IEffect> AllEffects { get; private set; } = null!;

        static void Main(string[] args)
        {
            using (var game = new Program())
            {
                game.Start();
            }
        }
        protected override GameScene GetStartScene() => new MainMenu();

        protected override bool WindowResizable => true;
        protected override bool BlurryScaling => true;
        protected override void Init()
        {
            DefaultEffectSearch = PluginLoad.LoadEffectSearches(defaultLogger).ToImmutableList();

            Dictionary<Type, IEffect> allEffects = [];
            allEffects.Add(typeof(ImageOutput), new ImageOutput());
            foreach(var effectStearch in DefaultEffectSearch)
            {
                var effect = effectStearch.CreateEffect();
                if(!allEffects.TryAdd(effect.GetType(), effect))
                {
                    throw new Exception($"Effect {effect.GetType().Name} has multiple effect search instances which return the same type.");
                }
            }
            AllEffects = allEffects.AsReadOnly();
            AddFontUpload("std", new FileDataUpload("./assets/fonts"));
            AddTextureUpload("std", new FileDataUpload("./assets/textures"));
        }
    }
}
