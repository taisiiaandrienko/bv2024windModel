#define PARALLEL

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks;
using BV2024WindModel.Abstractions;
using Clipper2Lib;


namespace BV2024WindModel.Logic
{
    public abstract class AbstractBV2024WindCalculator
    {
        protected static IEnumerable<SurfaceCalculationResult> GetWindExposedSurfaces(double alpha, List<ContainersAtCoordinate> affectedSurfaces, List<Surface> protectingSurfaces)
        {
            var windExposedSurfaces = new List<SurfaceCalculationResult>();

#if PARALLEL
            Parallel.ForEach(affectedSurfaces, affectedSurface =>
#else
            foreach (var affectedSurface in affectedSurfaces)
#endif
            {
                var windExposedSurface = GetWindExposedSurfaceCalculationResult(alpha, protectingSurfaces, affectedSurface);
                windExposedSurfaces.Add(windExposedSurface);

            }
#if PARALLEL
            );
#endif

            return windExposedSurfaces;
        }

        private static SurfaceCalculationResult GetWindExposedSurfaceCalculationResult(double alpha, List<Surface> protectingSurfaces, ContainersAtCoordinate affectedSurface)
        {
            var allWindExposedContainers = new List<ContainerCalculationResult>();
            foreach (var container in affectedSurface.Containers)
            {
                var windExposedPolygon = new PathsD(container.PointsYZ);

                foreach (var protectingSurface in protectingSurfaces)
                {
                    if (protectingSurface.Coordinate > affectedSurface.Coordinate && Math.Abs(protectingSurface.Coordinate - affectedSurface.Coordinate) < 75)
                    {
                        bool needCalculate = NeedToCalculate(alpha, affectedSurface, protectingSurface);

                        if (needCalculate)
                        {
                            var deflatedPaths = PolygonDeflator.DeflatePolygon(protectingSurface, affectedSurface.Coordinate, alpha);
                            if (deflatedPaths != null)
                            {

                                windExposedPolygon = Clipper.Difference(windExposedPolygon, deflatedPaths, FillRule.NonZero, 8);

                            }
                        }
                    }
                }
                var containerCalculationResult = new ContainerCalculationResult
                {
                    ContainerId = container.id,
                    WindExposedPolygon = windExposedPolygon,
                    Area = AreaCalculator.CalcArea(windExposedPolygon)
                };
                allWindExposedContainers.Add(containerCalculationResult);
            }

            var windExposedSurface = new SurfaceCalculationResult { Result = allWindExposedContainers, End = affectedSurface.End, Coordinate = affectedSurface.Coordinate };

            //(frontSurface.Coordinate, frontSurface.Paths);
            return windExposedSurface;
        }
         
        private static bool NeedToCalculate(double alpha, ContainersAtCoordinate affectedSurface, Surface protectingSurface)
        {
            var needCalculate = false;

            var containersDist = Math.Abs(protectingSurface.Coordinate - affectedSurface.Coordinate);
            var tg = Math.Tan(alpha * (Math.PI / 180));
            var offset = -tg * containersDist;
             
            foreach (var path in protectingSurface.Paths)
            {
                var width = path.Max(point => point.x) - path.Min(point => point.x);
                var height = path.Max(point => point.y) - path.Min(point => point.y);
                if (width > 2 * Math.Abs(offset) && height > Math.Abs(offset))
                {
                    needCalculate = true;
                    break;
                }
            }
            return needCalculate;
        }

        protected static List<Surface> GetProtectingSurfaces(IEnumerable<Container> containersFromFile, List<ContainersAtCoordinate> Surfaces1, List<ContainersAtCoordinate> Surfaces2, ContainerEnd protectingFrom)
        {
            var crossingContainers = CrossingContainersProvider.GetCrossingContainers(containersFromFile, Surfaces1, Surfaces2, protectingFrom);

            var protectingContainers = (protectingFrom == ContainerEnd.Aft || protectingFrom == ContainerEnd.Portside) ? Surfaces2 : Surfaces1;
            var allProtectingContainers = new List<ContainersAtCoordinate>();
            allProtectingContainers.AddRange(protectingContainers);
            allProtectingContainers.AddRange(crossingContainers);

            var allProtectingSurfaces = allProtectingContainers.GroupBy(container => container.Coordinate, container => container,
                (key, g) => new ContainersAtCoordinate(key, g.SelectMany(x => x.Containers).ToList(), protectingFrom)).ToList();

            var unitedProtectingSurfaces = GetUnitedProtectingSurfaces(allProtectingSurfaces, protectingFrom);
            return unitedProtectingSurfaces;
        }

        private static List<Surface> GetUnitedProtectingSurfaces(List<ContainersAtCoordinate> protectingSurfaces, ContainerEnd protectingFrom)
        {
            var inflatedProtectingSurfacesBag = new ConcurrentBag<Surface>();
            var unionOffset = 0.3;

#if PARALLEL
            Parallel.ForEach(protectingSurfaces, protectingSurface =>
#else
            foreach (var protectingSurface in protectingSurfaces)
#endif
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
                var protectingPolygonsAtCoordinate = Clipper.InflatePaths(allContainers, unionOffset / 2, JoinType.Miter, EndType.Polygon);
                inflatedProtectingSurfacesBag.Add(new Surface(protectingSurface.Coordinate, protectingPolygonsAtCoordinate));
            }
#if PARALLEL
            );
#endif
            var unitedProtectingSurfaces = new List<Surface>();
            var inflatedProtectingSurfaces = inflatedProtectingSurfacesBag.ToList();

#if PARALLEL
            Parallel.ForEach(inflatedProtectingSurfaces, inflatedSurface =>
#else
            foreach (var inflatedSurface in inflatedProtectingSurfaces)
#endif
            {
                var protectingPolygonsAtCoordinate = Clipper.InflatePaths(inflatedSurface.Paths, -unionOffset / 2, JoinType.Miter, EndType.Polygon);
                unitedProtectingSurfaces.Add(new Surface(inflatedSurface.Coordinate, protectingPolygonsAtCoordinate));
            }
#if PARALLEL
        );
#endif

            return unitedProtectingSurfaces;
        }

    }


}

