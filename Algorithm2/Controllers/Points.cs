using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Algorithm2.Controllers
{
    public class Points
    {
        public List<Point> points { get; set; } = new List<Point>();
        public Dictionary<string, List<Neighbour>> neighbours { get; set; } = new Dictionary<string, List<Neighbour>>();
    }
}
