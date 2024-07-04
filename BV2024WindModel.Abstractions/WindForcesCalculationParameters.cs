namespace BV2024WindModel.Abstractions
{
    public class WindForcesCalculationParameters
    { 
        public double VolumetricCenter; 
        public double Area;
        public WindForcesExternalCalculationParameters ExternalParameters;
        public WindForcesCalculationParameters(WindForcesExternalCalculationParameters externalParametrs, double area, double volumetricCenter)
        {  
            Area = area;
            VolumetricCenter = volumetricCenter;
            ExternalParameters = externalParametrs;
        }
    }
    public class WindForcesExternalCalculationParameters
    {
        public double WindSpeed;
        public double AirDencity;
        public double Draft; 
        public double WaterSurfaceRoughnessCoefficient; 
        /*public WindForcesCalculationParameters( )
        {
            WindSpeed = 35;
            AirDencity = 1.255; 
            WaterSurfaceRoughnessCoefficient = 0.11; 
        }*/
    }

}
