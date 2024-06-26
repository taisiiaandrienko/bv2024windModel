using System.Collections.Generic;

namespace ClipperLib
{

    public class MyIntersectNodeSort : IComparer<IntersectNode>
    {
        public int Compare(IntersectNode node1, IntersectNode node2)
        {
            var i = node2.Pt.Y - node1.Pt.Y;
            if (i > 0)
            {
                return 1;
            }
            if (i < 0)
            {
                return -1;
            }
            return 0;
        }
    }

}