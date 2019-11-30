using System;
using System.Collections.Generic;

namespace C
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


    class Cycle
    {
        private readonly Graph graph;

        public Cycle(Graph graph)
        {
            this.graph = graph;
        }

        public bool Has()
        {
            var visited = new bool[graph.V];

            for (int i = 0; i < graph.V; i++)
            {
                if (!visited[i] && DFS(i, -1, visited))
                    return true;
            }

            return false;
        }

        private bool DFS(int u, int p, bool[] visited)
        {
            visited[u] = true;

            foreach (var v in graph.Adj(u))
            {
                if (v == p) continue;
                if (visited[v]) return true;
                if (DFS(v, u, visited)) return true;
            }
            return false;
        }
    }


    class Program
    {
        static void Main(string[] _)
        {
            var graph = new Graph(9);

            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 4);
             
            graph.AddEdge(5, 6);
            graph.AddEdge(6, 5);

            graph.AddEdge(7, 8);

            Console.WriteLine(new Cycle(graph).Has());
        }
    }
}
