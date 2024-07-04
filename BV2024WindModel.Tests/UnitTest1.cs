using System.Diagnostics;
using BV2024WindModel.Abstractions;
using BV2024WindModel.Data;
using BV2024WindModel.Logic;
using Newtonsoft.Json;

namespace BV2024WindModel.Tests
{
    [TestClass]
    public class BV2024UnitTest1
    {
        [TestMethod]
        [DataRow("C:\\windLoadFiles\\wind9.csv", "C:\\windLoadFiles\\wind9ReferenceResults1.txt")]
        [DataRow("C:\\windLoadFiles\\wind7.csv", "C:\\windLoadFiles\\wind7ReferenceResults1.txt")]
        public void TestMethod1(string inputFileName, string inputReferenceResultsFileName)
        {
            var containersFromFile = ReadCSV.ReadFromCsv(inputFileName);

            var calculator = new BV2024LongitudinalWindCalculator();

            var windExposedFrontSurfaces = calculator.Calculate(containersFromFile);

            var windCalculationResults = WindCalculationResultFactory.Create(windExposedFrontSurfaces.Fore).OrderBy(entry => entry.Coordinate).ToList();

            string referenceWindResultsSerialized = File.ReadAllText(inputReferenceResultsFileName);

            var referenceResults = (JsonConvert.DeserializeObject<List<WindCalculationResult>>(referenceWindResultsSerialized)).OrderBy(entry => entry.Coordinate).ToList();

            Assert.IsNotNull(referenceResults);
            Assert.IsNotNull(windCalculationResults);
            Assert.AreEqual(windCalculationResults.Count, referenceResults.Count);
            //foreach (var referenceResult in referenceResults)
            for ( var index=0; index < referenceResults.Count; index++)
            {
                var referenceResult = referenceResults[index];
                var actualRecord = windCalculationResults.FirstOrDefault(entry => Math.Abs(entry.Coordinate - referenceResult.Coordinate) < 0.001);
                Assert.IsNotNull(actualRecord);
                var difference = Math.Abs(actualRecord.Area - referenceResult.Area);
                Assert.IsTrue(difference < 1e-0, $"For result with index {index} difference is {difference}");
            }

        }
        [TestMethod]
        [DataRow("C:\\windLoadFiles\\wind9.csv")]
        [DataRow("C:\\windLoadFiles\\wind7.csv")]
        public void ReproducibilityTest(string inputFileName)
        {
            var containersFromFile = ReadCSV.ReadFromCsv(inputFileName);
            var numberOfResults = 60;

            LongitudinalSurfacesCalculationResult[] longitudinalWindExposedSurfaces = new LongitudinalSurfacesCalculationResult[numberOfResults];
            TransverseSurfacesCalculationResult[] transverseWindExposedSurfaces = new TransverseSurfacesCalculationResult[numberOfResults];

            for (int i = 0; i < numberOfResults; i++)
            {
                (longitudinalWindExposedSurfaces[i], transverseWindExposedSurfaces[i]) = Calculate(containersFromFile);
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
        private static (LongitudinalSurfacesCalculationResult, TransverseSurfacesCalculationResult) Calculate(System.Collections.Generic.IEnumerable<Abstractions.Container> containersFromFile)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var longitudinalCalculator = new BV2024LongitudinalWindCalculator();
            var transverseCalculator = new BV2024TransverseWindCalculator();

            var longitudinalWindExposedSurfaces = longitudinalCalculator.Calculate(containersFromFile);
            stopWatch.Stop();
            Console.WriteLine($"Longitudinal calculation time {stopWatch.ElapsedMilliseconds}ms");
            stopWatch.Start();
            var transverseWindExposedSurfaces = transverseCalculator.Calculate(containersFromFile);
            stopWatch.Stop();
            Console.WriteLine($"Transverse calculation time {stopWatch.ElapsedMilliseconds}ms");
            return (longitudinalWindExposedSurfaces, transverseWindExposedSurfaces);
        }

    }
}