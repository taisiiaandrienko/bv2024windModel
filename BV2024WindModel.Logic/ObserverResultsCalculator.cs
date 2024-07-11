using System.Collections.Generic;
using System.Linq;
using BV2024WindModel.Abstractions;

namespace BV2024WindModel.Logic
{
    public class ObserverResultsCalculator : ICalculator< List<ObserverResult>, List<ShortObserverResult>>
    {
        public List<ShortObserverResult> Calculate(in List<ObserverResult> input)
        {
            var results = new List<ShortObserverResult>();
            foreach (var item in input)
            { 
                var result = new ShortObserverResult();
                result.Containers = item.Containers.Select(container => container.ID).OrderBy(id => id).ToList();
                result.Criteria = item.Criteria.OrderByDescending(criterium => criterium.Coordinate).ToList();
                results.Add(result);
            }
            return results;
        }
    }
}
    



         