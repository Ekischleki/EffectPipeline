using Pandemonium.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline
{
    internal class PluginLoad
    {
        public static IEnumerable<IEffectSearch> LoadEffectSearches(ILog log)
        {
            if (!Directory.Exists("./plugins"))
            {
                Directory.CreateDirectory("./plugins");
            }
                foreach(var plugin in Directory.EnumerateFiles("./plugins"))
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
}
