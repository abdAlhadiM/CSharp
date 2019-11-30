using System;
using System.Collections.Generic;

namespace CC
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

    class ConnectedComponent
    {
        private readonly Graph graph;
        public ConnectedComponent(Graph graph)
        {
            this.graph = graph;
        }

        public IEnumerable<IEnumerable<int>> Find()
        {
            var result = new List<IList<int>>();
            var visited = new bool[graph.V];

            for (int i = 0; i < graph.V; i++)
            {
                if (!visited[i])
                {
                    var component = new List<int>();
                    DFS(i, visited, component);
                    result.Add(component);
                }
            }

            return result;
        }

        private void DFS(int u, bool[] visited, IList<int> component)
        {
            visited[u] = true;
            component.Add(u);

            foreach (var v in graph.Adj(u))
            {
                if (!visited[v])
                {
                    DFS(v, visited, component);
                }
            }
        }
    }
    
    class Program
    {
        static void Main(string[] _)
        {
            var graph = new Graph(7);

            graph.AddEdge(0, 0);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 4);
            graph.AddEdge(5, 6);
 

            foreach(var component in new ConnectedComponent(graph).Find())
            {
                foreach(var item in component)
                {
                    Console.Write(item + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
