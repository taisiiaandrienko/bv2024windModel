using System;
using BV2024WindModel.Abstractions;


namespace BV2024WindModel.Logic
{

    public class WindForceCalculator : ICalculator<WindForcesCalculationParameters, double>
    {
        public double Calculate(in WindForcesCalculationParameters input)
        { 
            double force =  input.ExternalParameters.AirDencity * Math.Pow(input.ExternalParameters.WindSpeed * Math.Pow((input.VolumetricCenter - input.ExternalParameters.Draft) / 10, input.ExternalParameters.WaterSurfaceRoughnessCoefficient), 2) * input.Area / 2000;
            return force;
        }
    }
}


