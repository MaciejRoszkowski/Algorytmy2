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
            for (var i = 1; i <= 400; i++)
            {
                points.points.Add(new Point()
                {
                    id = i.ToString(),
                    latitude = cords[i].X,
                    longtitude = cords[i].Y,
                    name = "",
                    weight = weights[i]
                });
                points.neighbours[i.ToString()] = new List<Neighbour>();
                for (var j = 1; j <= 400; j++)
                {
                    points.neighbours[i.ToString()].Add(new Neighbour()
                        {pointId = j.ToString(), weight = distances[i, j]});
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

        [HttpGet("vns")]
        public PathWithValues GetVns(int start, int min)
        {
            var (cords, weights) = FileReader.ReadTestData();
            var distances = CalculateDistances(cords);
            var result = AlgorithmRunner.VNS(distances, weights, min, start);

            return result;
        }

        [HttpGet("ils")]
        public PathWithValues GetIls(int start, int min)
        {
            var (cords, weights) = FileReader.ReadTestData();
            var distances = CalculateDistances(cords);
            var result = AlgorithmRunner.ILS(distances, weights, min, start);

            return result;
        }

        [HttpGet("realIls")]
        public PathWithValues GetIlsRealData(int start, int min)
        {
            var (connections, distances, weights) = FileReader.ReadRealData();
            var distancesArray = AlgorithmRunner.Dijkstra(connections, distances, 201);
            
            var result = AlgorithmRunner.ILS(distancesArray, weights, min, start);
            return result;
        }


        [HttpGet("realvns")]
        public PathWithValues GetVnsRealData(int start, int min)
        {
            var (connections, distances, weights) = FileReader.ReadRealData();
            var distancesArray = AlgorithmRunner.Dijkstra(connections, distances, 201);
            
            var result = AlgorithmRunner.VNS(distancesArray, weights, min, start);
            return result;
        }


        [HttpGet("flow")]
        public int EdmondsKarp(int start, int end)
        {
            var (connections, distances) = FileReader.ConnectionsForFlow();
            return AlgorithmRunner.EdmondsKarp(start, end, connections, distances);
        }

        private double[,] CalculateDistances(Coordinates[] cords)
        {
            var distances = new double[cords.Length, cords.Length];
            for (int i = 1; i < cords.Length; i++)
            {
                for (int j = i; j < cords.Length; j++)
                {
                    var distance =
                        Math.Floor(Math.Sqrt(
                            Math.Pow(cords[i].X - cords[j].X, 2) + Math.Pow(cords[i].Y - cords[j].Y, 2)));
                    distances[i, j] = distance;
                    distances[j, i] = distance;
                }
            }

            return distances;
        }
    }
}