using System.Collections.Generic;
using System.Linq;
using Algorithm2.Classes;
using Algorithm2.Controllers;

namespace Algorithm2.Helpers
{
    public static class AlgorithmRunner
    {
        public static double[,] Dijkstra(
            Dictionary<int, List<int>> graph, 
            Dictionary<(int, int), int> weights, 
            int n)
        {
            var distances = new double[n + 1, n + 1];
            for (var i = 1; i <= n; i++)
            {
                var visited = new Dictionary<int, bool>();
                var d = new int[n + 1];

                for (var j = 1; j <= n; j++)
                {
                    visited[j] = false;
                    d[j] = int.MaxValue;
                }

                var start = new[] {i};
                d[i] = 0;

                var sortedSet = new SortedSet<int>(start, new Comparer(d));
                
                while (sortedSet.Count > 0)
                {
                    var v = sortedSet.First();
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

                for (var j = 1; j <= n; j++)
                {
                    distances[i, j] = d[j];
                }
            }

            return distances;
        }
        
        public static int EdmondsKarp(
            int start,
            int end,
            Dictionary<int, List<int>> connections,
            Dictionary<(int, int), int> distances)
        {
            var paths = 0;
            while (true)
            {
                var path = BFS(start, end, connections, distances);
                if (path == null)
                {
                    break;
                }

                paths++;
                for (var i = 0; i < path.Count() - 1; i++)
                {
                    distances[(path[i], path[i + 1])] -= 1;
                    distances[(path[i + 1], path[i])] += 1;
                }
            }

            return paths;
        }
        
        public static PathWithValues VNS(
            double[,] distances, 
            int[] weights, 
            int minGain, 
            int startingPoint)
        {
            var result = PathGenerator.GeneratePath(distances, weights, minGain, startingPoint);
            var tmpT = result.Clone();
            LocalSearch(result, distances, weights, minGain);
            for (int i = 0; i < 100; i++)
            {
                int k = 1;

                while (k <= 3)
                {
                    PathGenerator.Verify(result, distances, weights);
                    tmpT = result.Clone();
                    Operations.RemoveRandomPath(tmpT, distances, weights, k);
                    LocalSearch(tmpT, distances, weights, minGain);
                    PathGenerator.Verify(tmpT, distances, weights);
                    if (tmpT.Distance < result.Distance)
                    {
                        result = tmpT.Clone();
                        PathGenerator.Verify(tmpT, distances, weights);

                        k = 1;
                    }
                    else
                    {
                        k++;
                    }
                }
            }

            PathGenerator.Verify(result, distances, weights);
            return result;
        }

        public static PathWithValues ILS(
            double[,] distances, 
            int[] weights, 
            int minGain,
            int startingPoint)
        {
            var result = PathGenerator.GeneratePath(distances, weights, minGain, startingPoint);
            LocalSearch(result, distances, weights, minGain);

            for (int i = 0; i < 15; i++)
            {
                var pathLocal = PathGenerator.GeneratePath(distances, weights, minGain, startingPoint);
                LocalSearch(pathLocal, distances, weights, minGain);
                for (int j = 0; j < 50; j++)
                {
                    var tmpT = pathLocal.Clone();
                    Operations.RemoveRandomPath(tmpT, distances, weights, 2);
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


            PathGenerator.Verify(result, distances, weights);

            return result;
        }

        private static List<int> BFS(
            int start,
            int finish,
            Dictionary<int, List<int>> connections,
            Dictionary<(int, int), int> weights)
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

                if (!visited[finish]) continue;

                var result = new List<int>();
                var toAdd = finish;
                while (toAdd != start)
                {
                    result.Add(toAdd);
                    toAdd = previous[toAdd];
                }

                result.Add(start);

                result.Reverse();
                return result;
            }

            return null;
        }
        
        private static void LocalSearch(
            PathWithValues path, 
            double[,] distances, 
            int[] weights,
            int minGain)
        {
            Operations.LocalSearch2Opt(path, distances, weights);
            if (Operations.LocalSearchInsert(path, distances, weights, minGain))
            {
                Operations.LocalSearch2Opt(path, distances, weights);
            }

            PathGenerator.Verify(path, distances, weights);
        }
    }
}