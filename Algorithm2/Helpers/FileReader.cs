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
        public static (Coordinates[],int[]) ReadTestData()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            var lines = System.IO.File.ReadAllLines("test.txt");
            var style = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
            var cordCount = int.Parse(lines[0]);

            var cords = new Coordinates[cordCount + 1];
            var weights = new int[cordCount + 1];

            for (int i = 0; i < cordCount; i++)
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
            int cityCount = cityLines.Count();
            //var weights = new double[cityCount + 1];
            var weights = new int[201 + 1];
            var distances = new Dictionary<(int, int), int>();
            var connections = new Dictionary<int, List<int>>();
            int cityIndex = 0;
            int peopleCount = 0;



            for (int i = 0; i < cityCount; i++)
            {
                var line = cityLines[i].Split(' ', '\t');
                cityIndex = int.Parse(line[0]);
                peopleCount = int.Parse(line[2]);
                weights[cityIndex] = (int)(0.01 * peopleCount);
            }

            var connectionLines = System.IO.File.ReadAllLines("polaczenia.txt");

            for (int i = 1; i < 201 + 1; i++)
            {
                connections.Add(i, new List<int>());
            }


            for (int i = 0; i < connectionLines.Count(); i++)
            {
                var connectionLine = connectionLines[i].Split(' ');
                int cityA = int.Parse(connectionLine[0]);
                int cityB = int.Parse(connectionLine[1]);
                int distance = (int)double.Parse(connectionLine[2]);
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
    }
}
