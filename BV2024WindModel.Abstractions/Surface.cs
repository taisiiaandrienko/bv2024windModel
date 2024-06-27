using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Clipper2Lib;
using Macs3.Core.Mathematics.GeneralPolygonClipperLibrary;

namespace BV2024WindModel.Abstractions
{
    public class Surface
    {
        //public PolyDefault Polygon;
        public PathsD Paths;
        public double Coordinate;
        public double Area

        { get { return AreaCalculator.CalcArea(Paths) ; } }
        public Surface(double coordinate, List<PointD> points)
        {
            Coordinate = coordinate;
            var paths = new PathD();
            foreach (var point in points)
            {
                paths.Add(point);
            }
            Paths.Add(paths);
        }
        public Surface(double coordinate, PathD path)
        {
            Paths = new PathsD();
            Coordinate = coordinate;
            Paths.Add(path);
        }
        public Surface(double coordinate, PathsD paths)
        {
            Coordinate = coordinate;
            Paths = paths;
        }
        public Surface(double coordinate, List<PathD> paths)
        {
            Coordinate = coordinate;
            Paths = PathsFactory.FromListOfPath(paths);
        }
        public Surface(double coordinate, List<PathsD> paths)
        {
            Coordinate = coordinate;
            Paths = PathsFactory.FromListOfPaths(paths);
        }
    }

}
