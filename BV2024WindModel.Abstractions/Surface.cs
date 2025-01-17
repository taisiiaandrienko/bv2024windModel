﻿using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;

namespace BV2024WindModel.Abstractions
{
    public class Surface
    { 
        public PathsD Paths { get; }
        public double Coordinate { get; }
        public Bounds Bounds { get; }

        public Surface(double coordinate, PathsD paths)
        {
            Coordinate = coordinate;
            Paths = paths;

            var minx = paths.SelectMany(path => path).Min(point => point.x);
            var maxx = paths.SelectMany(path => path).Max(point => point.x);
            var miny = paths.SelectMany(path => path).Min(point => point.y);
            var maxy = paths.SelectMany(path => path).Max(point => point.y);
            Bounds = new Bounds { MinX = minx, MaxX = maxx, MinY = miny, MaxY = maxy };

        }
        
    }

}
