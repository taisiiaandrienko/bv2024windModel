﻿#define PARALLEL
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BV2024WindModel.Abstractions;


namespace BV2024WindModel.Logic
{
    public class BV2024TransverseWindCalculator : AbstractBV2024WindCalculator, ICalculator<WindCalculatorInput, TransverseSurfacesCalculationResult> 
    {
        public TransverseSurfacesCalculationResult Calculate(in WindCalculatorInput input)
        { 
            double alpha = input.Vessel.Alpha;

            var containers = input.Containers.ToList();
               
            List<Cluster> clusters = new List<Cluster>();
            var result = new List<IEnumerable<Container>>();

            const double maxGap = 0.5;

            foreach (var container in containers)
            {
                var max = container.ForeSurface.Coordinate;
                var min = container.AftSurface.Coordinate;
                var mid = ( min + max ) / 2;
                if (clusters.Count == 0)
                {
                    var cluster = new Cluster { Min = min, Max = max, Containers = new List<Container>{container} };
                    clusters.Add(cluster);
                }
                else
                {
                    Cluster currentCluster = null;
                    Cluster clusterToAdd = null;
                    var fittingCluster = clusters.FirstOrDefault(cluster => Fits(cluster, max, min, mid));
                    if (fittingCluster != null)
                    {
                        fittingCluster.Min = Math.Min(min - maxGap, fittingCluster.Min);
                        fittingCluster.Max = Math.Max(max + maxGap, fittingCluster.Max);
                        fittingCluster.Containers.Add(container);
                        currentCluster = fittingCluster;
                    }
                    else
                    {
                        currentCluster = clusterToAdd = new Cluster { Min = min - maxGap, Max = max + maxGap, Containers = new List<Container> { container } };

                        var currentClusterMid = (currentCluster.Min + currentCluster.Max) / 2;
                        var clustersToCombine = clusters.FirstOrDefault(cluster => Fits(cluster, currentCluster.Max, currentCluster.Min, currentClusterMid));
                        if (clustersToCombine != null)
                        {

                        }
                        if (clusterToAdd != null)
                        {
                            clusters.Add(currentCluster);
                        }
                    }
                }
            }

            var windExposedStarboardSurfaces = new ConcurrentBag<SurfaceCalculationResult>();
            var windExposedPortsideSurfaces = new ConcurrentBag<SurfaceCalculationResult>();
#if PARALLEL
            Parallel.ForEach(clusters, cluster =>
#else
            foreach (var cluster in clusters)
#endif
            {
                var containersInCluster = cluster.Containers;
                var portsideSurfaces = containersInCluster.GroupBy(container => container.PortsideSurface.Coordinate, container => container,
                    (key, g) => new ContainersAtCoordinate(key, g.ToList(), ContainerEnd.Portside)).ToList();
                var starboardSurfaces = containersInCluster.GroupBy(container => container.StarboardSurface.Coordinate, container => container,
                    (key, g) => new ContainersAtCoordinate(key, g.ToList(), ContainerEnd.Starboard)).ToList();

                var portsideProtectingSurfaces = ProtectingSurfacesFactory.Create(containersInCluster, starboardSurfaces, portsideSurfaces, ContainerEnd.Portside, false);
                var starboardProtectingSurfaces = ProtectingSurfacesFactory.Create(containersInCluster, starboardSurfaces, portsideSurfaces, ContainerEnd.Starboard, false);
                
                var windExposedStarboardSurfacesInCluster = GetWindExposedSurfaces(alpha, starboardSurfaces, portsideProtectingSurfaces, false);
                var windExposedPortsideSurfacesInCluster = GetWindExposedSurfaces(alpha, portsideSurfaces, starboardProtectingSurfaces, false);
                
                windExposedStarboardSurfaces. AddRange(windExposedStarboardSurfacesInCluster);
                windExposedPortsideSurfaces.AddRange(windExposedPortsideSurfacesInCluster);
            }
#if PARALLEL
            );
#endif



            return new TransverseSurfacesCalculationResult() { Portside = windExposedPortsideSurfaces.OrderByDescending(surface => surface.Coordinate).ToList(), Starboard = windExposedStarboardSurfaces.OrderByDescending(surface => surface.Coordinate).ToList() };

        }

        private static bool Fits(Cluster cluster, double max, double min, double mid)
        {
            return (cluster.Min <= max && max <= cluster.Max) ||
                                (cluster.Min <= min && min <= cluster.Max) || (cluster.Min >= min && max >= cluster.Max) || (cluster.Min <= mid && mid <= cluster.Max);
        }

        private static bool FitsLongitudinaly(Container protecting, Container container)
        {
            return protecting.LongitudinalBounds.MaxX >= container.LongitudinalBounds.MinX && protecting.LongitudinalBounds.MinX <= container.LongitudinalBounds.MaxX;
        }
    }
}

