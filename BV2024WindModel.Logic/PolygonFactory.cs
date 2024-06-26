using System.Collections.Generic;
using System.Drawing;
using Macs3.Core.Mathematics.GeneralPolygonClipperLibrary;
using BV2024WindModel.Abstractions;

namespace BV2024WindModel
{
    public class PolygonFactory
    { 
        public static PolyDefault FromContainer(Container container)
        {
            return FromPoints(container.PointsYZ);
        }
        public static PolyDefault FromPoints( IEnumerable<PointF> points)
        {
            var polygon = new PolyDefault();
            foreach (var point in points)
            {
                polygon.add(point);
            }
            return polygon;
        }
    }
    


}
         