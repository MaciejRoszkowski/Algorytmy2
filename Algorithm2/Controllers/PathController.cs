using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


/// <summary>
/// TODO: zmieniać na 1base index, albo dane mapować z 1base na 0base; algorytmy elegancko ugenerycznić 
/// 
/// </summary>


namespace Algorithm2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PathController : ControllerBase
    {

        //[HttpGet]
        //public PathWithValues Get()
        //{
        //    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

        //    var lines = System.IO.File.ReadAllLines("test.txt");
        //    var style = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
        //    var cordCount = int.Parse(lines[0]);

        //    var cords = new Coordinates[cordCount];
        //    var weights = new int[cordCount];

        //    for (int i = 0; i < cordCount; i++)
        //    {
        //        var line = lines[i + 1].Split(' ');
        //        cords[i] = new Coordinates(double.Parse(line[0], style), double.Parse(line[1], style));
        //        weights[i] = int.Parse(line[2]);
        //    }

        //    var distances = CalculateDistances(cords);


        //    int min = 12500;
        //    var result = ILS(distances, weights, min, 0);


        //    //int min = 12500;

        //    //var result = GeneratePath(distances, weights, min, 0);
        //    //LocalSearch2Opt(result, distances, weights);
        //    //LocalSearchInsert(result, distances, weights, min);

        //    return result;
        //}

        [HttpGet("getPoints")]
        public Points GetPoints()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            var cords = new Coordinates[400];
            var weights = new int[400];
            var lines = System.IO.File.ReadAllLines("test.txt");
            var style = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
            for (int i = 0; i < 400; i++)
            {
                var line = lines[i + 1].Split(' ');
                cords[i] = new Coordinates(double.Parse(line[0], style), double.Parse(line[1], style));
                weights[i] = int.Parse(line[2]);
            }

            var distances = CalculateDistances(cords);
            var points = new Points();
            for (int i = 0; i < 400; i++)
            {
                points.points.Add(new Point() { id = i.ToString(), latitude = cords[i].X, longtitude = cords[i].Y, name = "" });
                points.neighbours[i.ToString()] = new List<Neighbour>();
                for (int j = 0; j < 400; j++)
                {
                    points.neighbours[i.ToString()].Add(new Neighbour() { pointId = j.ToString(), weight = distances[i, j] });

                }
            }
            return points;

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
        public PathWithValues GetVns()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            var lines = System.IO.File.ReadAllLines("test.txt");
            var style = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
            var cordCount = int.Parse(lines[0]);

            var cords = new Coordinates[cordCount+1];
            var weights = new int[cordCount+1];

            for (int i = 0; i < cordCount; i++)
            {
                var line = lines[i + 1].Split(' ');
                cords[i + 1] = new Coordinates(double.Parse(line[0], style), double.Parse(line[1], style));
                weights[i + 1] = int.Parse(line[2]);
            }

            var distances = CalculateDistances(cords);


            int min = 12500;
            int startingPoint = 1;
            var result = VNS(distances, weights, min, startingPoint);

            return result;
        }

        [HttpGet("ils")]
        public PathWithValues GetIls()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            var lines = System.IO.File.ReadAllLines("test.txt");
            var style = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
            var cordCount = int.Parse(lines[0]);

            var cords = new Coordinates[cordCount + 1];
            var weights = new int[cordCount + 1];

            //cords[0] = new Coordinates(0, 0);
            for (int i = 0; i < cordCount; i++)
            {
                var line = lines[i + 1].Split(' ');
                cords[i + 1] = new Coordinates(double.Parse(line[0], style), double.Parse(line[1], style));
                weights[i + 1] = int.Parse(line[2]);
            }

            var distances = CalculateDistances(cords);


            int min = 12500;
            int startingPoint = 1;
            var result = ILS(distances, weights, min, startingPoint);

            return result;
        }




        //here real data



        private PathWithValues GeneratePath(double[,] distances, int[] weights, int minGain, int startingPoint)
        {
            var bestResult = GeneratePathSingle(distances, weights, minGain, startingPoint);

            for (int i = 0; i < 100; i++)
            {

                var result = GeneratePathSingle(distances, weights, minGain, startingPoint);
                if (bestResult.Distance > result.Distance)
                {
                    bestResult = result;
                }
            }

            return bestResult;
        }


        private PathWithValues GeneratePathSingle(double[,] distances, int[] weights, int minGain, int startingPoint)
        {
            var random = new Random();
            var path = new List<int>() { startingPoint };
            int weightsSum = 0;
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
            var startingPoint = random.Next(2, path.Nodes.Count - 2 - (degree * 5));

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
            for (int i = 0; i < 300; i++)
            {
                int k = 1;

                while (k < 3)
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

                        RecalculateDist(result, distances, weights);



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

            //LocalSearch2Opt(result, distances, weights);
            //LocalSearchInsert(result, distances, weights, minGain);

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
            return;
            double newDist = 0;
            int newWeight = 0;
            for (int i = 0; i < path.Nodes.Count - 1; i++)
            {
                newDist += distances[path.GetNodeAt(i), path.GetNodeAt(i + 1)];
                newWeight += weights[path.GetNodeAt(i)];
            }
            newDist += distances[path.GetNodeAt(path.Nodes.Count - 1), path.GetNodeAt(0)];
            newWeight += weights[path.GetNodeAt(path.Nodes.Count - 1)];

            if (path.Distance != newDist)
            {
                //asdasfa
                int a = 1;
                throw new Exception();
                //path.Distance = newDist;

            }
            if (newWeight != path.WeightsSum)
            {
                throw new Exception();

                path.WeightsSum = newWeight;
                int a = 1;
            }
        }
    }
}