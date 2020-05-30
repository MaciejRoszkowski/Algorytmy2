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



            //opt
            var result = GeneratePath(distances, weights, 12500, 0);

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
                    var distance = Math.Sqrt(Math.Pow(cords[i].X - cords[j].X, 2) + Math.Pow(cords[i].Y - cords[j].Y, 2));
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
                if (bestResult.distance > result.distance)
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
                    var listOfNumbers = Enumerable.Range(0, 399).Where(x => !path.Contains(x)).ToList();
                    bestIndex = listOfNumbers[random.Next(listOfNumbers.Count)];
                }
                else
                {
                    for (int i = 0; i < 400; i++)
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
            return new PathWithValues() { distance = pathLength, weightsSum = weightsSum, nodes = path };
        }

        private bool Insert(PathWithValues path, double[,] distances, int[] weights)
        {
            bool isBetter = false;



            return isBetter;
        }

        private bool Remove()
        {
            return false;
        }



    }
}