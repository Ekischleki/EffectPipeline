using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline
{
    internal class SearchIndex
    {
        IEnumerable<IEffectSearch> effects;
        internal SearchIndex(IEnumerable<IEffectSearch> effects) { this.effects = effects; }

        public IEnumerable<IEffect> Search(string query)
        {
            query = query.Trim();
            var elements = query.Split(' ');
            var searched_effects = effects.Select(effectSearch =>
                {
                    var effect = effectSearch.CreateEffect();
                    int score = 0;
                    foreach (var element in elements)
                    {
                        if (effectSearch.Tags.Any(tag => tag.Contains(element, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            if (effectSearch.Tags.Any(tag => tag.Equals(element, StringComparison.InvariantCultureIgnoreCase)))
                                score += 2;
                            else
                                score += 1;
                        }
                        else
                        {
                            score--;
                        }
                    }
                    if (elements.Any(element => effect.Title.Contains(element, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if(elements.Any(element => element.Equals(effect.Title, StringComparison.InvariantCultureIgnoreCase)) || query.Trim().Equals(effect.Title, StringComparison.InvariantCultureIgnoreCase))
                        {
                            score += 10;
                        } else
                        {
                            score += 4;
                        }
                    }
                    return (score, effect);
                }).ToList();
            searched_effects.Sort((a, b) => b.score.CompareTo(a.score));
            return searched_effects.Select(x => x.effect);
        }
    }

    public interface IEffectSearch
    {
        public IEnumerable<string> Tags { get; }
        public IEffect CreateEffect();
    }
}
