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
        public PolygonsAtCoordinate(double coordinate, List<PathsD> paths)
        {
            Coordinate = coordinate;
            Paths = new PathsD();
            foreach (var path in paths)
            {
                Paths.Union(path);
            }
        }
        public PolygonsAtCoordinate(double coordinate, List<PathD> paths)
        {
            Coordinate = coordinate;
            Paths = new PathsD();
            foreach (var path in paths)
            {
                Paths.Add(path);
            }
        }
    }
    


}
         