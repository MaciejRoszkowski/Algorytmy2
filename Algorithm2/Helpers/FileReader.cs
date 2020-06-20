using Algorithm2.Controllers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Algorithm2.Helpers
{
    public static class FileReader
    {
        public static (Coordinates[], int[]) ReadTestData()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            var lines = System.IO.File.ReadAllLines("test.txt");
            var style = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
            var cordCount = int.Parse(lines[0]);

            var cords = new Coordinates[cordCount + 1];
            var weights = new int[cordCount + 1];

            for (var i = 0; i < cordCount; i++)
            {
                var line = lines[i + 1].Split(' ');
                cords[i + 1] = new Coordinates(double.Parse(line[0], style), double.Parse(line[1], style));
                weights[i + 1] = int.Parse(line[2]);
            }

            return (cords, weights);
        }

        public static (Dictionary<int, List<int>>, Dictionary<(int, int), int>, int[]) ReadRealData()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            var cityLines = System.IO.File.ReadAllLines("miasta.txt");
            var cityCount = cityLines.Count();
            var weights = new int[201 + 1];
            var distances = new Dictionary<(int, int), int>();
            var connections = new Dictionary<int, List<int>>();

            for (var i = 0; i < cityCount; i++)
            {
                var line = cityLines[i].Split(' ', '\t');
                var cityIndex = int.Parse(line[0]);
                var peopleCount = int.Parse(line[2]);
                weights[cityIndex] = (int)(0.01 * peopleCount);
            }
            for (var i = 1; i < 201 + 1; i++)
            {
                connections.Add(i, new List<int>());
            }

            var connectionLines = System.IO.File.ReadAllLines("polaczenia.txt");
            for (var i = 0; i < connectionLines.Count(); i++)
            {
                var connectionLine = connectionLines[i].Split(' ');
                var cityA = int.Parse(connectionLine[0]);
                var cityB = int.Parse(connectionLine[1]);
                var distance = (int)double.Parse(connectionLine[2]);
                connections[cityA].Add(cityB);
                connections[cityB].Add(cityA);
                if (distances.ContainsKey((cityA, cityB)))
                {
                    continue;
                }
                distances.Add((cityA, cityB), distance);
                distances.Add((cityB, cityA), distance);
            }

            return (connections, distances, weights);
        }

        public static Points ReadRealDataToDraw()
        {
            var result = new Points();
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            var cityLines = System.IO.File.ReadAllLines("miasta.txt");
            var cityCount = cityLines.Count();

            for (var i = 0; i < cityCount; i++)
            {
                var line = cityLines[i].Split(' ', '\t');
                
                var cityIndex = int.Parse(line[0]);
                var cityName = line[1];
                var peopleCount = int.Parse(line[2]);
                var cordX = double.Parse(line[3]);
                var cordY = double.Parse(line[4]);
                result.points.Add(new Point()
                {
                    id = cityIndex.ToString(),
                    name = cityName,
                    latitude = cordX,
                    longtitude = cordY,
                    weight = peopleCount
                });
            }

            return result;
        }

        public static (Dictionary<int, List<int>>, Dictionary<(int, int),int>) ConnectionsForFlow()
        {
            var connectionLines = System.IO.File.ReadAllLines("polaczenia.txt");
            var distances = new Dictionary<(int, int), int>();
            var connections = new Dictionary<int, List<int>>();
            for (var i = 1; i < 201 + 1; i++)
            {
                connections.Add(i, new List<int>());
            }


            for (var i = 0; i < connectionLines.Count(); i++)
            {
                var connectionLine = connectionLines[i].Split(' ');
                var cityA = int.Parse(connectionLine[0]);
                var cityB = int.Parse(connectionLine[1]);
                connections[cityA].Add(cityB);
                connections[cityB].Add(cityA);
                if (distances.ContainsKey((cityA, cityB)))
                {
                    continue;
                }
                distances.Add((cityA, cityB), 1);
                distances.Add((cityB, cityA), 1);
            }

            return (connections, distances);
        }
    }
}
