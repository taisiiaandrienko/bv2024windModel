using System.Collections.Generic;
using System.Linq;
using BV2024WindModel.Abstractions;


namespace BV2024WindModel.Logic
{
    public class BV2024LongitudinalWindCalculator : AbstractBV2024WindCalculator, ICalculator<IEnumerable<Container>, LongitudinalSurfacesCalculationResult>
    {
        public LongitudinalSurfacesCalculationResult Calculate(in IEnumerable<Container> input)
        { 
            var containers = input.ToList(); 
            var aftSurfaces = containers.GroupBy(container => container.AftSurface.Coordinate, container => container,
                (key, g) => new ContainersAtCoordinate(key, g.ToList(), ContainerEnd.Aft)).ToList();
            var foreSurfaces = containers.GroupBy(container => container.ForeSurface.Coordinate, container => container,
                (key, g) => new ContainersAtCoordinate(key, g.ToList(), ContainerEnd.Fore)).ToList();

            var aftProtectingSurfaces = ProtectingSurfacesFactory.Create(containers, foreSurfaces, aftSurfaces, ContainerEnd.Aft, true); 
            var foreProtectingSurfaces = ProtectingSurfacesFactory.Create(containers, foreSurfaces, aftSurfaces, ContainerEnd.Fore, true);
             
            var building = new Building("1", 212.65, 0, 29, 14.3, 50, 33); 
            aftProtectingSurfaces.Add(building.AftSurface);
            foreProtectingSurfaces.Add(building.ForeSurface);


            double alpha = 25;

            var windExposedForeSurfaces = GetWindExposedSurfaces(alpha, foreSurfaces, aftProtectingSurfaces, true).OrderByDescending(surface => surface.Coordinate).ToList();
            var windExposedAftSurfaces = GetWindExposedSurfaces(alpha, aftSurfaces, foreProtectingSurfaces, true).OrderByDescending(surface => surface.Coordinate).ToList();
             
            return new LongitudinalSurfacesCalculationResult(){ Aft = windExposedAftSurfaces, Fore = windExposedForeSurfaces };

        }
         
    }


}

