namespace BV2024WindModel.Abstractions
{
    public class WindForcesExternalCalculationParameters
    {
        public static readonly double[] WindSpeedRoute =
      {
            // LASHING, NR 467, Pt F, Ch 12, Sec 5, 4.5.1 
            35,
            // LASHING, NR 467, Pt F, Ch 12, Sec 5, 4.6.1, Table 12
            30, 31, 34, 29, 30, 28, 27, 29, 29, 34, 33, 29, 26, 32, 35, 33, 32, 30, 29 

        };
        public double WindSpeed;
        public double AirDencity;
        public double Draft; 
        public double WaterSurfaceRoughnessCoefficient;

        public WindForcesExternalCalculationParameters(int route, double airDencity, double draft, double waterSurfaceRoughnessCoefficient, double? windSpeed = null)
        { 
            WindSpeed = windSpeed.HasValue ? windSpeed.Value: WindSpeedRoute[route];
            AirDencity = airDencity;
            Draft = draft;
            WaterSurfaceRoughnessCoefficient = waterSurfaceRoughnessCoefficient;
        }
    }

}
