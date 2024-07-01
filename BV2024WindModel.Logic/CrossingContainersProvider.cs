//#define PARALLEL

using System;
using System.Collections.Generic;
using System.Linq;
using BV2024WindModel.Abstractions;


namespace BV2024WindModel.Logic
{
    public class CrossingContainersProvider 
    {
        public static List<ContainersAtCoordinate> GetCrossingContainers(IEnumerable<Container> containersFromFile, List<ContainersAtCoordinate> Surfaces1, List<ContainersAtCoordinate> Surfaces2, ContainerEnd crossingAtEnd)
        {
            var crossingContainers = new List<ContainersAtCoordinate>();

            var windAffectedSurfaces = (crossingAtEnd == ContainerEnd.Aft || crossingAtEnd == ContainerEnd.Portside) ? Surfaces1 : Surfaces2;
            var windProtectingSurfaces = (crossingAtEnd == ContainerEnd.Fore || crossingAtEnd == ContainerEnd.Starboard) ? Surfaces1 : Surfaces2;
#if PARALLEL
            Parallel.ForEach(windAffectedSurfaces, windAffectedSurface =>
#else
            foreach (var windAffectedSurface in windAffectedSurfaces)
#endif
            {
                var crossingCoordinate = (crossingAtEnd == ContainerEnd.Aft) ? windAffectedSurface.Coordinate + 0.00001 : windAffectedSurface.Coordinate - 0.00001;
                var crossingContainersAtCoordinate = containersFromFile.Where(container => IsInside(container, crossingCoordinate, crossingAtEnd));

                if (crossingContainersAtCoordinate.Count() > 0)
                {
                    foreach (var windProtectingSurface in windProtectingSurfaces)
                    {
                        if (Math.Abs(windProtectingSurface.Coordinate - crossingCoordinate) < 0.1 && Protects(windProtectingSurface, crossingAtEnd, crossingCoordinate))
                        {
                            crossingContainers.Add(new ContainersAtCoordinate(windProtectingSurface.Coordinate, crossingContainersAtCoordinate.Select(container => container).ToList(), windProtectingSurface.End));
                        }
                    }
                }
            }
#if PARALLEL
        );
#endif

            return crossingContainers;
        }

        private static bool Protects(ContainersAtCoordinate windProtectingSurface, ContainerEnd crossingAtEnd, double crossingCoordinate)
        {
            if (crossingAtEnd == ContainerEnd.Aft || crossingAtEnd == ContainerEnd.Portside)
            {
                return crossingCoordinate <= windProtectingSurface.Coordinate;
            }
            return crossingCoordinate >= windProtectingSurface.Coordinate;
        }

        private static bool IsInside(Container container, double crossingCoordinate, ContainerEnd crossingAtEnd)
        {
            if( crossingAtEnd == ContainerEnd.Aft || crossingAtEnd == ContainerEnd.Fore)
            { 
                return crossingCoordinate >= container.AftSurface.Coordinate &&
                            crossingCoordinate <= container.ForeSurface.Coordinate;
            }
             return crossingCoordinate >= container.PortsideSurface.Coordinate &&
                                crossingCoordinate <= container.StarboardSurface.Coordinate;
        }
    }
}

