using System;
using System.Collections.Generic;
using System.Linq;

namespace BV2024WindModel.Abstractions
{

    public class WindObserver<T>
    {
        private double DeckHeight;
        private List<Building> Buildings;
        public WindObserver(double deckHeight, List<Building> buildings) 
        { 
            DeckHeight = deckHeight;
            Buildings = buildings;
        }
        public List<Container> DetectChangedAreasForAdding(List<Container> allContainers, List<Container> newContainers, double alpha)
        {
            var aftSurfaces = newContainers.GroupBy(container => container.AftSurface.Coordinate, container => container,
                (key, g) => new ContainersAtCoordinate(key, g.ToList(), ContainerEnd.Aft)).ToList();
            var foreSurfaces = newContainers.GroupBy(container => container.ForeSurface.Coordinate, container => container,
                (key, g) => new ContainersAtCoordinate(key, g.ToList(), ContainerEnd.Fore)).ToList();
            var tg = Math.Tan(alpha * (Math.PI / 180));

            var allcontainersToRecalculate = new List<Container>();
            GetContainersToRecalculate(allContainers, aftSurfaces, tg, allcontainersToRecalculate, ContainerEnd.Fore);
            GetContainersToRecalculate(allContainers, foreSurfaces, tg, allcontainersToRecalculate, ContainerEnd.Aft);

            return new List<Container>();
        }

        private void GetContainersToRecalculate(List<Container> allContainers, List<ContainersAtCoordinate> aftSurfaces, double tg, List<Container> allcontainersToRecalculate, ContainerEnd containerEnd)
        {
            foreach (var aftSurface in aftSurfaces)
            {
                double buildingMinY = 1000;
                double buildingMaxY = -1000;
                double buildingMaxZ = 0;
                foreach (var building in Buildings)
                {
                    var diff = containerEnd == ContainerEnd.Fore ? aftSurface.Coordinate - building.AftSurface.Coordinate : building.ForeSurface.Coordinate - aftSurface.Coordinate ;
                    if (diff < 0)
                    {
                        var dist =  Math.Abs(aftSurface.Coordinate - building.ForeSurface.Coordinate);
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
                var portsideOfProtectedArea = aftSurface.Containers.Min(container => container.TransverseBounds.MinX);
                var starboardOfProtectedArea = aftSurface.Containers.Max(container => container.TransverseBounds.MaxX);
                var topOfprotectedArea = aftSurface.Containers.Max(container => container.TransverseBounds.MaxY);
                var bottomOfProtectedArea = aftSurface.Containers.Min(container => container.TransverseBounds.MinY);
                if (portsideOfProtectedArea >= buildingMinY && starboardOfProtectedArea <= buildingMaxY && topOfprotectedArea <= buildingMaxZ)
                {
                }
                else
                {  
                    var containersToRecalculate = new List<Container>();
                    if (containerEnd == ContainerEnd.Fore)
                    {
                        containersToRecalculate = allContainers.Where(container => CanBeProtected(container, tg, container.ForeSurface.Coordinate - aftSurface.Coordinate, portsideOfProtectedArea, starboardOfProtectedArea, topOfprotectedArea, bottomOfProtectedArea)).ToList();
                    }
                    else
                    {
                        containersToRecalculate = allContainers.Where(container => CanBeProtected(container, tg, aftSurface.Coordinate - container.AftSurface.Coordinate, portsideOfProtectedArea, starboardOfProtectedArea, topOfprotectedArea, bottomOfProtectedArea)).ToList();

                    }
                    allcontainersToRecalculate.AddRange(containersToRecalculate);
                }

            }
        }

        private static bool CanBeProtected(Container container, double tg, double diff, double portside, double starboard, double maxZ, double minZ)
        { 
            var dist = Math.Abs(diff);
            var portsideOfProtectedArea = portside - dist * tg;
            var starboardOfProtectedArea = starboard + dist * tg;
            var topOfProtectesArea = maxZ - dist * tg;
            var bottomOfProtectedArea = minZ - dist * tg;
            return diff < 0 &&
                   dist < (starboard - portside) / (2 * tg) &&
                   dist < maxZ / tg &&
                   container.TransverseBounds.MinY < topOfProtectesArea && 
                   container.TransverseBounds.MaxY > bottomOfProtectedArea && 
                   ( container.TransverseBounds.MaxX > portsideOfProtectedArea  ||
                   container.TransverseBounds.MinX < starboardOfProtectedArea );         
        }

        public List<Container> DetectChangedAreasForDelition(List<Container> allContainers, List<T> ids )
        {

            var containersToDelete = allContainers.Where(container => ids.Contains((T)(object) container.id));

            return new List<Container>();
        }
    }
}
    



         