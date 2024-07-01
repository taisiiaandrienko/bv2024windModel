using System.Collections.Generic;

namespace BV2024WindModel.Abstractions
{
    public class LongitudinalSurfaceCalculationResult
    {
        public List<ContainerCalculationResult> Result;
        public ContainerEnd End;
        public double Coordinate;
    }
}
