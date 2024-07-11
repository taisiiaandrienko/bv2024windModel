﻿using System;
using System.Linq;
using BV2024WindModel.Abstractions;
using Clipper2Lib;

namespace BV2024WindModel
{
    public class PolygonDeflator
    {
        public static PathsD DeflatePolygon(Surface protectingSurface, double protectedSurfaceCoordinate, double angle)
        {
            var containersDist = Math.Abs(protectingSurface.Coordinate - protectedSurfaceCoordinate); 
            var tg = Math.Tan(angle * (Math.PI / 180));
            var offset = -tg * containersDist;

            var pathsToDeflate = new PathsD();
            foreach (var path in protectingSurface.Paths)
            {
                var deckHeight = path.Min(point => point.y);
                var pathToDeflate = new PathD();
                foreach (var point in path)
                {
                    Grounding(deckHeight, pathToDeflate, point);
                }
                pathsToDeflate.Add(pathToDeflate);
            }
            var allDeflatedPaths = Clipper.InflatePaths(pathsToDeflate, offset, JoinType.Miter, EndType.Polygon, 4.0, 8);

            if (allDeflatedPaths.Count > 0)
            {
                return allDeflatedPaths;
            }
            return null;
        } 
        private static void Grounding(double deckHeight, PathD pathToDeflate, PointD point)
        {
            if (point.y < deckHeight + 0.01)
            {
                pathToDeflate.Add(new PointD(point.x, -1000));
            }
            else
            {
                pathToDeflate.Add(new PointD(point.x, point.y));
            }
        }
    }
}
   