namespace BV2024WindModel.Abstractions
{
    public class WindForcesCalculationParameters
    { 
        public double VolumetricCenter { get; }
        public double Area { get; }
        public WindForcesExternalCalculationParameters ExternalParameters { get; }
        public WindForcesCalculationParameters(WindForcesExternalCalculationParameters externalParametrs, double area, double volumetricCenter)
        {  
            Area = area;
            VolumetricCenter = volumetricCenter;
            ExternalParameters = externalParametrs;
        }
    }

}
