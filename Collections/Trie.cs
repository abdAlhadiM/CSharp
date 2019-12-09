using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace T
{
    class Trie : IEnumerable<string>
    {
        class Node : Dictionary<char, Node>
        {
            public bool isEndOfWord;
        }

        private readonly Node root;

        public Trie()
        {
            root = new Node();
        }

        public void Add(string word)
        {
            var current = root;
            foreach (var key in word)
            {
                if (!current.ContainsKey(key))
                {
                    current[key] = new Node();
                }
                current = current[key];
            }
            current.isEndOfWord = true;
        }

        public bool Contains(string word)
        {
            var current = root;
            foreach (var key in word)
            {
                if (!current.ContainsKey(key)) return false;
                current = current[key];
            }
            return current.isEndOfWord;
        }

        public IEnumerator<string> GetEnumerator()
        {
            var set = new List<string>();
            var builder = new StringBuilder();
            foreach (var (key, value) in root)
            {
                DFS(key, value, builder, set);
            }
            return set.GetEnumerator();
        }


        private void DFS(char c, Node root, StringBuilder builder, IList<string> set)
        {
            builder.Append(c);

            if (root.isEndOfWord)
            {
                set.Add(builder.ToString());
            }

            foreach (var (key, value) in root)
            {
                DFS(key, value, builder, set);
            }

            builder.Remove(builder.Length - 1, 1);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }      
}
