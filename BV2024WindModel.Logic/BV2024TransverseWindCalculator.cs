#define PARALLEL

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BV2024WindModel.Abstractions;


namespace BV2024WindModel.Logic
{
    public class BV2024TransverseWindCalculator : AbstractBV2024WindCalculator, ICalculator<IEnumerable<Container>, TransverseSurfacesCalculationResult> 
    {
        public TransverseSurfacesCalculationResult Calculate(in IEnumerable<Container> input)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var containers = input.ToList();

            var portsideSurfaces = containers.GroupBy(container => container.PortsideSurface.Coordinate, container => container,
                (key, g) => new ContainersAtCoordinate(key, g.ToList(), ContainerEnd.Portside)).ToList();
            var starboardSurfaces = containers.GroupBy(container => container.StarboardSurface.Coordinate, container => container,
                (key, g) => new ContainersAtCoordinate(key, g.ToList(), ContainerEnd.Starboard)).ToList();


            stopWatch.Stop();
            Console.WriteLine($"Transverse Data preparation time {stopWatch.ElapsedMilliseconds}ms");
            stopWatch.Start();
            double alpha = 25;

            var portsideProtectingSurfaces = GetProtectingSurfaces(containers, starboardSurfaces, portsideSurfaces, ContainerEnd.Portside);
            var starboardProtectingSurfaces = GetProtectingSurfaces(containers, portsideSurfaces, starboardSurfaces, ContainerEnd.Starboard);

            var windExposedPortsideSurfaces = GetWindExposedSurfaces(alpha, portsideSurfaces, starboardProtectingSurfaces).OrderByDescending(surface => surface.Coordinate).ToList();
            var windExposedStarboardSurfaces = GetWindExposedSurfaces(alpha, starboardSurfaces, portsideProtectingSurfaces).OrderByDescending(surface => surface.Coordinate).ToList();

            stopWatch.Stop();
            Console.WriteLine($" Transverse calculation time {stopWatch.ElapsedMilliseconds}ms");

            return new TransverseSurfacesCalculationResult() { Portside = windExposedPortsideSurfaces, Starboard = windExposedStarboardSurfaces };

        }
    }


}

