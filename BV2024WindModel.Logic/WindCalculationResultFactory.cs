using System.Collections.Generic;
using System.Linq;
using BV2024WindModel.Abstractions;

namespace BV2024WindModel
{
    public class WindCalculationResultFactory
    {
        public static List<WindCalculationResult> Create(IEnumerable<Surface> surfaces)
        {

            return surfaces.Select(surface => new WindCalculationResult { Coordinate = surface.Coordinate, Area = surface.Area }).ToList();
        }
    }

}
