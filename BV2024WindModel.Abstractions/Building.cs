using System.Collections.Generic;
using System.Drawing;
//using Clipper2Lib;

namespace BV2024WindModel.Abstractions
{
    public class Building : Quader
    {


        /*public Surface AftSurface;
        public Surface FrontSurface;
        public List<PointF> PointsYZ;
        public Building()
        {
            PointsYZ = new List<PointF>
            {
                new PointF( 25, 62),
                new PointF( -25, 62),
                new PointF( -25, 29),
                new PointF( 25, 29)
            };
            AftSurface = new Surface(205.5, PointsYZ);
            FrontSurface = new Surface(219.8, PointsYZ);*/
        public Building(double lcg, double tcg, double basis, double length, double width, double height) : base(lcg, tcg, basis, length, width, height)
        {
        }
    }
}
    



         