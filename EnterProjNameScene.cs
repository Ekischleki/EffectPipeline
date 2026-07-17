using EffectPipeline.Effects;
using EffectPipeline.GameObjects;
using EffectPipeline.GameObjects.GUIElements;
using EffectPipeline.GameObjects.PipelineManagers;
using EffectPipeline.persist;
using Pandemonium.Engine;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.GameSceneStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline
{
    public class EnterProjNameScene : GameScene
    {
        protected override IEnumerable<GameObject> GetStartingGameObjects()
        {
            var projectDirectory = Path.GetFullPath("./Project");
            if (!Directory.Exists(projectDirectory))
            {
                Directory.CreateDirectory(projectDirectory);
            }



            yield return new ProjNameEntry()
            {
                OnConfirm = (s) => {
                    var savePath = Path.Combine(projectDirectory, $"{s}.ep");
                    if (Path.Exists(savePath))
                    {
                        defaultLogger.Warn($"Couldn't create '{s}' because path already exists");
                        return;
                    }
                    var manager = new NodeStateManager();
                    var source = manager.CreateNode(new ImageSource());
                    source.position = new(-200, 0);
                    manager.UpdatePropertyState(source, 0, new DropdownPropertyState(2));
                    manager.CreateConnection(new(manager.OutputNodeState, 0), new(source, 0));
                    using FileStream f = new(savePath, FileMode.CreateNew);
                    Project.SaveTo(f, manager, s).Wait();
                    Game.SetScene(new MainScene(manager, s, savePath));
                }
            };
        }


        protected override bool OnExitRequest() => true;
    }
}
