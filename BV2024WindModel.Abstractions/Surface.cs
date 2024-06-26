using System.Collections.Generic;
using System.Drawing;
using Macs3.Core.Mathematics.GeneralPolygonClipperLibrary;

namespace BV2024WindModel.Abstractions
{
    public class Surface
    {
        public PolyDefault Polygon;
        public double Coordinate;
        public double Area

        { get { return Polygon.Area; } }
        public Surface(double coordinate, List<PointF> points)
        {
            Coordinate = coordinate;
            Polygon = new PolyDefault();
            foreach (var point in points)
            {
                Polygon.add(point);
            }
        }
        public Surface(double coordinate, PolyDefault polygon)
        {
            Coordinate = coordinate;
            Polygon = new PolyDefault(polygon);
        }
        public Surface(double coordinate, List<PolyDefault> polygons)
        {
            Coordinate = coordinate;
            Polygon = new PolyDefault();
            foreach(var polygon in polygons)
            {
                Polygon = Polygon.union(polygon) as PolyDefault;
                
            }
        }
    }

}
