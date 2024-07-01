using System;
using System.Collections.Generic;
using System.Linq;
using BV2024WindModel.Abstractions;

namespace BV2024WindModel
{
    public class WindCalculationResultFactory
    {
        public static List<WindCalculationResult> Create(IEnumerable<LongitudinalSurfaceCalculationResult> surfaces)
        {

            return surfaces.Select(surface => new WindCalculationResult { Coordinate = surface.Coordinate, Area = GetSurfaceArea(surface) }).ToList();
        }

        private static double GetSurfaceArea(LongitudinalSurfaceCalculationResult surface)
        {
            var windArea = 0.0;
            foreach (var containerResult in surface.Result)
            {
                windArea += containerResult.Area; 
            }
            return windArea;
        }
    }

}
