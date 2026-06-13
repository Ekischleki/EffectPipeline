using Pandemonium.Engine;
using Pandemonium.Engine.GameSceneStuff;

namespace EffectPipeline
{
    internal class Program : Game
    {
        static void Main(string[] args)
        {
            using (var game = new Program())
            {
                game.Start();
            }
        }

        protected override GameScene GetStartScene() => new MainScene();

        protected override bool WindowResizable => true;

        protected override void Init()
        {
            AddFontUpload("std", new FileDataUpload("./assets/fonts"));
            AddTextureUpload("std", new FileDataUpload("./assets/textures"));
        }
    }
}
