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
        protected WindForcesExternalCalculationParameters WindForcesExternalCalculationParameters;
        protected static IEnumerable<SurfaceCalculationResult> GetWindExposedSurfaces(double alpha, List<ContainersAtCoordinate> affectedSurfaces, List<Surface> protectingSurfaces, bool parallellise)
        {
            
            var windExposedSurfaces = new ConcurrentBag<SurfaceCalculationResult>();

            if (parallellise)
            {

                Parallel.ForEach(affectedSurfaces, affectedSurface =>
                {
                    var windExposedSurface = GetWindExposedSurfaceCalculationResult(alpha, protectingSurfaces, affectedSurface);
                    windExposedSurfaces.Add(windExposedSurface);
                });

            }
            else
            {
                foreach (var affectedSurface in affectedSurfaces)

                {
                    var windExposedSurface = GetWindExposedSurfaceCalculationResult(alpha, protectingSurfaces, affectedSurface);
                    windExposedSurfaces.Add(windExposedSurface);
                }
            }
            return windExposedSurfaces;
        }

        private static SurfaceCalculationResult GetWindExposedSurfaceCalculationResult(double alpha, List<Surface> protectingSurfaces, ContainersAtCoordinate affectedSurface)
        {

            var allWindExposedContainers = new List<ContainerCalculationResult>(); 
            foreach (var container in affectedSurface.Containers)
            {
                var containerPoints = (affectedSurface.End  == ContainerEnd.Fore || affectedSurface.End == ContainerEnd.Aft) ? container.PointsYZ : container.PointsXZ;
                var windExposedPolygon = new PathsD(containerPoints);
                var containerBounds = (affectedSurface.End == ContainerEnd.Fore || affectedSurface.End == ContainerEnd.Aft) ? container.TransverseBounds : container.LongitudinalBounds;
                foreach (var protectingSurface in protectingSurfaces)
                {
                    var canProtect = (affectedSurface.End == ContainerEnd.Fore || affectedSurface.End == ContainerEnd.Starboard) ? (protectingSurface.Coordinate > affectedSurface.Coordinate) : (protectingSurface.Coordinate < affectedSurface.Coordinate);

                    if (canProtect)
                    {
                        var protectingBounds = protectingSurface.Bounds;
                        if (containerBounds.MinX > protectingBounds.MaxX)
                            continue;
                        if (containerBounds.MaxX < protectingBounds.MinX)
                            continue;
                        if (containerBounds.MinY > protectingBounds.MaxY)
                            continue;
                        if (containerBounds.MaxY < protectingBounds.MinY)
                            continue;

                        if (Math.Abs(protectingSurface.Coordinate - affectedSurface.Coordinate) <= 0.5)
                        {
                            windExposedPolygon = CheckForFullProtection(windExposedPolygon, containerBounds, protectingSurface, protectingBounds);
                            if (windExposedPolygon.Count == 0)
                            {
                                break;
                            }
                            windExposedPolygon = Clipper.Difference(windExposedPolygon, protectingSurface.Paths, FillRule.NonZero, 8);

                            if (windExposedPolygon.Count == 0)
                            {
                                break;
                            }
                            var area = AreaCalculator.CalcArea(windExposedPolygon);
                            if (area < 0.001)
                            {
                                windExposedPolygon = new PathsD();
                                break;
                            }
                        }
                        else
                        { 
                            bool needCalculate = NeedToCalculate(alpha, affectedSurface, protectingSurface);
                            if (needCalculate)
                            {
                                var deflatedPaths = PolygonDeflator.DeflatePolygon(protectingSurface, affectedSurface.Coordinate, alpha);
                                if (deflatedPaths != null)
                                { 
                                    windExposedPolygon = Clipper.Difference(windExposedPolygon, deflatedPaths, FillRule.NonZero, 8);
                                    if (windExposedPolygon.Count == 0)
                                        break;
                                }
                            }
                        }
                        
                    }
                    if (windExposedPolygon.Count == 0)
                        break;
                }

                var forceCalculator = new WindForceCalculator();
                var fullWidth = (affectedSurface.End == ContainerEnd.Fore || affectedSurface.End == ContainerEnd.Aft) ? container.Width : container.Length;
                var fullHeight = container.Height;
                var fullArea = fullWidth * fullHeight;
                var exposedArea = AreaCalculator.CalcArea(windExposedPolygon);
                var volumetricCenter = container.Basis + fullHeight / 2;

                var containerCalculationResult = new ContainerCalculationResult
                {
                    ContainerId = container.ID,
                    FullHeight = fullHeight,
                    FullWidth = fullWidth,
                    VolumetricCenter = volumetricCenter,
                    FullArea = fullArea,
                    WindExposedPolygon = windExposedPolygon,
                    ExposedArea = exposedArea
                };

                allWindExposedContainers.Add(containerCalculationResult);
            }

            var windExposedSurface = new SurfaceCalculationResult { Result = allWindExposedContainers, End = affectedSurface.End, Coordinate = affectedSurface.Coordinate };

            //(frontSurface.Coordinate, frontSurface.Paths);
            return windExposedSurface;
        }

        private static PathsD CheckForFullProtection(PathsD windExposedPolygon, Bounds containerBounds, Surface protectingSurface, Bounds protectingBounds)
        {
            foreach (var path in protectingSurface.Paths)
            {
                //if (protectingSurface.Paths.Count == 1 && protectingSurface.Paths[0].Count == 4)
                if (path.Count == 4 && protectingSurface.Paths.Count == 1)
                {
                    if (IsLowerOrSimilar(containerBounds, protectingBounds) && IsRightOrSimilar(containerBounds, protectingBounds) && IsLeftOrSimilar(containerBounds, protectingBounds))
                    {
                        windExposedPolygon = new PathsD();
                        break;
                    }
                }
                if (path.Count == 4 && protectingSurface.Paths.Count != 1)
                {
                    var pathBounds = GetBounds(new PathsD { path });
                    if (IsLowerOrSimilar(containerBounds, pathBounds) && IsRightOrSimilar(containerBounds, pathBounds) && IsLeftOrSimilar(containerBounds, pathBounds))
                    {
                        windExposedPolygon = new PathsD();
                        break;
                    }
                }
            }

            return windExposedPolygon;
        }

        private static bool IsLeftOrSimilar(Bounds containerBounds, Bounds protectingBounds)
        {
            return Math.Abs(containerBounds.MinX - protectingBounds.MinX) < 0.001 || containerBounds.MinX > protectingBounds.MinX;
        }

        private static bool IsRightOrSimilar(Bounds containerBounds, Bounds protectingBounds)
        {
            return Math.Abs(containerBounds.MaxX - protectingBounds.MaxX) < 0.001|| containerBounds.MaxX < protectingBounds.MaxX;
        }

        private static bool IsLowerOrSimilar(Bounds containerBounds, Bounds protectingBounds)
        {
            return Math.Abs(containerBounds.MaxY - protectingBounds.MaxY) < 0.001 || containerBounds.MaxY < protectingBounds.MaxY ;
        }

        private static Bounds GetBounds(PathsD paths)
        {
            var minx = paths.SelectMany(path => path).Min(point => point.x);
            var maxx = paths.SelectMany(path => path).Max(point => point.x);
            var miny = paths.SelectMany(path => path).Min(point => point.y);
            var maxy = paths.SelectMany(path => path).Max(point => point.y);
            return new Bounds { MinX = minx, MaxX = maxx, MinY = miny, MaxY = maxy };
        }
        private static bool NeedToCalculate(double alpha, ContainersAtCoordinate affectedSurface, Surface protectingSurface)
        {
            var needCalculate = false;
            var protectingDistance = (affectedSurface.End == ContainerEnd.Fore || affectedSurface.End == ContainerEnd.Aft) ? 75 : 14;
            var containersDist = Math.Abs(protectingSurface.Coordinate - affectedSurface.Coordinate);
            if (containersDist < protectingDistance)
            { 
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
            }
            return needCalculate;
        } 

    }
    public static class Utils
    {
        public static void AddRange<T>(this ConcurrentBag<T> @this, IEnumerable<T> toAdd)
        {
            foreach (var element in toAdd)
            {
                @this.Add(element);
            }
        } 
    }
}

