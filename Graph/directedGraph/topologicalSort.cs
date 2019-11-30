using System;
using System.Collections.Generic;

namespace T
{
    class Graph
    {
        private readonly IList<int>[] adjList;
        public Graph(int n)
        {
            adjList = new IList<int>[n];
            for(int i = 0; i < adjList.Length; i++)
            {
                adjList[i] = new List<int>();
            }
        }

        public void AddEdge(int u, int v) => adjList[u].Add(v);
        public int V => adjList.Length;
        public IEnumerable<int> Adj(int i) => adjList[i];
    }

    class Topsort
    {
        private readonly Graph graph;

        public Topsort(Graph graph)
        {
            this.graph = graph;

        }

        public IEnumerable<int> Find()
        {
            var result = new Stack<int>();
            var visited = new bool[graph.V];

            for (int i = 0; i < graph.V; i++)
            {
                if (!visited[i])
                {
                    DFS(i, visited, result);
                }
            }

            return result;
        }

        private void DFS(int u, bool[] visited, Stack<int> result)
        {
            visited[u] = true;

            foreach (var v in graph.Adj(u))
            {
                if (!visited[v])
                {
                    DFS(v, visited, result);
                }
            }
            
            result.Push(u);
        }
    }


    class Program
    {
        static void Main(string[] _)
        {
            var graph = new Graph(8);

            graph.AddEdge(1, 2);
            graph.AddEdge(2, 6);
            graph.AddEdge(1, 3);
            graph.AddEdge(3, 4);
            graph.AddEdge(4, 5);
            graph.AddEdge(4, 7);
            graph.AddEdge(5, 7);
            graph.AddEdge(6, 4);


            foreach (var item in new Topsort(graph).Find())
            {
                Console.Write(item + " ");
            }
        }
    }
}
