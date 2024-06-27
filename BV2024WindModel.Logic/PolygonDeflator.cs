using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using BV2024WindModel.Abstractions;
using Clipper2Lib;
using Macs3.Core.Mathematics.GeneralPolygonClipperLibrary;

namespace BV2024WindModel
{
    public class PolygonDeflator
    {
        public static PathsD DeflatePolygon(Surface protectingSurface, double protectedSurfaceCoordinate, double angle)
        {
            var containersDist = Math.Abs(protectingSurface.Coordinate - protectedSurfaceCoordinate);
            //var deckHeight = protectingSurface.Paths.Min(path => path.Min(point => point.y));
            var tg = Math.Tan(angle * (Math.PI / 180));
            var offset = -tg * containersDist;

            var pathsToDeflate = new PathsD();
            foreach (var path in protectingSurface.Paths)
            {
                var deckHeight = path.Min(point => point.y);
                var pathToDeflate = new PathD();
                foreach (var point in path)
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
                pathsToDeflate.Add(pathToDeflate);
            }
            var allDeflatedPaths = Clipper.InflatePaths(pathsToDeflate, offset, JoinType.Miter, EndType.Polygon, 2.0, 3);

            if (allDeflatedPaths.Count > 0)
            {
                return allDeflatedPaths;
            }
            return null;
        }
    }
}
    /*
    public class PolygonInflator
    {
        public List<PolyDefault> InflateContainers(PathD paths, double offset)
        {
            offset /= 2;
            
            offset = offset / 2 * 1000;
            var inflatedPolygons = new List<PolyDefault>();

            foreach (var polygon in polygons)
            {
                for (var polygonIndex = 0; polygonIndex < polygon.NumInnerPoly; polygonIndex++)
                {
                    var innerPoly = polygon.getInnerPoly(polygonIndex);
                    var path = new Path64();
                    foreach (var point in innerPoly.Points)
                    {
                        path.Add(new Point64(point.X * 1000, point.Y * 1000));
                    }
                    var inflatedPaths = Clipper.InflatePaths(new Paths64 { path }, offset, JoinType.Miter, EndType.Polygon);

                    foreach (var inflatedPath in inflatedPaths)
                    {
                        var inflatedPolygon = new PolyDefault();
                        foreach (var point in inflatedPath)
                        {
                            inflatedPolygon.add(point.X / 1000.0, point.Y / 1000.0);
                        }
                        inflatedPolygons.Add(inflatedPolygon);
                    }
                }
            }

            return inflatedPolygons;
        }

    }
    */


