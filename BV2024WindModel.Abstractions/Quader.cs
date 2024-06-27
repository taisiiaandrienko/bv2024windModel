using System;
using Clipper2Lib;

namespace BV2024WindModel.Abstractions
{
    public class Quader
    {
        private double lcg;
        private double tcg;
        private double basis;
        private double length;
        private double width;
        private double height;
        public Surface AftSurface;
        public Surface FrontSurface;
        public Surface PortsideSurface;
        public Surface StarboardSurface;
        public PathD PointsXZ;
        public PathD PointsYZ;
        public Quader(double lcg, double tcg, double basis, double length, double width, double height)
        {
            this.lcg = lcg;
            this.tcg = tcg;
            this.basis = basis;
            this.length = length;
            this.width = width;
            this.height = height;
            double[] xCoord = { lcg + length / 2, lcg - length / 2 };
            double[] yCoord = { tcg + width / 2, tcg - width / 2 };
            double[] zCoord = { basis + height, basis };

            PointsYZ = new PathD
            {
                new PointD(yCoord[0],zCoord[0]),
                new PointD(yCoord[1],zCoord[0]),
                new PointD(yCoord[1],zCoord[1]),
                new PointD(yCoord[0],zCoord[1])
            };
            AftSurface = new Surface(Math.Round(lcg - length / 2, 2), PointsYZ);
            FrontSurface = new Surface(Math.Round(lcg + length / 2, 2), PointsYZ);

        }

    }
}
