using System.Collections.Generic;


namespace BV2024WindModel.Abstractions
{
    public class ContainersAtCoordinate
    {
        public double Coordinate { get; }
        public List<Container> Containers { get; }
        public ContainerEnd End { get; }
        public ContainersAtCoordinate(double coordinate, List<Container> containers, ContainerEnd end)
        {
            Coordinate = coordinate;
            Containers = containers;
            End = end;
        }

    }
    


}
         