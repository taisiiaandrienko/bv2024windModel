using Clipper2Lib;

namespace BV2024WindModel
{
    public class PolygonPrinter
    {
        public static string PrintPath(PathD path)
        {
            string presentation = string.Empty;
            foreach (var point in path)
            {
                presentation += ($"({point.x:f03}; {point.y:f03})");
            }
            return presentation;
        }
        public static string Print(PathsD polygon)
        {
            string presentation = string.Empty;
            foreach (var path in polygon)
            {
                presentation += PrintPath(path);
            }
            return presentation;
        }
    }
    


}
