using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Algorithm2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PathController : ControllerBase
    {
        
        [HttpGet]
        public PathWithValues Get()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            var lines = System.IO.File.ReadAllLines("test.txt");
            var style = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
            var cordCount = int.Parse(lines[0]);

            var cords = new Coordinates[cordCount];
            var weights = new int[cordCount];

            for (int i = 0; i < cordCount; i++)
            {
                var line = lines[i + 1].Split(' ');
                cords[i] = new Coordinates(double.Parse(line[0], style), double.Parse(line[1], style));
                weights[i] = int.Parse(line[2]);
            }

            var distances = CalculateDistances(cords);


            //vns
            //ils


            int min = 12500;

            var result = GeneratePath(distances, weights, min, 0);

            LocalSearchInsert(result, distances, weights, min);

            return result;
        }

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
            for (int i = 0; i < cords.Length; i++)
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

        private PathWithValues GeneratePath(double[,] distances, int[] weights, int minGain, int startingPoint)
        {
            var bestResult = GeneratePathSingle(distances, weights, 12500, 0);

            for (int i = 0; i < 100; i++)
            {

                var result = GeneratePathSingle(distances, weights, 12500, 0);
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

            int currentPoint = 0;

            while (weightsSum < minGain)
            {
                double bestRatio = 0;
                int bestIndex = 0;
                if (random.Next(1, 100) == 1)
                {
                    var listOfNumbers = Enumerable.Range(0, weights.Length - 1).Where(x => !path.Contains(x)).ToList();
                    bestIndex = listOfNumbers[random.Next(listOfNumbers.Count)];
                }
                else
                {
                    for (int i = 0; i < weights.Length; i++)
                    {
                        if (path.Contains(i))
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
                currentPoint = bestIndex;

            }
            pathLength += distances[path.Last(), startingPoint];
            return new PathWithValues() { Distance = pathLength, WeightsSum = weightsSum, Nodes = path };
        }

        private void LocalSearchInsert(PathWithValues path, double[,] distances, int[] weights, int min)
        {
            while (path.WeightsSum < 12500)
            {
                Insert(path, distances, weights);
            }
        }

        private void Insert(PathWithValues path, double[,] distances, int[] weights)
        {
            //bool isBetter = false;
            int nodesCount = weights.Count();

            var bestTmpPath = path.Clone();
            //var nodesToCheck = new List<int>();

            var tmpDistance = path.Distance;
            double bestH = 0;
            int bestFirst = 0;
            int bestInsertedNode =0;
            double bestDistanceAdded = 0;
            int bestWeight = 0;
            // h = weight/distance have to big


            for (int i = 0; i < nodesCount; i++)
            {
                if (!path.Nodes.Contains(i))
                {
                    int weight = weights[i];
                    for (int j = 0; j < path.Nodes.Count - 1; j++)
                    {
                        int first = path.Nodes[j];
                        int second = path.Nodes[j + 1];
                        double distnceAdded = distances[first, i] + distances[i, second] - distances[first, second];
                        double newH = weight / distnceAdded;

                        //rather imposible, but we have to check
                        if (newH < 0)
                        {
                            newH = newH * -100000;
                        }

                        if (newH > bestH)
                        {
                            bestH = newH;
                            bestFirst = first;
                            bestInsertedNode = i;
                            bestDistanceAdded = distnceAdded;
                            bestWeight = weight;
                        }


                    }

                    //last 

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


            //return isBetter;
        }
        

        private void Opt()
        {

        }

        private bool Remove()
        {
            return false;
        }



    }
}