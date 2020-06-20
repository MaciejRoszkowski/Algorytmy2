using System;
using System.Collections.Generic;
using System.Linq;
using Algorithm2.Controllers;

namespace Algorithm2.Helpers
{
    public static class PathGenerator
    {
        public static PathWithValues GeneratePath(double[,] distances, int[] weights, int minGain, int startingPoint)
        {
            var bestResult = GeneratePathSingle(distances, weights, minGain, startingPoint);

            for (int i = 0; i < 100; i++)
            {

                var result = GeneratePathSingle(distances, weights, minGain, startingPoint);
                Verify(result, distances, weights);
                if (bestResult.Distance > result.Distance)
                {
                    bestResult = result;
                }
            }
            Verify(bestResult, distances, weights);
            return bestResult;
        }
        
        public static void Verify(PathWithValues path, double[,] distances, int[] weights)
        {

            double newDist = 0;
            int newWeight = 0;
            var visited = new Dictionary<int, bool>();

            for (int i = 0; i < path.Nodes.Count - 1; i++)
            {
                newDist += distances[path.GetNodeAt(i), path.GetNodeAt(i + 1)];
                newWeight += weights[path.GetNodeAt(i)];
                if (visited.ContainsKey(path.GetNodeAt(i)))
                {
                    throw new Exception();
                }
                visited[path.GetNodeAt(i)] = true;
            }
            newDist += distances[path.GetNodeAt(path.Nodes.Count - 1), path.GetNodeAt(0)];
            newWeight += weights[path.GetNodeAt(path.Nodes.Count - 1)];

            if (Math.Abs(path.Distance - newDist) > 0.05)
            {
                throw new Exception();
            }
            if (newWeight != path.WeightsSum)
            {
                throw new Exception();
            }
        }
        
        private static PathWithValues GeneratePathSingle(double[,] distances, int[] weights, int minGain, int startingPoint)
        {
            var random = new Random();
            var path = new List<int>() { startingPoint };
            int weightsSum = weights[startingPoint];
            double pathLength = 0;
            var visited = new Dictionary<int, bool>();
            for (int i = 0; i < weights.Length; i++)
            {
                visited[i] = false;
            }
            int currentPoint = startingPoint;
            visited[0] = true;
            visited[startingPoint] = true;

            while (weightsSum < minGain)
            {
                double bestRatio = 0;
                int bestIndex = 0;
                if (random.Next(100) == 5)
                {
                    var listOfNumbers = Enumerable.Range(1, weights.Length - 1).Where(x => !visited[x]).ToList();
                    if (listOfNumbers.Count() == 0)
                    {
                        break;
                    }
                    bestIndex = listOfNumbers[random.Next(listOfNumbers.Count)];
                }
                else
                {
                    for (int i = 1; i < weights.Length; i++)
                    {
                        if (visited[i])
                        {
                            continue;
                        }
                        var ratio = weights[i] / distances[currentPoint, i];
                        if (ratio > bestRatio)
                        {
                            bestRatio = ratio;
                            bestIndex = i;
                        }

                    }
                }
                if (bestIndex == 0)
                {
                    break;
                }
                pathLength += distances[currentPoint, bestIndex];
                weightsSum += weights[bestIndex];
                path.Add(bestIndex);
                visited[bestIndex] = true;
                currentPoint = bestIndex;

            }
            pathLength += distances[path.Last(), startingPoint];
            return new PathWithValues() { Distance = pathLength, WeightsSum = weightsSum, Nodes = path };
        }
    }
}