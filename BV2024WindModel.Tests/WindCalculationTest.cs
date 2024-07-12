using System.Collections.Immutable;
using System.Diagnostics;
using BV2024WindModel.Abstractions;
using BV2024WindModel.Data;
using BV2024WindModel.Logic;
using Newtonsoft.Json;

namespace BV2024WindModel.Tests
{
    [TestClass]
    public class WindCalculationTest
    {
        [TestMethod]
        //recalculated containers(aft) 500278(bilding), 500272(bilding+bay), 500780(bilding)
        //530280(bilding), 530780(bilding), 570780, 571282, 591372-80, 591472-80
        [DataRow(".\\TestData\\wind9.csv", ".\\TestData\\longitudinalWind9ReferenceResults.txt")]
        [DataRow(".\\TestData\\wind7.csv", ".\\TestData\\referenceLongitudinalWind7Results2B.txt")]
        public void LonitudinalCalculationCorrectnessTest(string inputFileName, string inputReferenceResultsFileName)
        {
            var allContainersFromFile = ReadCSV.ReadFromCsv(inputFileName);


            var vessel = GetVessel();

            //container on deck, which are below hatch cover will not be calculated
            var containersFromFile = allContainersFromFile.Where(container => IsOnDeck(container, vessel)).ToList();

            var input = new WindCalculatorInput { Containers = containersFromFile, Vessel = vessel };

            var longitudinalCalculator = new BV2024LongitudinalWindCalculator();
            var longitudinalWindExposedSurfaces = longitudinalCalculator.Calculate(input);

            var forcesCalculator = new WindForceCalculator();
            var externalParametrs = new WindForcesExternalCalculationParameters(0, 1.225, 15, 0.11);
            //{ Draft = 15, WindSpeed = 35, AirDencity = 1.225, WaterSurfaceRoughnessCoefficient = 0.11 };

            WindForcesCalculator.Calculate(forcesCalculator, externalParametrs, longitudinalWindExposedSurfaces.Fore);
            WindForcesCalculator.Calculate(forcesCalculator, externalParametrs, longitudinalWindExposedSurfaces.Aft);

            string referenceLongitudinalWindResultsSerialized = File.ReadAllText(inputReferenceResultsFileName);

            var longitudinalReferenceResults = (JsonConvert.DeserializeObject<LongitudinalSurfacesCalculationResult>(referenceLongitudinalWindResultsSerialized));


            Assert.IsNotNull(longitudinalWindExposedSurfaces);
            Assert.IsNotNull(longitudinalReferenceResults);
            Assert.AreEqual(longitudinalWindExposedSurfaces.Aft.Count, longitudinalReferenceResults.Aft.Count);
            Assert.AreEqual(longitudinalWindExposedSurfaces.Fore.Count, longitudinalReferenceResults.Fore.Count);


            CheckResults(longitudinalReferenceResults.Aft, longitudinalWindExposedSurfaces.Aft);
            CheckResults(longitudinalReferenceResults.Fore, longitudinalWindExposedSurfaces.Fore);
        }

        [TestMethod]
        [DataRow(".\\TestData\\wind9.csv",".\\TestData\\transverseWind9ReferenceResults.txt")]
        [DataRow(".\\TestData\\wind7.csv", ".\\TestData\\referenceTransverseWind7Results2B.txt")]
        public void TransverseCorrectnessTest(string inputFileName, string inputReferenceResultsFileName)
        {
            var allContainersFromFile = ReadCSV.ReadFromCsv(inputFileName);


            var vessel = GetVessel();

            //container on deck, which are below hatch cover will not be calculated
            var containersFromFile = allContainersFromFile.Where(container => IsOnDeck(container, vessel)).ToList();

            var input = new WindCalculatorInput { Containers = containersFromFile, Vessel = vessel };

            var transverseCalculator = new BV2024TransverseWindCalculator();
            var transverseWindExposedSurfaces = transverseCalculator.Calculate(input);

            var forcesCalculator = new WindForceCalculator();
            var externalParametrs = new WindForcesExternalCalculationParameters(0, 1.225, 15, 0.11);
            //{ Draft = 15, WindSpeed = 35, AirDencity = 1.225, WaterSurfaceRoughnessCoefficient = 0.11 };

            WindForcesCalculator.Calculate(forcesCalculator, externalParametrs, transverseWindExposedSurfaces.Portside);
            WindForcesCalculator.Calculate(forcesCalculator, externalParametrs, transverseWindExposedSurfaces.Starboard);

            string referenceTransverseWindResultsSerialized = File.ReadAllText(inputReferenceResultsFileName);

            var transverseReferenceResults = (JsonConvert.DeserializeObject<TransverseSurfacesCalculationResult>(referenceTransverseWindResultsSerialized));
             

            Assert.IsNotNull(transverseWindExposedSurfaces);
            Assert.IsNotNull(transverseReferenceResults);
            Assert.AreEqual(transverseWindExposedSurfaces.Portside.Count, transverseReferenceResults.Portside.Count);
            Assert.AreEqual(transverseWindExposedSurfaces.Starboard.Count, transverseReferenceResults.Starboard.Count);

            CheckResults(transverseReferenceResults.Portside, transverseWindExposedSurfaces.Portside);
            CheckResults(transverseReferenceResults.Starboard, transverseWindExposedSurfaces.Starboard);
        }

        private static void CheckResults(List<SurfaceCalculationResult> referenceWindResult, List<SurfaceCalculationResult> windResult)
        {
            for (var index = 0; index < referenceWindResult.Count; index++)
            {
                var referenceResult = referenceWindResult[index];
                var actualRecords = windResult.Where(entry => Math.Abs(entry.Coordinate - referenceResult.Coordinate) < 0.001);
                Assert.IsNotNull(actualRecords);
                foreach (var referenceContainer in referenceResult.Result)
                {
                    var actualContainer = actualRecords.SelectMany(actualRecord => actualRecord.Result).FirstOrDefault(c => c.ContainerId == referenceContainer.ContainerId);

                    Assert.IsNotNull(actualContainer);
                    var areaDifference = Math.Abs(actualContainer.ExposedArea - referenceContainer.ExposedArea);
                    Assert.IsTrue(areaDifference < 1e-6);
                    var forceDifference = Math.Abs(actualContainer.WindForceForArea - referenceContainer.WindForceForArea);
                    Assert.IsTrue(forceDifference < 1e-6);

                }

            }
        }

        [TestMethod]
        [DataRow(".\\TestData\\wind9.csv")]
        [DataRow(".\\TestData\\wind7.csv")]
        public void ReproducibilityTest(string inputFileName)
        {
            var allContainersFromFile = ReadCSV.ReadFromCsv(inputFileName);
            var numberOfResults = 60;

            var vessel = GetVessel();

            //container on deck, which are below hatch cover will not be calculated
            var containersFromFile = allContainersFromFile.Where(container => IsOnDeck(container, vessel)).ToList();

            var input = new WindCalculatorInput { Containers = containersFromFile, Vessel = vessel };

            LongitudinalSurfacesCalculationResult[] longitudinalWindExposedSurfaces = new LongitudinalSurfacesCalculationResult[numberOfResults];
            TransverseSurfacesCalculationResult[] transverseWindExposedSurfaces = new TransverseSurfacesCalculationResult[numberOfResults];

            for (int i = 0; i < numberOfResults; i++)
            {
                (longitudinalWindExposedSurfaces[i], transverseWindExposedSurfaces[i]) = Calculate(input);
            }
            for (int i = 0; i < numberOfResults - 1; i++)
            {
                var thisLongitudinal = longitudinalWindExposedSurfaces[i];
                var nextLongitudinal = longitudinalWindExposedSurfaces[i + 1];
                for (int j = 0; j < thisLongitudinal.Fore.Count; j++)
                {
                    var thisLongitudinalForeSurface = thisLongitudinal.Fore[j];
                    var nextLongitudinalForeSurface = nextLongitudinal.Fore[j];
                    var nextArea = AreaCalculator.GetWindArea(nextLongitudinalForeSurface.Result);
                    var thisArea = AreaCalculator.GetWindArea(thisLongitudinalForeSurface.Result);
                    if (Math.Abs(thisArea - nextArea) > 1e-6)
                    {

                    }
                    else

                    {

                    }
                }
                var thisTransverse = transverseWindExposedSurfaces[i];
                var nextTransverse = transverseWindExposedSurfaces[i + 1];
                for (int j = 0; j < thisLongitudinal.Fore.Count; j++)
                {
                    var thisTransversePortsideSurface = thisTransverse.Portside[j];
                    var nextTransversePortsideSurface = nextTransverse.Portside[j];
                    var thisArea = AreaCalculator.GetWindArea(thisTransversePortsideSurface.Result);
                    var nextArea = AreaCalculator.GetWindArea(nextTransversePortsideSurface.Result);
                    if (Math.Abs(thisArea - nextArea) > 1e-6)
                    {

                    }
                    else

                    {

                    }
                }
            }
        }

        private static bool IsOnDeck(Container container, Vessel vessel)
        {
            return container.Basis >= vessel.DeckHeight - 3;
        }

        private static Vessel GetVessel()
        {
            var buildings = new List<Building>();
            var building1 = new Building("1", 212.65, 0, 29, 14.3, 50, 33);
            buildings.Add(building1);
            var building2 = new Building("2", 56.5, 0, 29, 9, 50, 31);
            buildings.Add(building2);
            var vessel = new Vessel(30, buildings, 25);
            return vessel;
        }

        private static (LongitudinalSurfacesCalculationResult, TransverseSurfacesCalculationResult) Calculate( WindCalculatorInput input)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var longitudinalCalculator = new BV2024LongitudinalWindCalculator();
            var transverseCalculator = new BV2024TransverseWindCalculator();

            var longitudinalWindExposedSurfaces = longitudinalCalculator.Calculate(input);
            stopWatch.Stop();
            Console.WriteLine($"Longitudinal calculation time {stopWatch.ElapsedMilliseconds}ms");
            stopWatch.Start();
            var transverseWindExposedSurfaces = transverseCalculator.Calculate(input);
            stopWatch.Stop();
            Console.WriteLine($"Transverse calculation time {stopWatch.ElapsedMilliseconds}ms");
            return (longitudinalWindExposedSurfaces, transverseWindExposedSurfaces);
        }


        [TestMethod]
        [DataRow(".\\TestData\\wind9.csv", ".\\TestData\\wind9Add.csv", ".\\TestData\\longitudinalWind9ObserverReferenceResultsSmall.txt")]
        [DataRow(".\\TestData\\wind7.csv", ".\\TestData\\wind7Add2B.csv", ".\\TestData\\referenceLongitudinalWind7ObserverResults2B.txt")]
        //[DataRow(".\\TestData\\wind7.csv", ".\\TestData\\transverseWind7Results.txt")]
        public void WindObserverCorrectnessTest(string inputFileName, string changesFileName, string inputReferenceResultsFileName)
        {
            var containersFromFile = ReadCSV.ReadFromCsv(inputFileName);
            var vessel = GetVessel();

            //var buildings = new List<Building>();
            //var building = new Building("1", 212.65, 0, 29, 14.3, 50, 33);
            //buildings.Add(building);
            var newContainersFromFile = ReadCSV.ReadFromCsv(changesFileName);
            var longitudinalWindObserver = new LongitudinalWindObserver<string>(vessel);
            var longitudinalWindObserverResults = longitudinalWindObserver.DetectChangedAreasForAdding(containersFromFile.ToList(), newContainersFromFile.ToList());


            var shortResultCalculator = new ObserverResultsCalculator();
            var longitudinalWindObserverResultsShort = shortResultCalculator.Calculate(longitudinalWindObserverResults);
             
              
            string referenceLongitudinalWindObserverResultsSerialized = File.ReadAllText(inputReferenceResultsFileName);

            var referenceResults = (JsonConvert.DeserializeObject<List<ShortObserverResult>>(referenceLongitudinalWindObserverResultsSerialized));


            Assert.IsNotNull(longitudinalWindObserverResultsShort);
            Assert.IsNotNull(referenceResults);
            Assert.AreEqual(longitudinalWindObserverResultsShort[0].Criteria.Count, referenceResults[0].Criteria.Count);
            Assert.AreEqual(longitudinalWindObserverResultsShort[1].Criteria.Count, referenceResults[1].Criteria.Count);

            Check(referenceResults, longitudinalWindObserverResultsShort); 
        }

        private static void Check(List<ShortObserverResult> referenceWindResult, List<ShortObserverResult> windResult)
        {
            for (var index = 0; index < referenceWindResult.Count; index++)
            {
                var referenceResult = referenceWindResult[index];
                var actualResult = windResult[index];
                Assert.IsNotNull(actualResult);
                foreach (var referenceCriterium in referenceResult.Criteria)
                { 
                    var actualRecord = actualResult.Criteria.FirstOrDefault(criterium => Math.Abs(criterium.Coordinate - referenceCriterium.Coordinate) < 1e-6);
                    Assert.IsNotNull(actualRecord);
                    var portsideDifference = Math.Abs(referenceCriterium.Portside - actualRecord.Portside);
                    Assert.IsTrue(portsideDifference < 1e-6);
                    var starboardDifference = Math.Abs(referenceCriterium.Starboard - actualRecord.Starboard);
                    Assert.IsTrue(portsideDifference < 1e-6);
                }
                foreach (var referenceContainer in referenceResult.Containers)
                {
                    var actualRecord = actualResult.Containers.FirstOrDefault(containerId => containerId == referenceContainer);
                    Assert.IsNotNull(actualRecord);
                }
            }
        }

    }
}