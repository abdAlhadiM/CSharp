public class TernarySearchTree
    {
        private class Node
        {
            public Node left;
            public Node middle;
            public Node right;
            public char key;
            public bool isLeaf;

            public Node() { }

            public Node(char key)
            {
                this.key = key;
            }

            public static implicit operator bool(Node node) => node != null;
        }

        private readonly Node root;

        public TernarySearchTree()
        {
            root = new Node();
        }

        public void Add(string word)
        {
            if (word is null)
            {
                throw new ArgumentNullException(nameof(word));
            }

            var i = 0;
            var key = word[i];
            var current = root;

            while (true)
            {
                if (key > current.key)
                {
                    if (!current.right)
                    {
                        current.right = new Node(key);
                    }
                    current = current.right;
                }
                else if (key < current.key)
                {
                    if (!current.left)
                    {
                        current.left = new Node(key);
                    }
                    current = current.left;
                }
                else
                {
                    if (!current.middle)
                    {
                        while (++i < word.Length)
                        {
                            current.middle = new Node(word[i]);
                            current = current.middle;
                        }
                        break;
                    }
                    if (++i == word.Length) break;
                    key = word[i];
                    current = current.middle;
                }
            }
            current.isLeaf = true;
        }

        private Node Get(string str)
        {
            var prev = root;
            var current = prev;

            foreach (var key in str)
            {
                while (current && key != current.key)
                {
                    prev = current;
                    current = key > current.key ?
                                        current.right :
                                        current.left;
                }

                if (current)
                {
                    prev = current;
                    current = current.middle;
                }
                else return null;
            }
            return prev;
        }

        public bool Contains(string word)
        {
            if (word is null)
            {
                throw new ArgumentNullException(nameof(word));
            }

            var node = Get(word);
            return node && node.isLeaf;
        }

        public bool StartsWith(string prefix)
        {
            if (prefix is null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            return Get(prefix);
        }
    }
