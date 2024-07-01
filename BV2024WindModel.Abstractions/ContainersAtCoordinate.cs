using System.Collections.Generic;


namespace BV2024WindModel.Abstractions
{
    public class ContainersAtCoordinate
    {
        public double Coordinate;
        public List<Container> Containers;
        public ContainerEnd End; 
        public ContainersAtCoordinate(double coordinate, List<Container> containers, ContainerEnd end)
        {
            Coordinate = coordinate;
            Containers = containers;
            End = end;
        }

    }
    


}
         