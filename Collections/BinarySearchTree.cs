using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BST
{
    static class SerializationInfoExtensions
    {
        public static void AddValue<T>(this SerializationInfo @this, string name, object value)
        {
            @this.AddValue(name, value, typeof(T));
        }
    }

    [Serializable]
    class BinarySearchTree
        : IEnumerable<int>,
          IEnumerable,
          ISerializable,
          IReadOnlyCollection<int>,
          ICollection<int>,
          ICollection
    {

        private class Node
        {
            public Node left;
            public Node right;
            public int val;

            public Node(int val)
            {
                this.val = val;
                left = right = null;
            }

            public Node GetAny() => left ?? right;
            public bool IsLeaf() => left is null && right is null;
            public bool IsFull() => left != null && right != null;
            public bool HasChildren() => left != null || right != null;

            public static implicit operator bool(Node node) => node != null;
        }

        private static readonly string InfoName = "tree";
        private static readonly char Space = ' ';

        private Node root;
        private object _syncRoot;

        public int Count { get; private set; }
        public bool IsReadOnly => false;
        public bool IsSynchronized => false;
        public object SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
                }
                return _syncRoot;
            }
        }

        public BinarySearchTree() { }

        void ICollection<int>.Add(int item) => Add(item);
        public bool Add(int val)
        {
            ref var current = ref root;

            while (current)
            {
                if (val == current.val) return false;

                current = ref val > current.val ?
                                        ref current.right :
                                        ref current.left;
            }

            current = new Node(val);
            Count++;
            return true;
        }

        public bool Contains(int val)
        {
            return Get(ref root, val);
        }

        public bool Remove(int val)
        {
            ref var current = ref Get(ref root, val);
            if (current)
            {
                if (current.IsFull())
                {
                    var other = current;
                    current = GetMin(ref root);
                    other.val = current.val;
                }

                if (current.HasChildren())
                {
                    current = current.GetAny();
                }
                else
                {
                    current = null;
                }

                Count--;
                return true;
            }
            return false;
        }

        public int GetMin()
        {
            var node = GetMin(ref root);
            if (node is null)
                throw new InvalidOperationException();
            return node.val;
        }

        private ref Node GetMin(ref Node root)
        {
            ref var current = ref root;
            while (current.left)
            {
                current = current.left;
            }
            return ref current;
        }

        public int GetMax()
        {
            var node = GetMax(ref root);
            if (node is null)
                throw new InvalidOperationException();
            return node.val;
        }

        private ref Node GetMax(ref Node root)
        {
            ref var current = ref root;
            while (current.right)
            {
                current = current.right;
            }
            return ref current;
        }

        private ref Node Get(ref Node root, int val)
        {
            ref var current = ref root;
            while (current && val != current.val)
            {
                current = ref val > current.val ?
                                    ref current.right :
                                    ref current.left;
            }
            return ref current;
        }

        public IEnumerable<IEnumerable<int>> Levels()
        {
            return new LevelEnumerator(this);
        }

        public IEnumerable<IEnumerable<int>> Paths()
        {
            return new PathEnumerator(this);
        }

        public void Clear()
        {
            root = null;
            Count = 0;
        }

        public void CopyTo(int[] array, int arrayIndex)
        {
            if (array is null)
                throw new ArgumentNullException();
            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new IndexOutOfRangeException();
            if (array.Length - arrayIndex < Count)
                throw new InvalidOperationException();

            foreach (var item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        public void CopyTo(Array array, int index)
        {
            if (array != null && array.Rank != 1)
                throw new InvalidOperationException();

            if (!(array is int[] arr))
                throw new ArrayTypeMismatchException();
            CopyTo(arr, index);

        }

        protected BinarySearchTree(SerializationInfo info, StreamingContext context)
        {
            var tree = info.GetString(InfoName);

            var list = tree.Split(Space)
                           .Select(val => int.Parse(val))
                           .ToArray();
            int i = 0;
            root = Build(list, ref i, null, null);
        }

        private Node Build(int[] nums, ref int index, int? min, int? max)
        {
            if (index >= nums.Length)
                return null;

            int val = nums[index];

            if (min.HasValue && val < min) return null;

            if (max.HasValue && val > max) return null;

            index++;
            return new Node(val)
            {
                left = Build(nums, ref index, min, val),
                right = Build(nums, ref index, val, max)
            };
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var builder = new StringBuilder();
            PreorderDataQuery(root, builder);
            builder.Remove(builder.Length - 1, 1);
            info.AddValue<string>(InfoName, builder.ToString());
        }

        private void PreorderDataQuery(Node root, StringBuilder builder)
        {
            if (root is null)
                return;
            builder.Append(root.val)
                   .Append(Space);
            PreorderDataQuery(root.left, builder);
            PreorderDataQuery(root.right, builder);
        }

        public IEnumerator<int> GetEnumerator() => new InorderEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public class PathEnumerator :
            IEnumerator<IEnumerable<int>>,
            IEnumerable<IEnumerable<int>>,
            IEnumerator,
            IEnumerable
        {
            private readonly BinarySearchTree binarySearchTree;
            private readonly int threadId;
            private readonly Stack<Node> stack;
            private readonly Node root;
            private readonly List<int> container;
            public PathEnumerator(BinarySearchTree binarySearchTree)
            {
                this.binarySearchTree = binarySearchTree;
                threadId = Environment.CurrentManagedThreadId;
                stack = new Stack<Node>();
                container = new List<int>();
                root = binarySearchTree.root;
                if (root != null) stack.Push(root);
            }

            public IEnumerator<IEnumerable<int>> GetEnumerator()
            {
                if (threadId != Environment.CurrentManagedThreadId)
                {
                    return new PathEnumerator(binarySearchTree);
                }
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerable<int> Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (stack.Count != 0)
                {
                    while (stack.Count != 0)
                    {
                        var current = stack.Pop();
                        if (!current)
                        {
                            container.RemoveAt(container.Count - 1);
                        }
                        else if (current.IsLeaf())
                        {
                            Current = new List<int>(container) { current.val };
                            return true;
                        }
                        else
                        {
                            container.Add(current.val);
                            stack.Push(null);
                            if (current.right) stack.Push(current.right);
                            if (current.left) stack.Push(current.left);
                        }
                    }
                }
                return false;
            }

            public void Reset()
            {
            }
        }
        public class LevelEnumerator :
            IEnumerator<IEnumerable<int>>,
            IEnumerable<IEnumerable<int>>,
            IEnumerator,
            IEnumerable
        {
            private readonly BinarySearchTree binarySearchTree;
            private readonly int threadId;
            private readonly Queue<Node> queue;
            private readonly Node root;

            public LevelEnumerator(BinarySearchTree binarySearchTree)
            {
                this.binarySearchTree = binarySearchTree;
                threadId = Environment.CurrentManagedThreadId;
                queue = new Queue<Node>();
                root = binarySearchTree.root;
                if (root != null) queue.Enqueue(root);
            }
            public IEnumerator<IEnumerable<int>> GetEnumerator()
            {
                if (threadId != Environment.CurrentManagedThreadId)
                {
                    return new LevelEnumerator(binarySearchTree);
                }
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerable<int> Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (queue.Count == 0) return false;

                var n = queue.Count;
                int[] level = new int[n];

                for (int i = 0; i < n; i++)
                {
                    var current = queue.Dequeue();
                    if (current.left) queue.Enqueue(current.left);
                    if (current.right) queue.Enqueue(current.right);
                    level[i] = current.val;
                }

                Current = level;
                return true;
            }

            public void Reset()
            {
            }

        }


        public class InorderEnumerator :
            IEnumerable<int>,
            IEnumerable,
            IEnumerator<int>,
            IEnumerator
        {
            private readonly BinarySearchTree binarySearchTree;
            private readonly Stack<Node> stack;
            private readonly int threadId;
            private Node root;
            public InorderEnumerator(BinarySearchTree binarySearchTree)
            {
                this.binarySearchTree = binarySearchTree ?? throw new ArgumentNullException(nameof(binarySearchTree));
                stack = new Stack<Node>();
                threadId = Environment.CurrentManagedThreadId;
                root = binarySearchTree.root;
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
                while (root)
                {
                    stack.Push(root);
                    root = root.left;
                }
                if (stack.Count == 0) return false;

                root = stack.Pop();
                Current = root.val;
                root = root.right;

                return true;
            }

            public void Reset()
            {
            }
        }
    }
}
