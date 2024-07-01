﻿using System.Collections.Generic;
using System.Drawing;
using Macs3.Core.Mathematics.GeneralPolygonClipperLibrary;
using BV2024WindModel.Abstractions;
using Clipper2Lib;

namespace BV2024WindModel
{
    public class PathsFactory
    {
        /*
        public static PolyDefault FromContainer(Container container)
        {
            return FromPoints(container.PointsYZ);
        */
        public static PathsD FromListOfPaths(IEnumerable<PathsD> listOfPaths)
        {
            var newPaths = new PathsD();
            foreach (var paths in listOfPaths)
            {
                foreach (var path in paths)
                { 
                    newPaths.Add(path);
                }
            }
            return newPaths;
        }
        public static PathD FromPoints( IEnumerable<PointD> points)
        {
            var path = new PathD();
            foreach (var point in points)
            {
                path.Add(point);
            }
            return path;
        }
    }
    


}
         