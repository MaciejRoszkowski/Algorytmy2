using System;
using System.Linq;
using Algorithm2.Controllers;

namespace Algorithm2.Helpers
{
    public static class Operations
    {
        public static bool LocalSearchInsert(PathWithValues path, double[,] distances, int[] weights, int min)
        {
            bool isBetter = false;
            while (path.WeightsSum < min)
            {
                Insert(path, distances, weights);
                isBetter = true;
            }

            return isBetter;
        }

        public static void LocalSearch2Opt(PathWithValues path, double[,] distances, int[] weights)
        {
            bool isBetter = true;
            while (isBetter)
            {
                isBetter = Opt2(path, distances, weights);
            }
        }

        public static void RemoveRandomPath(PathWithValues path, double[,] distances, int[] weights, int degree)
        {
            PathGenerator.Verify(path, distances, weights);
            var random = new Random();
            int startingPoint = 0;
            try
            {
                startingPoint = random.Next(2, path.Nodes.Count - 2 - (degree * 5));
            }
            catch (Exception)
            {
                return;
            }

            double dist = 0;
            int weightsSum = 0;


            for (int i = 0; i < 5 * degree; i++)
            {
                dist -= distances[path.GetNodeAt(startingPoint + i), path.GetNodeAt(startingPoint + i + 1)];
                weightsSum -= weights[path.GetNodeAt(startingPoint + i + 1)];
            }

            dist -= distances[path.GetNodeAt(startingPoint + (5 * degree)),
                path.GetNodeAt(startingPoint + 1 + (5 * degree))];
            dist += distances[path.GetNodeAt(startingPoint), path.GetNodeAt(startingPoint + 1 + (5 * degree))];

            path.Distance += dist;
            path.WeightsSum += weightsSum;
            path.Nodes.RemoveRange(startingPoint + 1, degree * 5);
            PathGenerator.Verify(path, distances, weights);
        }

        private static void Insert(PathWithValues path, double[,] distances, int[] weights)
        {
            int nodesCount = weights.Count();


            double bestH = 0;
            int bestFirst = 0;
            int bestInsertedNode = 0;
            double bestDistanceAdded = 0;
            int bestWeight = 0;

            int first;
            int second;
            double distanceAdded = 0;
            double newH = 0;
            // h = weight/distance have to be big


            for (int i = 1; i < nodesCount; i++)
            {
                if (!path.Nodes.Contains(i))
                {
                    int weight = weights[i];
                    for (int j = 0; j < path.Nodes.Count - 1; j++)
                    {
                        first = path.Nodes[j];
                        second = path.Nodes[j + 1];
                        distanceAdded = distances[first, i] + distances[i, second] - distances[first, second];
                        if (distanceAdded <= 0)
                        {
                            newH = distanceAdded * -weight * 1000;
                        }
                        else
                        {
                            newH = weight / distanceAdded;
                        }

                        if (newH > bestH)
                        {
                            bestH = newH;
                            bestFirst = first;
                            bestInsertedNode = i;
                            bestDistanceAdded = distanceAdded;
                            bestWeight = weight;
                        }
                    }

                    first = path.Nodes.Last();
                    second = path.Nodes.First();

                    distanceAdded = distances[first, i] + distances[i, second] - distances[first, second];


                    if (distanceAdded <= 0)
                    {
                        newH = distanceAdded * -weight * 1000;
                    }
                    else
                    {
                        newH = weight / distanceAdded;
                    }


                    if (newH > bestH)
                    {
                        bestH = newH;
                        bestFirst = first;
                        bestInsertedNode = i;
                        bestDistanceAdded = distanceAdded;
                        bestWeight = weight;
                    }
                }
            }

            path.Distance += bestDistanceAdded;
            path.WeightsSum += bestWeight;
            int index = path.Nodes.IndexOf(bestFirst);
            if (index != path.Nodes.Count - 1)
            {
                path.Nodes.Insert(index + 1, bestInsertedNode);
            }
            else
            {
                path.Nodes.Add(bestInsertedNode);
            }

            PathGenerator.Verify(path, distances, weights);

            //return isBetter;
        }

        private static bool Opt2(PathWithValues path, double[,] distances, int[] weights)
        {
            var tmpNodes = path.CloneNodes();

            int totalNodesCount = path.Nodes.Count;
            double distDif = 0;

            for (int i = 0; i < totalNodesCount - 2; i++)
            {
                for (int j = i + 2; j < totalNodesCount; j++)
                {
                    if (j == totalNodesCount - 1)
                    {
                        distDif = distances[path.GetNodeAt(i), path.GetNodeAt(j)] +
                                  distances[path.GetNodeAt(i + 1), path.GetNodeAt(0)]
                                  - (distances[path.GetNodeAt(i), path.GetNodeAt(i + 1)] +
                                     distances[path.GetNodeAt(j), path.GetNodeAt(0)]);
                    }
                    else
                    {
                        distDif = distances[path.GetNodeAt(i), path.GetNodeAt(j)] +
                                  distances[path.GetNodeAt(i + 1), path.GetNodeAt(j + 1)]
                                  - distances[path.GetNodeAt(i), path.GetNodeAt(i + 1)] -
                                  distances[path.GetNodeAt(j), path.GetNodeAt(j + 1)];
                    }

                    if (distDif < 0)
                    {
                        path.Distance += distDif;
                        for (int k = 0; k < j - i; k++)
                        {
                            path.Nodes[i + k + 1] = tmpNodes[j - k];
                        }

                        PathGenerator.Verify(path, distances, weights);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}