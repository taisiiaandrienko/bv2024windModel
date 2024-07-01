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

            var calculator = new BV2024WindCalculator();

            var longitudeWindExposedSurfaces = calculator.Calculate(containersFromFile);

            foreach (var windExposedFrontSurface in longitudeWindExposedSurfaces.Fore)
            {
                var windArea = 0.0; 
                foreach (var containerResult in windExposedFrontSurface.Result)
                {
                    windArea += containerResult.Area;
                //string polygon = PolygonPrinter.Print(containerResult.WindExposedPolygon);
                //Console.WriteLine($"Id= {containerResult.ContainerId}, Area= {containerResult.Area:f06}, Points= {polygon}");
                } 
                Console.WriteLine($"XFore= {windExposedFrontSurface.Coordinate}, Area= {windArea:f06}");
            }
            foreach (var windExposedFrontSurface in longitudeWindExposedSurfaces.Aft)
            {
                var windArea = 0.0;
                foreach (var containerResult in windExposedFrontSurface.Result)
                {
                    windArea += containerResult.Area;
                }
                Console.WriteLine($"XAft= {windExposedFrontSurface.Coordinate}, Area= {windArea:f06}");
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
         