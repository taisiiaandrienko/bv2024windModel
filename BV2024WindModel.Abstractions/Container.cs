using System.Collections.Generic;
using System.Drawing;

namespace BV2024WindModel.Abstractions
{
    public class Container : Quader
    {
        public Container(string id, double lcg, double tcg, double basis, double length, double width, double height) : base(id, lcg, tcg, basis, length, width, height)
        {
        }
    }
}
