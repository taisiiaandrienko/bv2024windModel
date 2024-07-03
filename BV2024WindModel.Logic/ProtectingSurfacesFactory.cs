using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BV2024WindModel.Abstractions;
using Clipper2Lib;


namespace BV2024WindModel.Logic
{
    public class ProtectingSurfacesFactory
    {
        public static List<Surface> Create(IEnumerable<Container> containersFromFile, List<ContainersAtCoordinate> Surfaces1, List<ContainersAtCoordinate> Surfaces2, ContainerEnd protectingFrom, bool parallellise)
        {
            var crossingContainers = CrossingContainersProvider.GetCrossingContainers(containersFromFile, Surfaces1, Surfaces2, protectingFrom, parallellise);

            var protectingContainers = (protectingFrom == ContainerEnd.Aft || protectingFrom == ContainerEnd.Portside) ? Surfaces2 : Surfaces1; 
            var allProtectingContainers = new List<ContainersAtCoordinate>();
            allProtectingContainers.AddRange(protectingContainers);
            allProtectingContainers.AddRange(crossingContainers);

            var allProtectingSurfaces = allProtectingContainers.GroupBy(container => container.Coordinate, container => container,
                (key, g) => new ContainersAtCoordinate(key, g.SelectMany(x => x.Containers).ToList(), protectingFrom)).ToList();

            var unitedProtectingSurfaces = GetUnitedProtectingSurfaces(allProtectingSurfaces, protectingFrom, parallellise);

            return unitedProtectingSurfaces;
        }

        private static List<Surface> GetUnitedProtectingSurfaces(List<ContainersAtCoordinate> protectingSurfaces, ContainerEnd protectingFrom, bool parallellise)
        {
            var inflatedProtectingSurfacesBag = new ConcurrentBag<Surface>();
            var unionOffset = 0.3;


            if (parallellise)
            {
                Parallel.ForEach(protectingSurfaces, protectingSurface =>
                {
                    var allContainers = new PathsD();
                    foreach (var container in protectingSurface.Containers)
                    {
                        var paths = (protectingFrom == ContainerEnd.Fore || protectingFrom == ContainerEnd.Aft) ? container.PointsYZ : container.PointsXZ;
                        foreach (var path in paths)
                        {
                            allContainers.Add(path);
                        }
                    }
                    var protectingPolygonsAtCoordinate = Clipper.InflatePaths(allContainers, unionOffset / 2, JoinType.Miter, EndType.Polygon, 4.0, 8);

                    inflatedProtectingSurfacesBag.Add(new Surface(protectingSurface.Coordinate, protectingPolygonsAtCoordinate));
                }
                );
            }
            else
            {
                foreach (var protectingSurface in protectingSurfaces)
                {
                    var allContainers = new PathsD();
                    foreach (var container in protectingSurface.Containers)
                    {
                        var paths = (protectingFrom == ContainerEnd.Fore || protectingFrom == ContainerEnd.Aft) ? container.PointsYZ : container.PointsXZ;
                        foreach (var path in paths)
                        {
                            allContainers.Add(path);
                        }
                    }
                    var protectingPolygonsAtCoordinate = Clipper.InflatePaths(allContainers, unionOffset / 2, JoinType.Miter, EndType.Polygon, 4.0, 8);

                    inflatedProtectingSurfacesBag.Add(new Surface(protectingSurface.Coordinate, protectingPolygonsAtCoordinate));
                }
            }


            var unitedProtectingSurfaces = new List<Surface>();
            var inflatedProtectingSurfaces = inflatedProtectingSurfacesBag.ToList();
            if (parallellise)
            {
                Parallel.ForEach(inflatedProtectingSurfaces, inflatedSurface =>
                {
                    var protectingPolygonsAtCoordinate = Clipper.InflatePaths(inflatedSurface.Paths, -unionOffset / 2, JoinType.Miter, EndType.Polygon, 4.0, 8);

                    unitedProtectingSurfaces.Add(new Surface(inflatedSurface.Coordinate, protectingPolygonsAtCoordinate));
                });
            }
            else
            {
                foreach (var inflatedSurface in inflatedProtectingSurfaces)
                {
                    var protectingPolygonsAtCoordinate = Clipper.InflatePaths(inflatedSurface.Paths, -unionOffset / 2, JoinType.Miter, EndType.Polygon, 4.0, 8);

                    unitedProtectingSurfaces.Add(new Surface(inflatedSurface.Coordinate, protectingPolygonsAtCoordinate));
                }
            }
            if (protectingFrom == ContainerEnd.Aft || protectingFrom == ContainerEnd.Portside)
            {
                unitedProtectingSurfaces = unitedProtectingSurfaces.OrderBy(surface => surface.Coordinate).ToList();
            }
            else
            {
                unitedProtectingSurfaces = unitedProtectingSurfaces.OrderByDescending(surface => surface.Coordinate).ToList();
            }
            return unitedProtectingSurfaces;
        }
    }
}

