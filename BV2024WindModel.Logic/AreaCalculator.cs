using BV2024WindModel.Abstractions;
using System.Collections.Generic;
using Clipper2Lib;


namespace BV2024WindModel.Logic
{
    public static class AreaCalculator 
    {
        public static double CalcArea(PathsD paths)
        {
            double totalArea = 0;
            for (int i = 0; i < paths.Count; i++)
                totalArea += Clipper.Area(paths[i]);
            return totalArea;
        }
        public static double GetWindArea(List<ContainerCalculationResult> results)
        {
            var windArea = 0.0;
            foreach (var containerResult in results)
            {
                windArea += containerResult.ExposedArea;

            }
            return windArea;
        }
    }
}

