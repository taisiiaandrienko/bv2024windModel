//#define PARALLEL

using System;
using System.Collections.Concurrent;
using System.Collections.Generic; 
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BV2024WindModel.Abstractions;
using Clipper2Lib;
using Macs3.Core.Mathematics.GeneralPolygonClipperLibrary;


namespace BV2024WindModel.Logic
{

    public class BV2024WindCalculator : ICalculator<IEnumerable<Container>, LongitudinalSurfacesCalculationResult>
    {
        
        public LongitudinalSurfacesCalculationResult Calculate(in IEnumerable<Container> input)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var containers = input.ToList();
            var foreSurfaces = containers.GroupBy(container => container.ForeSurface.Coordinate, container => container,
                (key, g) => new ContainersAtCoordinate(key, g.ToList(), ContainerEnd.Fore)).ToList();

            var aftSurfaces = containers.GroupBy(container => container.AftSurface.Coordinate, container => container,
                (key, g) => new ContainersAtCoordinate(key, g.ToList(), ContainerEnd.Aft)).ToList();

            var aftProtectingSurfaces = GetProtectingSurfaces(containers, foreSurfaces, aftSurfaces, ContainerEnd.Aft); 
            var foreProtectingSurfaces = GetProtectingSurfaces(containers, foreSurfaces, aftSurfaces, ContainerEnd.Fore);
             
            var building = new Building("1", 212.65, 0, 29, 14.3, 50, 33); 
            aftProtectingSurfaces.Add(building.AftSurface);
            foreProtectingSurfaces.Add(building.ForeSurface);


            stopWatch.Stop();
            Console.WriteLine($"Data preparation time {stopWatch.ElapsedMilliseconds}ms");
            stopWatch.Restart();
            double alpha = 25;

            var windExposedForeSurfaces = GetWindExposedSurfaces(alpha, foreSurfaces, aftProtectingSurfaces).OrderByDescending(surface => surface.Coordinate).ToList();
            var windExposedAftSurfaces = GetWindExposedSurfaces(alpha, aftSurfaces, foreProtectingSurfaces).OrderByDescending(surface => surface.Coordinate).ToList();

            stopWatch.Stop();
            Console.WriteLine($"Calculation time {stopWatch.ElapsedMilliseconds}ms");
            foreach (var windExposedFrontSurface in aftProtectingSurfaces)
            {
                Console.WriteLine($"X= {windExposedFrontSurface.Coordinate}");
            }
            
            return new LongitudinalSurfacesCalculationResult(){ Fore = windExposedForeSurfaces, Aft = windExposedAftSurfaces };

        }

        private static IEnumerable<LongitudinalSurfaceCalculationResult> GetWindExposedSurfaces(double alpha, List<ContainersAtCoordinate> affectedSurfaces, List<Surface> protectingSurfaces)
        {
            var windExposedSurfaces = new List<LongitudinalSurfaceCalculationResult>();

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

        private static LongitudinalSurfaceCalculationResult GetWindExposedSurfaceCalculationResult(double alpha, List<Surface> protectingSurfaces, ContainersAtCoordinate affectedSurface)
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

            var windExposedFrontSurface = new LongitudinalSurfaceCalculationResult { Result = allWindExposedContainers, End = affectedSurface.End, Coordinate = affectedSurface.Coordinate };

            //(frontSurface.Coordinate, frontSurface.Paths);
            return windExposedFrontSurface;
        }

        private static bool NeedToCalculate(double alpha, ContainersAtCoordinate affectedSurface, Surface protectingSurface)
        {
            var needCalculate = false;

            var containersDist = Math.Abs(protectingSurface.Coordinate - affectedSurface.Coordinate);
            var tg = Math.Tan(alpha * (Math.PI / 180));
            var offset = -tg * containersDist;

            var deflatedPolygons = new List<PolyDefault>();
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

        private static List<Surface> GetProtectingSurfaces(IEnumerable<Container> containersFromFile, List<ContainersAtCoordinate> foreSurfaces, List<ContainersAtCoordinate> aftSurfaces, ContainerEnd protectingFrom)
        {
            var crossingContainers = GetCrossingContainers(containersFromFile, foreSurfaces, aftSurfaces, protectingFrom);

            var protectingContainers = (protectingFrom == ContainerEnd.Aft) ? aftSurfaces : foreSurfaces;
            var allProtectingContainers = new List<ContainersAtCoordinate>();
            allProtectingContainers.AddRange(protectingContainers);
            allProtectingContainers.AddRange(crossingContainers);
            
            var allProtectingSurfaces = allProtectingContainers.GroupBy(container => container.Coordinate, container => container,
                (key, g) => new ContainersAtCoordinate(key, g.SelectMany(x => x.Containers).ToList(), protectingFrom)).ToList();
             
            var unitedProtectingSurfaces = GetUnitedProtectingSurfaces(allProtectingSurfaces); 
            return unitedProtectingSurfaces;
        }

        private static List<Surface> GetUnitedProtectingSurfaces(List<ContainersAtCoordinate> protectingSurfaces)
        {
            var inflatedProtectingSurfacesBag = new ConcurrentBag<Surface>();
            var unionOffset = 0.3;

#if PARALLEL
            Parallel.ForEach(allAftSurfaces, aftSurface =>
#else
            foreach (var protectingSurface in protectingSurfaces)
#endif
            {
                var allContainers = new PathsD();
                foreach (var container in protectingSurface.Containers)
                {
                    foreach (var path in container.PointsYZ)
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

        private static List<ContainersAtCoordinate> GetCrossingContainers(IEnumerable<Container> containersFromFile, List<ContainersAtCoordinate> foreSurfaces, List<ContainersAtCoordinate> aftSurfaces, ContainerEnd crossingAtEnd)
        {
            var crossingContainers = new List<ContainersAtCoordinate>();

            var windAffectedSurfaces = (crossingAtEnd == ContainerEnd.Aft) ? foreSurfaces: aftSurfaces;
            var windProtectingSurfaces = (crossingAtEnd == ContainerEnd.Fore) ? foreSurfaces : aftSurfaces;
#if PARALLEL
            Parallel.ForEach(windAffectedSurfaces, windAffectedSurface =>
#else
            foreach (var windAffectedSurface in windAffectedSurfaces)
#endif
            {
                var crossingCoordinate = (crossingAtEnd == ContainerEnd.Aft) ? windAffectedSurface.Coordinate + 0.01: windAffectedSurface.Coordinate - 0.01;
                var crossingContainersAtCoordinate = containersFromFile.Where(container => IsInside(container, crossingCoordinate));

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
            if( crossingAtEnd == ContainerEnd.Aft)
            { 
                return crossingCoordinate <= windProtectingSurface.Coordinate;
            }
            return crossingCoordinate >= windProtectingSurface.Coordinate;
        }

        private static bool IsInside(Container container, double crossingCoordinate)
        {

            return crossingCoordinate >= container.AftSurface.Coordinate &&
                            crossingCoordinate <= container.ForeSurface.Coordinate;
        }
    }
}

