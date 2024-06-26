using System;

namespace ClipperLib
{

    public struct IntRect
    {
        public Int64 left;
        public Int64 top;
        public Int64 right;
        public Int64 bottom;

        public IntRect(Int64 l, Int64 t, Int64 r, Int64 b)
        {
            left = l;
            top = t;
            right = r;
            bottom = b;
        }

        public IntRect(IntRect ir)
        {
            left = ir.left;
            top = ir.top;
            right = ir.right;
            bottom = ir.bottom;
        }
    }

}