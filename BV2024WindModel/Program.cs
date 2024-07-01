using System;
using BV2024WindModel.Logic;
using BV2024WindModel.Data;

namespace BV2024WindModel
{

    public class Windmodel
    {

        static void Main(string[] args)
        {
           
            var containersFromFile = ReadCSV.ReadFromCsv("C:\\windLoadFiles\\wind9.csv");

            var longitudinalCalculator = new BV2024LongitudinalWindCalculator();
            var transverseCalculator = new BV2024TransverseWindCalculator();

            var longitudinalWindExposedSurfaces = longitudinalCalculator.Calculate(containersFromFile);


            foreach (var windExposedFrontSurface in longitudinalWindExposedSurfaces.Fore)
            {
                var windArea = 0.0;
                foreach (var containerResult in windExposedFrontSurface.Result)
                {
                    windArea += containerResult.Area;
                    //string polygon = PolygonPrinter.Print(containerResult.WindExposedPolygon);
                    //Console.WriteLine($"Id= {containerResult.ContainerId}, Area= {containerResult.Area:f06}, Points= {polygon}");
                }
                Console.WriteLine($"XFore= {windExposedFrontSurface.Coordinate:f02}, Area= {windArea:f06}");
            }
            foreach (var windExposedAftSurface in longitudinalWindExposedSurfaces.Aft)
            {
                var windArea = 0.0;
                foreach (var containerResult in windExposedAftSurface.Result)
                {
                    windArea += containerResult.Area;
                }
                Console.WriteLine($"XAft= {windExposedAftSurface.Coordinate:f02}, Area= {windArea:f06}");
            }


             


            var transverseWindExposedSurfaces = transverseCalculator.Calculate(containersFromFile);

            foreach (var windExposedPortsideSurface in transverseWindExposedSurfaces.Portside)
            {
                var windArea = 0.0;
                foreach (var containerResult in windExposedPortsideSurface.Result)
                {
                    windArea += containerResult.Area;
                }
                Console.WriteLine($"Yport= {windExposedPortsideSurface.Coordinate:f03}, Area= {windArea:f06}");
            }
            foreach (var windExposedStarboardSurface in transverseWindExposedSurfaces.Starboard)
            {
                var windArea = 0.0;
                foreach (var containerResult in windExposedStarboardSurface.Result)
                {
                    windArea += containerResult.Area;
                }
                Console.WriteLine($"Ystar= {windExposedStarboardSurface.Coordinate:f03}, Area= {windArea:f06}");
            }

            /*
            var windCalculationResults = WindCalculationResultFactory.Create(windExposedFrontSurfaces);
           
            string windResultsSerialized = JsonConvert.SerializeObject(windCalculationResults, Formatting.Indented);
            


            //write string to file
            System.IO.File.WriteAllText(@"C:\windLoadFiles\wind9Results.txt", windResultsSerialized);

            Console.WriteLine("Exposed ");
            foreach (var windExposedFrontSurface in windExposedFrontSurfaces)
            {
                Console.WriteLine($"X= {windExposedFrontSurface.Coordinate}, Area= {windExposedFrontSurface.Area:f06}");
            }*/

            /*Parallel.ForEach(aftSurfaces, aftSurface =>
             {
                 var protectingPolygonsAtCoordinate = inflator.InflateContainers(aftSurface.Polygons, 0.3);
                 aftInflatedProtectingSurfacesBag.Add(new Surface(aftSurface.Coordinate, protectingPolygonsAtCoordinate));
             });*/

            Console.Read();

        }

    }


}
         