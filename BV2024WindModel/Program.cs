using System;
using BV2024WindModel.Logic;
using BV2024WindModel.Data;
using System.Text;
using System.Diagnostics;

namespace BV2024WindModel
{

    public class Windmodel
    {

        static void Main(string[] args)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var containersFromFile = ReadCSV.ReadFromCsv("C:\\windLoadFiles\\wind9.csv");

            var longitudinalCalculator = new BV2024LongitudinalWindCalculator();
            var transverseCalculator = new BV2024TransverseWindCalculator();

            var longitudinalWindExposedSurfaces = longitudinalCalculator.Calculate(containersFromFile);
            stopWatch.Stop();
            Console.WriteLine($"Longitudinal calculation time {stopWatch.ElapsedMilliseconds}ms");
            stopWatch.Start();
            var transverseWindExposedSurfaces = transverseCalculator.Calculate(containersFromFile);
            stopWatch.Stop();
            Console.WriteLine($"Transverse calculation time {stopWatch.ElapsedMilliseconds}ms");



            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"ForeWind");
            foreach (var windExposedFrontSurface in longitudinalWindExposedSurfaces.Fore)
            {
                var windArea = 0.0;
                foreach (var containerResult in windExposedFrontSurface.Result)
                {
                    windArea += containerResult.Area;
                    string polygon = PolygonPrinter.Print(containerResult.WindExposedPolygon);
                    sb.AppendLine($"Id= {containerResult.ContainerId}, Area= {containerResult.Area:f06}, Points= {polygon}");
                }
                sb.AppendLine($"XFore= {windExposedFrontSurface.Coordinate:f03}, Area= {windArea:f06}");
            }
            sb.AppendLine($"________________________________________________________");
            sb.AppendLine($"AftWind");
            foreach (var windExposedAftSurface in longitudinalWindExposedSurfaces.Aft)
            {
                var windArea = 0.0;
                foreach (var containerResult in windExposedAftSurface.Result)
                {
                    windArea += containerResult.Area;
                    string polygon = PolygonPrinter.Print(containerResult.WindExposedPolygon);
                    sb.AppendLine($"Id= {containerResult.ContainerId}, Area= {containerResult.Area:f06}, Points= {polygon}");
                }
                sb.AppendLine($"XAft= {windExposedAftSurface.Coordinate:f03}, Area= {windArea:f06}");
            }

            

            sb.AppendLine($"________________________________________________________");
            sb.AppendLine($"PortsideWind");
            foreach (var windExposedPortsideSurface in transverseWindExposedSurfaces.Portside)
            {
                var windArea = 0.0;
                foreach (var containerResult in windExposedPortsideSurface.Result)
                {
                    windArea += containerResult.Area;
                    string polygon = PolygonPrinter.Print(containerResult.WindExposedPolygon);
                    sb.AppendLine($"Id= {containerResult.ContainerId}, Area= {containerResult.Area:f06}, Points= {polygon}");
                }
                sb.AppendLine($"Yport= {windExposedPortsideSurface.Coordinate:f03}, Area= {windArea:f06}");
            }
            sb.AppendLine($"________________________________________________________");
            sb.AppendLine($"StarboardWind");
            foreach (var windExposedStarboardSurface in transverseWindExposedSurfaces.Starboard)
            {
                var windArea = 0.0;
                foreach (var containerResult in windExposedStarboardSurface.Result)
                {
                    windArea += containerResult.Area;
                    string polygon = PolygonPrinter.Print(containerResult.WindExposedPolygon);
                    sb.AppendLine($"Id= {containerResult.ContainerId}, Area= {containerResult.Area:f06}, Points= {polygon}");
                }
                sb.AppendLine($"Ystar= {windExposedStarboardSurface.Coordinate:f03}, Area= {windArea:f06}");
            }
            System.IO.File.WriteAllText(@"C:\windLoadFiles\wind9Results.txt", sb.ToString());
            /*
            var windCalculationResults = WindCalculationResultFactory.Create(windExposedFrontSurfaces);
           
            string windResultsSerialized = JsonConvert.SerializeObject(windCalculationResults, Formatting.Indented);
              
            //write string to file
            System.IO.File.WriteAllText(@"C:\windLoadFiles\wind9Results.txt", windResultsSerialized);
*/
            
            Console.Read();

        }

    }


}
         