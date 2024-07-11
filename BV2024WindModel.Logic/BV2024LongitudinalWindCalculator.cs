using System.Collections.Generic;
using System.Linq;
using BV2024WindModel.Abstractions;


namespace BV2024WindModel.Logic
{
    public class BV2024LongitudinalWindCalculator : AbstractBV2024WindCalculator, ICalculator<WindCalculatorInput, LongitudinalSurfacesCalculationResult>
    {
        public LongitudinalSurfacesCalculationResult Calculate(in WindCalculatorInput input)
        { 
            var containers = input.Containers.ToList(); 
            var aftSurfaces = containers.GroupBy(container => container.AftSurface.Coordinate, container => container,
                (key, g) => new ContainersAtCoordinate(key, g.ToList(), ContainerEnd.Aft)).ToList();
            var foreSurfaces = containers.GroupBy(container => container.ForeSurface.Coordinate, container => container,
                (key, g) => new ContainersAtCoordinate(key, g.ToList(), ContainerEnd.Fore)).ToList();

            var aftProtectingSurfaces = ProtectingSurfacesFactory.Create(containers, foreSurfaces, aftSurfaces, ContainerEnd.Aft, true); 
            var foreProtectingSurfaces = ProtectingSurfacesFactory.Create(containers, foreSurfaces, aftSurfaces, ContainerEnd.Fore, true);


            foreach (var building in input.Vessel.Buildings)
            {
                aftProtectingSurfaces.Add(building.AftSurface);
                foreProtectingSurfaces.Add(building.ForeSurface);
            }

            double alpha = input.Vessel.Alpha;

            var windExposedForeSurfaces = GetWindExposedSurfaces(alpha, foreSurfaces, aftProtectingSurfaces, true).OrderByDescending(surface => surface.Coordinate).ToList();
            var windExposedAftSurfaces = GetWindExposedSurfaces(alpha, aftSurfaces, foreProtectingSurfaces, true).OrderByDescending(surface => surface.Coordinate).ToList();
             
            return new LongitudinalSurfacesCalculationResult(){ Aft = windExposedAftSurfaces, Fore = windExposedForeSurfaces };

        }
         
    }


}

