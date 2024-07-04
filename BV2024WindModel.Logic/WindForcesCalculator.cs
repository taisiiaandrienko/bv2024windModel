#define PARALLEL

using System.Collections.Generic;
using BV2024WindModel.Abstractions;


namespace BV2024WindModel.Logic
{
    public class WindForcesCalculator
    {
        public static void Calculate(WindForceCalculator forcesCalculator, WindForcesExternalCalculationParameters externalParametrs, List<SurfaceCalculationResult> surfaces)
        {
            foreach (var surface in surfaces)
            {
                foreach (var containerResult in surface.Result)
                {
                    var fullWindForcesCalculationParametrs = new WindForcesCalculationParameters(externalParametrs, containerResult.FullArea, containerResult.VolumetricCenter);
                    var exposedForcesCalculationParametrs = new WindForcesCalculationParameters(externalParametrs, containerResult.ExposedArea, containerResult.VolumetricCenter);
                    containerResult.WindForceFull = forcesCalculator.Calculate(fullWindForcesCalculationParametrs);
                    containerResult.WindForceForArea = forcesCalculator.Calculate(exposedForcesCalculationParametrs);
                }
            }
        }
    }
}

