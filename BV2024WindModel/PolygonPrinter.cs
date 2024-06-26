using System;
using System.Collections.Generic;
using System.Drawing;
using Macs3.Core.Mathematics.GeneralPolygonClipperLibrary;

namespace BV2024WindModel
{
    public class PolygonPrinter
    {
        public static void Print(IEnumerable<PointF> points)
        {
            foreach (var point in points)
            {
                Console.WriteLine($"({point.X:f03}; {point.Y:f03})");
            }
        }
        public static void Print(PolyDefault polygon)
        {
            foreach (var point in polygon.Points)
            {
                Console.WriteLine($"({point.X:f03}; {point.Y:f03})");
            }
        }
    }
    


}
