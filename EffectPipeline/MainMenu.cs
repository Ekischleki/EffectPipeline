using EffectPipeline.GameObjects;
using EffectPipeline.persist;
using EffectPipeline.types;
using Pandemonium.Engine.GameObjectStuff;
using Pandemonium.Engine.GameSceneStuff;
using Pandemonium.Engine.Positioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline
{
    internal class MainMenu : GameScene
    {

        private IEnumerable<(string, Project)> EnumerateProjects()
        {
            var path = Path.GetFullPath("./Project");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Directory.EnumerateFiles(path).Select(projectPath => {
                Project project = null!;
                try
                {
                    using FileStream f = new(projectPath, FileMode.Open);
                    project = Project.LoadFrom(f).Result;
                } catch (Exception ex) {
                    defaultLogger.Error($"Couldn't load project at {projectPath}: {ex}");
                }
                return (projectPath, project);
            }).Where(proj => proj.project is not null);
        }
        protected override IEnumerable<GameObject> GetStartingGameObjects()
        {
            var add = new ProjectFileDisplay("New Project", RGBImage.LoadFrom("./assets/textures/add.png"))
            {
                origin = IPositioning.TopCenter,
                anchor = IPositioning.TopCenter,
                offset = new(0, 10)
            };
            add.OnClick += () => Game.SetScene(new EnterProjNameScene());
            yield return add;
            float y = 70;
            foreach(var (path, proj) in EnumerateProjects())
            {
                var display = new ProjectFileDisplay(proj) {
                    origin = IPositioning.TopCenter,
                    anchor = IPositioning.TopCenter,
                    offset = new(0, y)

                };
                display.OnClick += () => Game.SetScene(new MainScene(proj.nodeStateManager, proj.Title, path));
                yield return display;
                y += 60;
            }
        }

        protected override bool OnExitRequest() => true;
    }
}
