using System;
using System.Collections.Generic;

namespace B
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

    class Bridges
    {
        private readonly Graph graph;
        private int[] low;
        private int[] disc;
        private bool[] visited;
        private int timer;

        public Bridges(Graph graph)
        {
            this.graph = graph;
        }

        public IEnumerable<int[]> Find()
        {
            var result = new List<int[]>();

            low = new int[graph.V];
            visited = new bool[graph.V];
            disc = new int[graph.V];
            timer = 0;


            for (int i = 0; i < graph.V; i++)
            {
                if (!visited[i])
                {
                    DFS(i, -1, result);
                }
            }

            return result;
        }

        private void DFS(int u, int p, List<int[]> result)
        {
            visited[u] = true;
            low[u] = disc[u] = timer++;

            foreach (var v in graph.Adj(u))
            {
                if (v == p) continue;
                if (visited[v])
                {
                    low[u] = Math.Min(low[u], disc[v]);
                }
                else
                {
                    DFS(v, u, result);
                    low[u] = Math.Min(low[u], low[v]);
                    if (low[v] > disc[u])
                    {
                        result.Add(new int[] { u, v });
                    }
                }
            }
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
            graph.AddEdge(3, 0);
            graph.AddEdge(3, 4);
            graph.AddEdge(4, 5);
            //graph.AddEdge(5, 2);

            foreach (var bridge in new Bridges(graph).Find())
            {
                Console.WriteLine(bridge[0] + " " + bridge[1]);
            }

        }
    }
}
