﻿using System.Collections.Generic;

namespace BV2024WindModel.Abstractions
{
    public class TransverseSurfacesCalculationResult
    {
        public List<TransverseSurfaceCalculationResult> Portside;
        public List<TransverseSurfaceCalculationResult> Starboard;
    }
}