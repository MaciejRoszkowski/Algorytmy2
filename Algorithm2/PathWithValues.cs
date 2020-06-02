using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Algorithm2.Controllers
{
    public class PathWithValues
    {
        public List<int> Nodes { get; set; }
        public int WeightsSum { get; set; }
        public double Distance { get; set; }

        public PathWithValues()
        {

        }
        public PathWithValues(List<int> nodes, int weightsSum, double distance)
        {
            Nodes = nodes;
            WeightsSum = weightsSum;
            Distance = distance;
        }


        public PathWithValues Clone()
        {
            return new PathWithValues(Nodes, WeightsSum, Distance);
        }
    }
}
