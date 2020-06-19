using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Algorithm2.Classes;
using Algorithm2.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


/// <summary>
/// TODO:   polaczenia
/// </summary>


namespace Algorithm2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PathController : ControllerBase
    {


        [HttpGet("getPoints")]
        public Points GetPoints()
        {
            var (cords, weights) = FileReader.ReadTestData();

            var distances = CalculateDistances(cords);
            var points = new Points();
            for (int i = 1; i <= 400; i++)
            {
                points.points.Add(new Point() { id = i.ToString(), latitude = cords[i].X, longtitude = cords[i].Y, name = "", weight = weights[i] });
                points.neighbours[i.ToString()] = new List<Neighbour>();
                for (int j = 1; j <= 400; j++)
                {
                    points.neighbours[i.ToString()].Add(new Neighbour() { pointId = j.ToString(), weight = distances[i, j] });

                }
            }
            return points;

        }

        [HttpGet("realGetPoints")]
        public Points GetRealPoints()
        {
            var result = FileReader.ReadRealDataToDraw();
            return result;
        }


        public double[,] CalculateDistances(Coordinates[] cords)
        {
            var distances = new double[cords.Length, cords.Length];
            for (int i = 1; i < cords.Length; i++)
            {
                for (int j = i; j < cords.Length; j++)
                {
                    var distance = Math.Floor(Math.Sqrt(Math.Pow(cords[i].X - cords[j].X, 2) + Math.Pow(cords[i].Y - cords[j].Y, 2)));
                    distances[i, j] = distance;
                    distances[j, i] = distance;
                }
            }
            return distances;
        }

        [HttpGet("vns")]
        public PathWithValues GetVns(int start, int min)
        {
            var (cords, weights) = FileReader.ReadTestData();

            var distances = CalculateDistances(cords);


            var result = VNS(distances, weights, min, start);

            return result;
        }

        [HttpGet("ils")]
        public PathWithValues GetIls(int start, int min)
        {
            var (cords, weights) = FileReader.ReadTestData();

            var distances = CalculateDistances(cords);

            var result = ILS(distances, weights, min, start);

            return result;
        }

        [HttpGet("realils")]
        public PathWithValues GetIlsRealData(int start, int min)
        {

            var (connections, distances, weights) = FileReader.ReadRealData();
            var distancesArray = Dijkstra(connections, distances, 201);

            var result = ILS(distancesArray, weights, min, start);


            return result;
        }


        [HttpGet("realvns")]
        public PathWithValues GetVnsRealData(int start, int min)
        {

            var (connections, distances, weights) = FileReader.ReadRealData();
            var distancesArray = Dijkstra(connections, distances, 201);

            var result = VNS(distancesArray, weights, min, start);


            return result;
        }


        [HttpGet("flow")]
        public int EdmondsKarp(int start, int end)
        {
            int w = 0;
            var (connections, distances) = FileReader.ConnectionsForFlow();

            // distances(ij) == r(ij) fmin =1 
            while (true)
            {
                var path = BFS(connections, distances, start, end);
                if (path == null)
                {
                    break;
                }

                w++;
                for (int i = 0; i < path.Count() - 1; i++)
                {
                    distances[(path[i], path[i + 1])] -= 1;
                    distances[(path[i + 1], path[i])] += 1;
                }
            }



            return w;
        }

        private List<int> BFS(Dictionary<int, List<int>> connections, Dictionary<(int, int), int> weights, int start, int finish)
        {
            var visited = new Dictionary<int, bool>();
            var previous = new int[202];
            var queue = new Queue<int>();

            for (int i = 1; i < 202; i++)
            {
                visited[i] = false;
            }
            visited[start] = true;
            queue.Enqueue(start);

            while (queue.Count() != 0)
            {
                var current = queue.Dequeue();
                foreach (var item in connections[current])
                {
                    if (!visited[item] && weights[(current, item)] > 0)
                    {
                        queue.Enqueue(item);
                        visited[item] = true;
                        previous[item] = current;
                    }
                }
                if (visited[finish])
                {
                    var result = new List<int>();
                    int toAdd = finish;
                    while (toAdd != start)
                    {
                        result.Add(toAdd);
                        toAdd = previous[toAdd];
                    }
                    result.Add(start);

                    result.Reverse();
                    return result;
                }
            }

            return null;

        }

        private double[,] Dijkstra(Dictionary<int, List<int>> graph, Dictionary<(int, int), int> weights, int n)
        {
            var distances = new double[n + 1, n + 1];
            for (int i = 1; i <= n; i++)
            {
                var visited = new Dictionary<int, bool>();
                var d = new int[n + 1];
                int v;

                for (int j = 1; j <= n; j++)
                {
                    visited[j] = false;
                    d[j] = int.MaxValue;
                }
                var start = new int[] { i };
                d[i] = 0;

                var sortedSet = new SortedSet<int>(start, new Comparer(d));


                while (sortedSet.Count > 0)
                {
                    v = sortedSet.First();
                    sortedSet.Remove(v);
                    visited[v] = true;


                    foreach (var item in graph[v])
                    {
                        if (!visited[item] && d[v] + weights[(v, item)] < d[item])
                        {
                            sortedSet.Remove(item);
                            d[item] = d[v] + weights[(v, item)];
                            sortedSet.Add(item);
                        }
                    }
                }

                for (int j = 1; j <= n; j++)
                {
                    distances[i, j] = d[j];
                }
            }

            return distances;
        }

        private PathWithValues GeneratePath(double[,] distances, int[] weights, int minGain, int startingPoint)
        {
            var bestResult = GeneratePathSingle(distances, weights, minGain, startingPoint);

            for (int i = 0; i < 100; i++)
            {

                var result = GeneratePathSingle(distances, weights, minGain, startingPoint);
                RecalculateDist(result, distances, weights);
                if (bestResult.Distance > result.Distance)
                {
                    bestResult = result;
                }
            }
            RecalculateDist(bestResult, distances, weights);
            return bestResult;
        }


        private PathWithValues GeneratePathSingle(double[,] distances, int[] weights, int minGain, int startingPoint)
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

        private bool LocalSearchInsert(PathWithValues path, double[,] distances, int[] weights, int min)
        {
            bool isBetter = false;
            while (path.WeightsSum < min)
            {
                Insert(path, distances, weights);
                isBetter = true;
            }
            return isBetter;
        }

        private void Insert(PathWithValues path, double[,] distances, int[] weights)
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

            RecalculateDist(path, distances, weights);

            //return isBetter;
        }
        private void LocalSearch2Opt(PathWithValues path, double[,] distances, int[] weights)
        {
            bool isBetter = true;
            while (isBetter)
            {
                isBetter = Opt2(path, distances, weights);
            }
        }

        private bool Opt2(PathWithValues path, double[,] distances, int[] weights)
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
                        distDif = distances[path.GetNodeAt(i), path.GetNodeAt(j)] + distances[path.GetNodeAt(i + 1), path.GetNodeAt(0)]
                            - (distances[path.GetNodeAt(i), path.GetNodeAt(i + 1)] + distances[path.GetNodeAt(j), path.GetNodeAt(0)]);

                    }
                    else
                    {
                        distDif = distances[path.GetNodeAt(i), path.GetNodeAt(j)] + distances[path.GetNodeAt(i + 1), path.GetNodeAt(j + 1)]
                            - distances[path.GetNodeAt(i), path.GetNodeAt(i + 1)] - distances[path.GetNodeAt(j), path.GetNodeAt(j + 1)];
                    }

                    if (distDif < 0)
                    {
                        path.Distance += distDif;
                        for (int k = 0; k < j - i; k++)
                        {
                            path.Nodes[i + k + 1] = tmpNodes[j - k];
                        }
                        RecalculateDist(path, distances, weights);
                        return true;
                    }
                }
            }

            return false;
        }

        //disturb
        private void RemoveRandomPath(PathWithValues path, double[,] distances, int[] weights, int degree)
        {
            RecalculateDist(path, distances, weights);
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

            var pathCopy = path.CloneNodes();

            double dist = 0;
            int weightsSum = 0;


            for (int i = 0; i < 5 * degree; i++)
            {
                dist -= distances[path.GetNodeAt(startingPoint + i), path.GetNodeAt(startingPoint + i + 1)];
                weightsSum -= weights[path.GetNodeAt(startingPoint + i + 1)];
                //path.Nodes.RemoveAt(startingPoint + 1);
            }

            dist -= distances[path.GetNodeAt(startingPoint + (5 * degree)), path.GetNodeAt(startingPoint + 1 + (5 * degree))];
            dist += distances[path.GetNodeAt(startingPoint), path.GetNodeAt(startingPoint + 1 + (5 * degree))];

            path.Distance += dist;
            path.WeightsSum += weightsSum;
            path.Nodes.RemoveRange(startingPoint + 1, degree * 5);
            RecalculateDist(path, distances, weights);
            //var asdf = RecalculateDist(path, distances);

        }


        private PathWithValues VNS(double[,] distances, int[] weights, int minGain, int startingPoint)
        {

            var result = GeneratePath(distances, weights, minGain, startingPoint);
            var tmpT = result.Clone();
            LocalSearch(result, distances, weights, minGain);
            for (int i = 0; i < 100; i++)
            {
                int k = 1;

                while (k <= 3)
                {
                    RecalculateDist(result, distances, weights);
                    tmpT = result.Clone();
                    RemoveRandomPath(tmpT, distances, weights, k);
                    LocalSearch(tmpT, distances, weights, minGain);
                    RecalculateDist(tmpT, distances, weights);
                    if (tmpT.Distance < result.Distance)
                    {
                        result = tmpT.Clone();
                        RecalculateDist(tmpT, distances, weights);

                        k = 1;
                    }
                    else
                    {
                        k++;
                    }
                }
            }
            RecalculateDist(result, distances, weights);
            return result;
        }

        private PathWithValues ILS(double[,] distances, int[] weights, int minGain, int startingPoint)
        {
            var result = GeneratePath(distances, weights, minGain, startingPoint);
            LocalSearch(result, distances, weights, minGain);

            for (int i = 0; i < 10; i++)
            {
                var pathLocal = GeneratePath(distances, weights, minGain, startingPoint);
                LocalSearch(pathLocal, distances, weights, minGain);
                for (int j = 0; j < 50; j++)
                {
                    var tmpT = pathLocal.Clone();
                    RemoveRandomPath(tmpT, distances, weights, 2);
                    LocalSearch(tmpT, distances, weights, minGain);
                    if (tmpT.Distance < result.Distance)
                    {
                        result = tmpT;
                    }
                    if (tmpT.Distance < pathLocal.Distance)
                    {
                        pathLocal = tmpT;
                    }
                }
            }


            RecalculateDist(result, distances, weights);

            return result;
        }

        private void LocalSearch(PathWithValues path, double[,] distances, int[] weights, int minGain)
        {
            LocalSearch2Opt(path, distances, weights);
            if (LocalSearchInsert(path, distances, weights, minGain))
            {
                LocalSearch2Opt(path, distances, weights);

            }

            RecalculateDist(path, distances, weights);
        }
        private void RecalculateDist(PathWithValues path, double[,] distances, int[] weights)
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

            if (path.Distance != newDist)
            {
                throw new Exception();
            }
            if (newWeight != path.WeightsSum)
            {
                throw new Exception();
            }
        }
    }
}