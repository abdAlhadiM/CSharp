using System;
using System.Collections.Generic;
using System.Linq;

namespace S
{
    class Graph
    {
        private readonly IList<int>[] adjList;

        public Graph(int n)
        {
            adjList = new IList<int>[n];
            for (int i = 0; i < n; i++)
                adjList[i] = new List<int>();
        }

        public void AddEdge(int u, int v)
        {
            adjList[u].Add(v);
            adjList[v].Add(u);
        }

        public int V => adjList.Length;
        public IEnumerable<int> Adj(int i) => adjList[i];
    }

    class ShortestPaths
    {
        private readonly Graph graph;
        private readonly int src;
        private readonly int[] parent;
        public ShortestPaths(Graph graph, int src)
        {
            this.graph = graph;
            this.src = src;
            parent = new int[graph.V];

            BFS();
        }

        private void BFS()
        {
            var queue = new Queue<int>(graph.V);
            var visited = new bool[graph.V];

            queue.Enqueue(src);
            visited[src] = true;
            parent[src] = -1;

            while (queue.Count != 0)
            {
                var u = queue.Dequeue();
                foreach (var v in graph.Adj(u))
                {
                    if (!visited[v])
                    {
                        visited[v] = true;
                        queue.Enqueue(v);
                        parent[v] = u;
                    }
                }
            }
        }

        public IEnumerable<int> Find(int target)
        {
            if (parent[target] == 0)
                return Enumerable.Empty<int>();

            var result = new Stack<int>();

            for (int i = target; i != src; i = parent[i])
            {
                result.Push(i);
            }

            result.Push(src);

            return result;
        }
    }

    class Program
    {
        static void Main(string[] _)
        {
            var graph = new Graph(6);

            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 4);
            graph.AddEdge(4, 1);
            graph.AddEdge(4, 5);

            foreach (var item in new ShortestPaths(graph, 0).Find(5))
            {
                Console.Write(item + " ");
            }

        }
    }
}
