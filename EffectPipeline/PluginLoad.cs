using Pandemonium.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline
{
    internal class PluginLoad
    {
        public static IEnumerable<IEffectSearch> LoadEffectSearches(ILog log)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string pluginDir = @".\plugins";

                string dllName = new AssemblyName(args.Name).Name + ".dll";
                string path = Path.GetFullPath( Path.Combine(pluginDir, dllName));

                if (File.Exists(path))
                    return Assembly.LoadFrom(path);

                return null;
            };
            var plugin_path = Path.GetFullPath("./plugins");
            if (!Directory.Exists(plugin_path))
            {
                Directory.CreateDirectory(plugin_path);
            }
            
            foreach(var plugin in Directory.EnumerateFiles(plugin_path))
            {

                foreach(var effect_search in GetEffects(plugin, log))
                {
                    yield return effect_search;
                }
            }
            foreach (var effect_search in GetEffects(typeof(Program).Assembly, log))
            {
                yield return effect_search;
            }
        }

        public static IEnumerable<IEffectSearch> GetEffects(Assembly assembly, ILog log)
        {
            foreach (System.Type type in assembly.GetTypes().Where(type => type.IsAssignableTo(typeof(IEffectSearch)) && !(type.IsAbstract || type.IsInterface)))
            {
                IEffectSearch? instance = null;
                try
                {
                    instance = (IEffectSearch?)Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    log.Warn($"Couldn't create instance of `{type.AssemblyQualifiedName ?? type.FullName ?? type.Name}`: {ex}");
                }
                if (instance != null)
                    yield return instance;
            }
        }

        public static IEnumerable<IEffectSearch> GetEffects(string file, ILog log)
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFile(file);
               

            } catch(Exception ex)
            {
                log.Error($"Couldn't load assembly: {ex}");
                return [];
            }

           return GetEffects(assembly, log);
        }
    }

    public class PluginLoadContext : AssemblyLoadContext
    {
        private readonly string _pluginPath;

        public PluginLoadContext(string pluginPath)
            : base(isCollectible: true)
        {
            _pluginPath = pluginPath;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string dependencyPath = Path.Combine(_pluginPath, assemblyName.Name + ".dll");

            if (File.Exists(dependencyPath))
            {
                return LoadFromAssemblyPath(dependencyPath);
            }

            return null;
        }
    }
}
