using System.Collections.Generic;

namespace BV2024WindModel.Abstractions
{
    public class TransverseSurfaceCalculationResult
    {
        public List<ContainerCalculationResult> Result;
        public ContainerSide Side;
        public double Coordinate;
    }
}
