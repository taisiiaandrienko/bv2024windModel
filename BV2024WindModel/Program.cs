using System;
using BV2024WindModel.Logic;
using BV2024WindModel.Data;
using System.Text;
using System.Diagnostics;
using BV2024WindModel.Abstractions;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BV2024WindModel
{

    public class Windmodel
    {
        

static void Main(string[] args)
        {

            try
            {
                var fileNumber = 9;

                var containersFromFile = ReadCSV.ReadFromCsv($"C:\\windLoadFiles\\wind{fileNumber}.csv");

                var longitudinalCalculator = new BV2024LongitudinalWindCalculator();
                var transverseCalculator = new BV2024TransverseWindCalculator();
                var stopWatchTotal = new Stopwatch();
                stopWatchTotal.Start();
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var longitudinalWindExposedSurfaces = longitudinalCalculator.Calculate(containersFromFile);
                stopWatch.Stop();
                Console.WriteLine($"Longitudinal calculation time {stopWatch.ElapsedMilliseconds}ms");
                stopWatch.Restart();
                var transverseWindExposedSurfaces = transverseCalculator.Calculate(containersFromFile);
                stopWatch.Stop();
                Console.WriteLine($"Transverse calculation time {stopWatch.ElapsedMilliseconds}ms");

                stopWatch.Restart();

                var forcesCalculator = new WindForceCalculator();
                var externalParametrs = new WindForcesExternalCalculationParameters { Draft = 15, WindSpeed = 35, AirDencity = 1.225, WaterSurfaceRoughnessCoefficient = 0.11 };
                 
                WindForcesCalculator.Calculate(forcesCalculator, externalParametrs, longitudinalWindExposedSurfaces.Fore);
                WindForcesCalculator.Calculate(forcesCalculator, externalParametrs, longitudinalWindExposedSurfaces.Aft);
           
                WindForcesCalculator.Calculate(forcesCalculator, externalParametrs, transverseWindExposedSurfaces.Portside);
                WindForcesCalculator.Calculate(forcesCalculator, externalParametrs, transverseWindExposedSurfaces.Starboard);

                stopWatch.Stop();
                stopWatchTotal.Stop();
                Console.WriteLine($"Forces calculation time {stopWatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"Total calculation time {stopWatchTotal.ElapsedMilliseconds}ms");

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"ForeWind");
                foreach (var windExposedFrontSurface in longitudinalWindExposedSurfaces.Fore)
                {
                    var windArea = 0.0;
                    foreach (var containerResult in windExposedFrontSurface.Result)
                    {
                        windArea += containerResult.ExposedArea;
                        string polygon = PolygonPrinter.Print(containerResult.WindExposedPolygon);
                        //if (containerResult.Area == 0)
                        //continue;
                        if (containerResult.ExposedArea != 0.0)
                            sb.AppendLine($"Id= {containerResult.ContainerId}, Area= {containerResult.ExposedArea:f06}, Force= {containerResult.WindForceForArea:f06}, Points= {polygon}");
                    }
                    sb.AppendLine($"XFore= {windExposedFrontSurface.Coordinate:f03}, Area= {windArea:f06}");
                }
                /*
                sb.AppendLine($"________________________________________________________");
                sb.AppendLine($"AftWind");
                foreach (var windExposedAftSurface in longitudinalWindExposedSurfaces.Aft)
                {
                    var windArea = 0.0;
                    foreach (var containerResult in windExposedAftSurface.Result)
                    {
                        windArea += containerResult.Area;
                        string polygon = PolygonPrinter.Print(containerResult.WindExposedPolygon);
                        if (containerResult.Area != 0.0)
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
                        if (containerResult.Area != 0.0)
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
                        if (containerResult.Area != 0.0)
                            sb.AppendLine($"Id= {containerResult.ContainerId}, Area= {containerResult.Area:f06}, Points= {polygon}");
                    }
                    sb.AppendLine($"Ystar= {windExposedStarboardSurface.Coordinate:f03}, Area= {windArea:f06}");
                }
               */
                System.IO.File.WriteAllText($"C:\\windLoadFiles\\wind{fileNumber}ForcesResults.txt", sb.ToString());

                string longitudinalWindResultsSerialized = JsonConvert.SerializeObject(longitudinalWindExposedSurfaces, Formatting.Indented);
                string transverseWindResultsSerialized = JsonConvert.SerializeObject(transverseWindExposedSurfaces, Formatting.Indented);

                System.IO.File.WriteAllText($"C:\\windLoadFiles\\longitudinalWind{fileNumber}Results.txt", longitudinalWindResultsSerialized);
                System.IO.File.WriteAllText($"C:\\windLoadFiles\\transverseWind{fileNumber}Results.txt", transverseWindResultsSerialized);
                /*
                var windCalculationResults = WindCalculationResultFactory.Create(windExposedFrontSurfaces);

                

                //write string to file
                System.IO.File.WriteAllText(@"C:\windLoadFiles\wind9Results.txt", windResultsSerialized);

    */
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("exceptions.txt", ex.ToString());
            }

        }

        

    }


}
         