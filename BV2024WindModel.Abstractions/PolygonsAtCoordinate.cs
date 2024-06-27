using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using Macs3.Core.Mathematics.GeneralPolygonClipperLibrary;


namespace BV2024WindModel
{
    public class PolygonsAtCoordinate
    {
        public double Coordinate;
        public PathsD Paths;
        public PolygonsAtCoordinate(double coordinate, List<PathsD> listOfPaths)
        {
            Coordinate = coordinate;
            Paths = PathsFactory.FromListOfPaths(listOfPaths); 
        }
        public PolygonsAtCoordinate(double coordinate, List<PathD> paths)
        {
            Coordinate = coordinate;
            Paths = PathsFactory.FromListOfPath(paths);
        }
    }
    


}
         