using System;
using System.Collections;
using System.Collections.Generic;

namespace AVL
{
    class AVLTree : IEnumerable<int>
    {
        private class Node
        {
            public Node left;
            public Node right;
            public Node parent;
            public int val;
            public int frequency;
            public int height;
            public Node() { }
            public Node(int val) : this(val, 0) { }

            public Node(int val, int height)
            {
                this.val = val;
                this.height = height;
                left = right = null;
                frequency = 1;
            }

            public Node Any() => left ?? right;
            public bool IsLeaf() => left is null && right is null;
            public bool IsFull() => left != null && right != null;
            public bool HasChildren() => left != null && right != null;

            public static implicit operator bool(Node node) => node != null;
        }

        private Node root;

        public int Count { get; private set; }

        public void Add(int val)
        {
            var node = new Node(val);

            if (root is null)
            {
                root = node;
                return;
            }

            var current = root;
            while (true)
            {
                if (val > current.val)
                {
                    if (!current.right)
                    {
                        node.parent = current;
                        current.right = node;
                        break;
                    }
                    current = current.right;
                }
                else if (val < current.val)
                {
                    if (!current.left)
                    {
                        node.parent = current;
                        current.left = node;
                        break;
                    }
                    current = current.left;
                }
                else
                {
                    current.frequency++;
                    Count++;
                    return;
                }
            }
            Count++;
            UpdateHeight(current);
            Balance(current, node == current.left);
        }

        private int GetHeight(Node node) => node is null ? -1 : node.height;
        private int BalanceFactor(Node node) => GetHeight(node.left) - GetHeight(node.right);
        private void UpdateHeight(Node node)
        {
            while (node)
            {
                node.height = 1 + Math.Max(GetHeight(node.left), GetHeight(node.right));
                node = node.parent;
            }
        }

        private void Balance(Node node, bool isLeft)
        {
            while (node)
            {
                var old = node;
                var bf = BalanceFactor(node);
                //case right heavy
                if (bf < -1)
                {
                    //RL
                    if (isLeft)
                    {
                        node = RotateRight(node.right);
                        UpdateHeight(node);
                        node = RotateLeft(node.parent);
                        UpdateHeight(node);
                    }
                    //RR
                    else
                    {
                        node = RotateLeft(node);
                        UpdateHeight(node);
                    }
                }
                //case left heavy
                else if (bf > 1)
                {
                    //LL
                    if (isLeft)
                    {
                        node = RotateRight(node);
                        UpdateHeight(node);
                    }
                    //LR
                    else
                    {
                        node = RotateLeft(node.left);
                        UpdateHeight(node);
                        node = RotateRight(node.parent);
                        UpdateHeight(node);
                    }
                }
                if (old.parent) isLeft = old == old.parent.left;
                node = node.parent;
            }
        }

        private Node RotateRight(Node node)
        {
            var other = node.left;
            if (other)
            {

                node.left = other.right;
                node.height = 1 + Math.Max(GetHeight(node.left), GetHeight(node.right));

                if (other.right) other.right.parent = node;

                other.parent = node.parent;
                other.right = node;

            }

            if (!node.parent) root = other;
            else if (node == node.parent.left)
                node.parent.left = other;
            else node.parent.right = other;

            node.parent = other;
            return other;
        }

        private Node RotateLeft(Node node)
        {
            var other = node.right;

            if (other)
            {
                node.right = other.left;
                node.height = 1 + Math.Max(GetHeight(node.left), GetHeight(node.right));

                if (other.left) other.left.parent = node;

                other.parent = node.parent;
                other.left = node;
            }

            if (!node.parent) root = other;
            else if (node == node.parent.left)
                node.parent.left = other;
            else node.parent.right = other;


            node.parent = other;
            return other;
        }

        public bool Contains(int val)
        {
            return Get(val);
        }

        private Node Get(int val)
        {
            var current = root;
            while (current && val != current.val)
            {
                current = val > current.val ?
                                    current.right :
                                    current.left;
            }
            return current;
        }

        public int GetMin()
        {
            var node = GetMin(root);
            if (node is null)
                throw new InvalidOperationException();
            return node.val;
        }
        private Node GetMin(Node node)
        { 
            while (node.left)
            {
                node = node.left;
            }
            return node;
        }

        public bool Remove(int val)
        {
            var node = Get(val);
          
            if (node)
            {
                if (--node.frequency > 0)
                {
                    return true;
                }

                if (node.IsFull())
                {
                    var min = GetMin(node.right);
                    node.val = min.val;
                    node.frequency = min.frequency;
                    node = min;
                }

                if (node.IsLeaf())
                {
                    if (node.parent.left == node)
                    {
                        node.parent.left = null;
                    }
                    else
                        node.parent.right = null;
                }
                else
                {
                    var child = node.Any();
                    node.parent = child;
                    child.parent = node.parent;

                }
                node.parent.height = 1 + Math.Max(GetHeight(node.parent.left), GetHeight(node.parent.right));
                Count--;
                return true;
            }
            return false;
        }

        public IEnumerator<int> GetEnumerator()
        {
            return new InorderEnumerator(this);
        }
         
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable<IEnumerable<int>> Levels()
        {
            var queue = new Queue<Node>();
            queue.Enqueue(root);
            while (queue.Count != 0)
            {
                var level = new List<int>();
                var n = queue.Count;
                while (n-- > 0)
                {
                    var current = queue.Dequeue();
                    if (current.left) queue.Enqueue(current.left);
                    if (current.right) queue.Enqueue(current.right);
                    level.Add(current.val);
                }
                yield return level;
            }
        }

        public class InorderEnumerator :
                        IEnumerable<int>,
                        IEnumerable,
                        IEnumerator<int>,
                        IEnumerator
        {
            private readonly AVLTree binarySearchTree;
            private readonly Stack<Node> stack;
            private readonly int threadId;
            private Node root;
            private int freq;
            public InorderEnumerator(AVLTree binarySearchTree)
            {
                this.binarySearchTree = binarySearchTree ?? throw new ArgumentNullException(nameof(binarySearchTree));
                stack = new Stack<Node>();
                threadId = Environment.CurrentManagedThreadId;
                root = binarySearchTree.root;
                freq = 0;
            }


            public IEnumerator<int> GetEnumerator()
            {
                if (Environment.CurrentManagedThreadId != threadId)
                    return new InorderEnumerator(binarySearchTree);
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (freq > 0)
                {
                    freq--;
                    return true;
                }
                while (root)
                {
                    stack.Push(root);
                    root = root.left;
                }
                if (stack.Count == 0) return false;

                root = stack.Pop();
                Current = root.val;
                freq = root.frequency - 1;
                root = root.right;

                return true;
            }

            public void Reset()
            {
                
            }
        }
    }
}
