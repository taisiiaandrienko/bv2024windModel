using System;
using BV2024WindModel.Logic;
using BV2024WindModel.Data;
using System.Text;
using System.Diagnostics;
using BV2024WindModel.Abstractions;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace BV2024WindModel
{

    public class Windmodel
    {


        static void Main(string[] args)
        {

            try
            {
                var fileNumber =7;
                var buildings = new List<Building>();
                var building1 = new Building("1", 212.65, 0, 29, 14.3, 50, 33);
                buildings.Add(building1);
                var building2 = new Building("2", 56.5, 0, 29, 9, 50, 31);
                buildings.Add(building2);
                var vessel = new Vessel(30, buildings, 25);


                var allContainersFromFile = ReadCSV.ReadFromCsv($"C:\\windLoadFiles\\wind{fileNumber}.csv");
                //container on deck, which are below hatch cover will not be calculated
                var containersFromFile = allContainersFromFile.Where(container => container.Basis >= vessel.DeckHeight).ToList();

                var input = new WindCalculatorInput { Containers = containersFromFile, Vessel = vessel };

                var longitudinalCalculator = new BV2024LongitudinalWindCalculator();
                var transverseCalculator = new BV2024TransverseWindCalculator();
                var stopWatchTotal = new Stopwatch();
                stopWatchTotal.Start();
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var longitudinalWindExposedSurfaces = longitudinalCalculator.Calculate(input);
                stopWatch.Stop();
                Console.WriteLine($"Longitudinal calculation time {stopWatch.ElapsedMilliseconds}ms");
                stopWatch.Restart();
                var transverseWindExposedSurfaces = transverseCalculator.Calculate(input);
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
                Console.WriteLine($"Forces calculation time {stopWatch.ElapsedMilliseconds}ms");

                stopWatch.Restart();

                var fileNumber1 = 9;
                var newContainersFromFile = ReadCSV.ReadFromCsv($"C:\\windLoadFiles\\wind{fileNumber1}Add2B.csv");
                var longitudinalWindObserver = new LongitudinalWindObserver<string>(vessel);
                var longitudinalWindObserverResults = longitudinalWindObserver.DetectChangedAreasForAdding(containersFromFile.ToList(), newContainersFromFile.ToList());


                var shortResultCalculator = new ObserverResultsCalculator();
                var longitudinalWindObserverResultsShort = shortResultCalculator.Calculate(longitudinalWindObserverResults);

                stopWatch.Stop();
                Console.WriteLine($"Added containers calculation time {stopWatch.ElapsedMilliseconds}ms");
                stopWatchTotal.Stop();
                Console.WriteLine($"Total calculation time {stopWatchTotal.ElapsedMilliseconds}ms");

                 

                string longitudinalWindResultsSerialized = JsonConvert.SerializeObject(longitudinalWindExposedSurfaces, Formatting.Indented);
                string transverseWindResultsSerialized = JsonConvert.SerializeObject(transverseWindExposedSurfaces, Formatting.Indented);

                System.IO.File.WriteAllText($"C:\\windLoadFiles\\longitudinalWind{fileNumber}Results2B.txt", longitudinalWindResultsSerialized);
                System.IO.File.WriteAllText($"C:\\windLoadFiles\\transverseWind{fileNumber}Results2B.txt", transverseWindResultsSerialized);
                 
                string longitudinalWindObserverResultsSerialized = JsonConvert.SerializeObject(longitudinalWindObserverResultsShort, Formatting.Indented);
                 
                System.IO.File.WriteAllText($"C:\\windLoadFiles\\longitudinalWind{fileNumber}ObserverResults2B.txt", longitudinalWindObserverResultsSerialized);

                PrintAllResults(fileNumber, longitudinalWindExposedSurfaces, transverseWindExposedSurfaces);

                Console.ReadLine();
            }
            catch (Exception ex)
             {
                System.IO.File.WriteAllText("exceptions.txt", ex.ToString());
            }

        }

        private static void PrintAllResults(int fileNumber, LongitudinalSurfacesCalculationResult longitudinalWindExposedSurfaces, TransverseSurfacesCalculationResult transverseWindExposedSurfaces)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"ForeWind");

            PrintResult(sb, longitudinalWindExposedSurfaces.Fore, "XFore");

            sb.AppendLine($"________________________________________________________");
            sb.AppendLine($"AftWind");
            sb.AppendLine($"________________________________________________________");
            PrintResult(sb, longitudinalWindExposedSurfaces.Aft, "XAft");


            sb.AppendLine($"________________________________________________________");
            sb.AppendLine($"PortsideWind");
            sb.AppendLine($"________________________________________________________");
            PrintResult(sb, transverseWindExposedSurfaces.Portside, "YPort");


            sb.AppendLine($"________________________________________________________");
            sb.AppendLine($"StarboardWind");
            sb.AppendLine($"________________________________________________________");
            PrintResult(sb, transverseWindExposedSurfaces.Starboard, "YStar");


            System.IO.File.WriteAllText($"C:\\windLoadFiles\\wind{fileNumber}ForcesResults2B.txt", sb.ToString());
        }

        private static void PrintResult(StringBuilder sb, List<SurfaceCalculationResult> surfaceCalculationResult, string coord)
        {
            foreach (var windExposedFrontSurface in surfaceCalculationResult)
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
                sb.AppendLine(coord + $"= {windExposedFrontSurface.Coordinate:f03}, Area= {windArea:f06}");
            }
        }


    }


}
/*
      
      */