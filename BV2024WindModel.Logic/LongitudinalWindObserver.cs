using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BV2024WindModel.Abstractions;

namespace BV2024WindModel.Logic
{
    public class LongitudinalWindObserver<T>
    {
        private Vessel Vessel;
        public LongitudinalWindObserver(Vessel vessel) 
        { 
            Vessel = vessel;
        }
       public List<ObserverResult> DetectChangedAreasForDeleting(List<Container> previousContainers, List<Container> deletedContainers)
        {
            (var containerCoordinatePairs, var containersHeightFore, var containersHeightAft) = GetCoordinatePairsAndHeights(deletedContainers);

            var allRelevantContainers = new List<Container>();
            var relevantPreviousContainers = previousContainers.Where(container => IsRelevant(container, containerCoordinatePairs)).ToList();
            allRelevantContainers.AddRange(relevantPreviousContainers);

            List<ObserverResult> observerResults = GetObserverReult(previousContainers, containerCoordinatePairs, containersHeightFore, containersHeightAft, allRelevantContainers);
            return observerResults;
        } 

        private static bool IsRelevant(Container container, List<Bounds> coordinatePairs)
        { 
            foreach (var pair in coordinatePairs)
            {
                if(container.AftSurface.Coordinate > pair.MaxX + 10)
                {
                    continue;
                }
                if (container.ForeSurface.Coordinate < pair.MinX - 10)
                {
                    continue;
                }
                if (container.AftSurface.Coordinate >= pair.MinX - 10 && container.ForeSurface.Coordinate <= pair.MaxX + 10)
                {
                    return true;
                } 
            } 
            return false;
        }
        public List<ObserverResult> DetectChangedAreasForAdding(List<Container> previousContainers, List<Container> newContainers)
        { 
            (var containerCoordinatePairs, var containersHeightFore, var containersHeightAft) = GetCoordinatePairsAndHeights(newContainers);

            var allRelevantContainers = new List<Container>();
            var relevantPreviousContainers = previousContainers.Where(container => IsRelevant(container, containerCoordinatePairs)).ToList();
            allRelevantContainers.AddRange(relevantPreviousContainers);
            allRelevantContainers.AddRange(newContainers);
             
            List<ObserverResult> observerResults = GetObserverReult(previousContainers, containerCoordinatePairs, containersHeightFore, containersHeightAft, allRelevantContainers);
            return observerResults;
        }
        private List<ObserverResult> GetObserverReult(List<Container> previousContainers, List<Bounds> containerCoordinatePairs, List<Bounds> containersHeightFore, List<Bounds> containersHeightAft, List<Container> allRelevantContainers)
        {
            var aftSurfaces = allRelevantContainers.GroupBy(container => container.AftSurface.Coordinate, container => container,
                            (key, g) => new ContainersAtCoordinate(key, g.ToList(), ContainerEnd.Aft)).ToList();
            var foreSurfaces = allRelevantContainers.GroupBy(container => container.ForeSurface.Coordinate, container => container,
                (key, g) => new ContainersAtCoordinate(key, g.ToList(), ContainerEnd.Fore)).ToList();

            var allProtectingContainersAft = GetAllProtectingContainers(allRelevantContainers, aftSurfaces, foreSurfaces, ContainerEnd.Aft);
            var allProtectingContainersFore = GetAllProtectingContainers(allRelevantContainers, aftSurfaces, foreSurfaces, ContainerEnd.Fore);

            List<ObserverResult> observerResults = GetObserverResults(previousContainers, allProtectingContainersAft, allProtectingContainersFore, containerCoordinatePairs, containersHeightFore, containersHeightAft);
            return observerResults;
        }
        private static List<ContainersAtCoordinate> GetAllProtectingContainers(List<Container> allRelevantContainers, List<ContainersAtCoordinate> aftSurfaces, List<ContainersAtCoordinate> foreSurfaces, ContainerEnd end)
        {
            var protectingContainers = end == ContainerEnd.Aft ? aftSurfaces : foreSurfaces;
            var allProtectingContainersAft = new List<ContainersAtCoordinate>();
            allProtectingContainersAft.AddRange(protectingContainers);
            var crossingContainers = CrossingContainersProvider.GetCrossingContainers(allRelevantContainers, foreSurfaces, aftSurfaces, end, false);
            allProtectingContainersAft.AddRange(crossingContainers);
            return allProtectingContainersAft;
        }

        private static (List<Bounds>, List<Bounds>, List<Bounds>) GetCoordinatePairsAndHeights(List<Container> newContainers)
        {
            var containerCoordinatePairs = new List<Bounds>();
            var containersHeight = new List<Bounds>();
            foreach (var container in newContainers)
            {
                var maxX = container.ForeSurface.Coordinate;
                var minX = container.AftSurface.Coordinate;
                var maxY = container.TransverseBounds.MaxY;
                var minY = container.TransverseBounds.MinY;
                var containerCoordinates = new Bounds { MaxX = maxX, MinX = minX };
                var containerHeight = new Bounds { MaxX = maxX, MinX = minX, MaxY = maxY, MinY = minY };

                containerCoordinatePairs.Add(containerCoordinates);
                containersHeight.Add(containerHeight);
            }
            containerCoordinatePairs = containerCoordinatePairs.Distinct(new BoundComparer()).ToList();
            var containersHeightFore = containersHeight.GroupBy(bound => bound.MaxX, bound => bound, (key, g) => new Bounds { MaxX = key, MaxY = g.Max(b => b.MaxY), MinY = g.Min(b => b.MinY) }).ToList();
            var containersHeightAft = containersHeight.GroupBy(bound => bound.MinX, bound => bound, (key, g) => new Bounds { MinX = key, MaxY = g.Max(b => b.MaxY), MinY = g.Min(b => b.MinY) }).ToList();
            return (containerCoordinatePairs, containersHeightFore, containersHeightAft);
        }


        private List<ObserverResult> GetObserverResults(List<Container> previousContainers, List<ContainersAtCoordinate> aftSurfaces, List<ContainersAtCoordinate> foreSurfaces, List<Bounds> containerCoordinatePairs, List<Bounds> containersHeightFore, List<Bounds> containersHeightAft)
        {
            var tg = Math.Tan(Vessel.Alpha * (Math.PI / 180));
              
            ObserverResult observerResultsFore = null;
            ObserverResult observerResultsAft = null;
            Parallel.For(0, 2, index =>
            {
                if (index == 0)
                {
                    observerResultsFore = GetObserverResut(previousContainers, aftSurfaces, containerCoordinatePairs, containersHeightAft, tg, ContainerEnd.Fore);
                }
                else
                {
                    observerResultsAft = GetObserverResut(previousContainers, foreSurfaces, containerCoordinatePairs, containersHeightFore, tg, ContainerEnd.Aft);
                }
            }); 

            var observerResults = new List<ObserverResult> { observerResultsFore, observerResultsAft };
            return observerResults;
        }
        
        private ObserverResult GetObserverResut(List<Container> previousContainers, List<ContainersAtCoordinate> protectingSurfaces, List<Bounds> containerCoordinatePairs, List<Bounds> containersHeight, double tg, ContainerEnd end)
        {
            var allContainersToRecalculate = new List<Container>();
            GetContainersToRecalculate(previousContainers, containerCoordinatePairs, containersHeight, protectingSurfaces, tg, allContainersToRecalculate, end);
            var oppositEnd = end == ContainerEnd.Fore ? ContainerEnd.Aft : ContainerEnd.Fore;
            var aftSurfacesToRecalculate = allContainersToRecalculate.GroupBy(container => GetCoordinate(container, end), container => container,
               (key, g) => new ContainersAtCoordinate(key, g.ToList(), oppositEnd)).ToList();
            var observerResults = new ObserverResult() { Criteria = new List<Criterium>(), Containers = new List<Container>() };
            observerResults.Containers.AddRange(allContainersToRecalculate);
            foreach (var surface in aftSurfacesToRecalculate)
            {
                var portsideOfProtectedArea = surface.Containers.Min(container => container.TransverseBounds.MinX);
                var starboardOfProtectedArea = surface.Containers.Max(container => container.TransverseBounds.MaxX);
                var criterium = new Criterium { Coordinate = surface.Coordinate, Portside = portsideOfProtectedArea, Starboard = starboardOfProtectedArea };
                observerResults.Criteria.Add(criterium);
            }

            return observerResults;
        }

        private static double GetCoordinate(Container container, ContainerEnd end)
        {
            if ( end == ContainerEnd.Fore)
            { 
                return container.AftSurface.Coordinate; 
            }
            else
            {
                return container.ForeSurface.Coordinate;
            }
        }

        private void GetContainersToRecalculate(List<Container> allContainers, List<Bounds> containerCoordinatePairs, List<Bounds> containersHeight, List<ContainersAtCoordinate> biggerSurfaces, double tg, List<Container> allcontainersToRecalculate, ContainerEnd containerEnd)
        {
            foreach (var coordinates in containerCoordinatePairs)
            {
                double buildingMinY = 1000;
                double buildingMaxY = -1000;
                double buildingMaxZ = 0;
                foreach (var building in Vessel.Buildings)
                {
                    var diff = containerEnd == ContainerEnd.Fore ? coordinates.MinX - building.AftSurface.Coordinate : building.ForeSurface.Coordinate - coordinates.MaxX ;
                    if (diff < 0)
                    {
                        var dist =  Math.Abs(diff);
                        var offset = tg * dist;
                        var maxX = building.TransverseBounds.MaxX;
                        var minX = building.TransverseBounds.MinX;
                        var maxYCoord = building.TransverseBounds.MaxY;
                        if (maxX - minX > 2 * offset && maxYCoord > offset)
                        {
                            var newMinY = minX + offset;
                            var newMaxY = maxX - offset;
                            var newMaxZ = maxYCoord - offset;
                            if (newMinY < buildingMinY && newMaxY > buildingMaxY && newMaxZ > buildingMaxZ)
                            {
                                buildingMaxY = newMaxY;
                                buildingMinY = newMinY;
                                buildingMaxZ = newMaxZ;
                            }
                        }
                    }
                }
                var surfaceWithBiggerBounds = containerEnd == ContainerEnd.Fore ? biggerSurfaces.FirstOrDefault(surf => Math.Abs(surf.Coordinate - coordinates.MinX) < 1e-6) : biggerSurfaces.FirstOrDefault(surf => Math.Abs(surf.Coordinate - coordinates.MaxX) < 1e-6);
                var surfaceContainers = surfaceWithBiggerBounds.Containers;
                var portsideOfProtectedArea = surfaceContainers.Min(container => container.TransverseBounds.MinX);
                var starboardOfProtectedArea = surfaceContainers.Max(container => container.TransverseBounds.MaxX);

                var currentBounds = containerEnd == ContainerEnd.Fore ? containersHeight.FirstOrDefault(bound => (bound.MinX - coordinates.MinX) < 1e-6) : containersHeight.FirstOrDefault(bound => (bound.MaxX - coordinates.MaxX) < 1e-6 );
                var topOfprotectedArea = currentBounds.MaxY; 
                //var bottomOfProtectedArea = DeckHeight;
                //If we take the lower container as a bottom of protected area we will get smaller amount of containers(respectively stacks) to recalculate
                var bottomOfProtectedArea = currentBounds.MinY;
                if (portsideOfProtectedArea >= buildingMinY && starboardOfProtectedArea <= buildingMaxY && topOfprotectedArea <= buildingMaxZ)
                {
                }
                else
                {  
                    var containersToRecalculate = new List<Container>();
                    if (containerEnd == ContainerEnd.Fore)
                    {
                        containersToRecalculate = allContainers.Where(container => CanBeProtected(container, tg, coordinates.MinX, portsideOfProtectedArea, starboardOfProtectedArea, topOfprotectedArea, bottomOfProtectedArea, Vessel.Buildings, containerEnd)).ToList();
                    }
                    else
                    {
                        containersToRecalculate = allContainers.Where(container => CanBeProtected(container, tg, coordinates.MaxX, portsideOfProtectedArea, starboardOfProtectedArea, topOfprotectedArea, bottomOfProtectedArea, Vessel.Buildings, containerEnd)).ToList();

                    }
                    allcontainersToRecalculate.AddRange(containersToRecalculate);
                }

            }
        }

        private static bool CanBeProtected(Container container, double tg, double surfaceCoordinate, double portside, double starboard, double maxZ, double minZ, List<Building> Buildings, ContainerEnd containerEnd)
        {
            var diff = containerEnd == ContainerEnd.Fore ? container.ForeSurface.Coordinate - surfaceCoordinate : surfaceCoordinate - container.AftSurface.Coordinate;
            var dist = Math.Abs(diff);
            var offset = dist * tg;
            if(offset > 10.0 && offset < 11.0)
            {

            }
            var portsideOfProtectedArea = portside + offset;
            var starboardOfProtectedArea = starboard - offset;
            var topOfProtectedArea = maxZ - offset;
            var bottomOfProtectedArea = minZ - offset;
            if(diff >= 0)
            {
                return false;
            }
            if (dist >= (starboard - portside) / (2 * tg))
            {
                return false;
            }
            if (dist >= maxZ / tg)
            {
                return false;
            }
            if (container.TransverseBounds.MinY >= topOfProtectedArea)
            {
                return false;
            }
            if (container.TransverseBounds.MaxY <= bottomOfProtectedArea)
            {
                return false;
            }
            if (container.TransverseBounds.MaxX <= portsideOfProtectedArea || container.TransverseBounds.MinX >= starboardOfProtectedArea)
            {
                return false;
            }
            foreach (var building in Buildings)
            {
                if (containerEnd == ContainerEnd.Fore)
                {
                    var diffToBuilding = container.ForeSurface.Coordinate - building.ForeSurface.Coordinate;
                    if (diffToBuilding < 0)
                    {
                        if (dist >= Math.Abs(diffToBuilding))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    var diffToBuilding = building.AftSurface.Coordinate - container.AftSurface.Coordinate;
                    if (diffToBuilding < 0)
                    {
                        if (dist >= Math.Abs(diffToBuilding))
                        {
                            return false;
                        }
                    }
                   
                }
            }
                return true;         
        }

        public List<Container> DetectChangedAreasForDelition(List<Container> allContainers, List<T> ids )
        { 
            var containersToDelete = allContainers.Where(container => ids.Contains((T)(object) container.ID)).ToList();
            return containersToDelete;
        }
    }
}
    



         