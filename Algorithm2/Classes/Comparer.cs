using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Algorithm2.Classes
{
    public class Comparer : IComparer<int>
    {
        public int[] dist { get; set; }
        public Comparer(int[] dist)
        {
            this.dist = dist;
        }
        public int Compare(int x, int y)
        {
            var compare = dist[x].CompareTo(dist[y]);
            if (compare == 0)
            {
                return x.CompareTo(y);
            }
            return compare;
        }

    }
}
