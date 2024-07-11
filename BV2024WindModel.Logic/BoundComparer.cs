using System;
using System.Collections.Generic;
using BV2024WindModel.Abstractions;

namespace BV2024WindModel.Logic
{
    public class BoundComparer : IEqualityComparer<Bounds>
    {
        public bool Equals(Bounds x, Bounds y)
        {
            return Math.Abs(x.MinX - y.MinX) < 1e-6 && Math.Abs(x.MaxX - y.MaxX) < 1e-6;
        }

        public int GetHashCode(Bounds obj)
        {
            return  obj.MaxX.GetHashCode() * 13 * 1000;
        }
    }
}
    



         