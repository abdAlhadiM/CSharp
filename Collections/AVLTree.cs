using System;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.Serialization;

internal static class SerializationStreamExtensions
{
    public static void AddValue<T>(this SerializationInfo @this, string name, T val)
    {
        @this.AddValue(name, val, typeof(T));
    }
}

public interface INode<T>
{
    INode<T> Left { get; }
    INode<T> Right { get; }
    INode<T> Parent { get; }
    int Height { get; }
    int Frequency { get; }
    T Val { get; }
}

[Serializable]
public class AVLTree<T> : IReadOnlyCollection<T>,
                          ICollection,
                          ICollection<T>,
                          IEnumerable<T>,
                          IEnumerable,
                          ISerializable
{

    private class Node : INode<T>
    {
        public Node left;
        public Node right;
        public Node parent;
        public int height;
        public int frequency;
        public T val;

        public int Height => height;
        public int Frequency => frequency;
        public T Val => val;

        public INode<T> Left => left;
        public INode<T> Right => right;
        public INode<T> Parent => parent;
         
        public Node() { }
        public Node(T val, Node parent = null)
        {
            this.val = val;
            this.parent = parent;
            height = 0;
            frequency = 1;
            left = right = null;
        }


        public Node Any() => left ?? right;
        public bool IsFull() => left != null && right != null;
        public bool IsLeaf() => left is null && right is null;
        public bool HasChildren() => left != null || right != null;

        public static implicit operator bool(Node node) => node != null;
    }

    [NonSerialized]
    private object _syncRoot;
    public bool IsReadOnly => false;
    public bool IsSynchronized => false;
    public object SyncRoot
    {
        get
        {
            if (_syncRoot == null)
            {
                _syncRoot = System.Threading.Interlocked.CompareExchange(ref _syncRoot, new object(), null);
            }
            return _syncRoot;
        }
    }
    private readonly IComparer<T> comparer;
    private Node root;
    public int Count { get; private set; }

    public AVLTree()
    {
        comparer = Comparer<T>.Default;
    }
    public AVLTree(IComparer<T> comparer)
    {
        this.comparer = comparer ?? Comparer<T>.Default;
    }

    public AVLTree(IEnumerable<T> collection) : this(collection, Comparer<T>.Default) { }

    public AVLTree(IEnumerable<T> collection, IComparer<T> comparer) : this(comparer)
    {
        if (collection is null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        if (collection is AVLTree<T> other && comparer.Equals(other.comparer))
        {
            if (other.Count == 0)
            {
                root = null;
                Count = 0;
            }
        }
        else
        {
            Dictionary<T, int> map = new Dictionary<T, int>();
            foreach (var item in collection)
            {
                if (!map.ContainsKey(item))
                {
                    map.Add(item, 1);
                }
                else
                {
                    map[item]++;
                }
            }
            var freq = map.Values.ToArray();
            var keys = map.Keys.ToArray();
            Array.Sort(keys);
            root = BuildFromSortedArray(keys, freq, 0, keys.Length - 1, null);
            Count = keys.Length;
        }
    }

    private Node BuildFromSortedArray(T[] values, int[] freq, int l, int r, Node parent)
    {
        if (l > r) return null;

        int mid = l + (r - l) / 2;

        var current = new Node(values[mid], parent);
        current.left = BuildFromSortedArray(values, freq, l, mid - 1, current);
        current.right = BuildFromSortedArray(values, freq, mid + 1, r, current);
        current.height = 1 + Math.Max(Height(current.left), Height(current.right));
        current.frequency = freq[mid];
        return current;
    }

    public void Add(T val)
    {
        if (root == null)
        {
            root = new Node(val);
            Count++;
            return;
        }

        var current = root;

        while (true)
        {
            var cr = comparer.Compare(val, current.val);
            if (cr > 0)
            {
                if (!current.right)
                {
                    current.right = new Node(val, parent: current);
                    break;
                }
                current = current.right;
            }
            else if (cr < 0)
            {
                if (!current.left)
                {
                    current.left = new Node(val, parent: current);
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
        Balance(current);
    }

    private void Balance(Node node)
    {
        while (node)
        {
            node.height = 1 + Math.Max(Height(node.left), Height(node.right));
            var bf = BalanceFactor(node);

            //L
            if (bf < -1)
            {
                //LL
                if (BalanceFactor(node.left) <= 0)
                {
                    node = RotateRight(node);
                }
                //LR
                else
                {
                    node = RotateLeft(node.left);
                    node = RotateRight(node.parent);
                }
            }
            //R
            else if (bf > 1)
            {
                //RR
                if (BalanceFactor(node.right) >= 0)
                {
                    node = RotateLeft(node);
                }
                //RL
                else
                {
                    node = RotateRight(node.right);
                    node = RotateLeft(node.parent);
                }
            }


            node = node.parent;
        }
    }

    private Node RotateLeft(Node node)
    {
        var other = node.right;

        Debug.Assert(other != null);

        node.right = other.left;
        node.height = 1 + Math.Max(Height(node.left), Height(node.right));

        if (other.left) other.left.parent = node;

        other.parent = node.parent;
        other.left = node;
        other.height = 1 + Math.Max(Height(other.left), Height(other.right));


        if (!node.parent) root = other;
        else if (node == node.parent.left) node.parent.left = other;
        else node.parent.right = other;

        node.parent = other;
        return other;
    }

    private Node RotateRight(Node node)
    {
        var other = node.left;

        Debug.Assert(other != null);

        node.left = other.right;
        node.height = 1 + Math.Max(Height(node.left), Height(node.right));

        if (other.right) other.right.parent = node;

        other.parent = node.parent;
        other.right = node;
        other.height = 1 + Math.Max(Height(other.left), Height(other.right));


        if (!node.parent) root = other;
        else if (node == node.parent.left) node.parent.left = other;
        else node.parent.right = other;

        node.parent = other;
        return other;
    }

    private int Height(Node node) => node is null ? -1 : node.height;
    private int BalanceFactor(Node node) => Height(node.right) - Height(node.left);

    public bool Contains(T val)
    {
        return GetNode(val);
    }

    public bool Remove(T val)
    {
        var node = GetNode(val);

        if (!node) return false;

        Count--;

        if (--node.frequency > 0) return true;

        if (node.IsFull())
        {
            var min = GetMin(node.right);
            node.val = min.val;
            node.frequency = min.frequency;
            node = min;
        }
        else if (node == root)
        {
            if (node.IsLeaf()) root = null;
            else
            {
                var child = node.Any();
                child.parent = null;
                root = child;
            }
            return true;
        }

        RemoveInternal(node);
        Balance(node.parent);
        return true;
    }

    private void RemoveInternal(Node node)
    {
        var isLeft = node == node.parent.left;
        if (node.IsLeaf())
        {
            if (isLeft)
            {
                node.parent.left = null;
            }
            else
            {
                node.parent.right = null;
            }
        }
        else
        {
            var child = node.Any();
            child.parent = node.parent;
            if (isLeft)
            {
                node.parent.left = child;

            }
            else
            {
                node.parent.right = child;
            }
        }
    }

    public T Max()
    {
        if (!root) throw new InvalidOperationException();
        return GetMax(root).val;
    }

    public T Min()
    {
        if (!root) throw new InvalidOperationException();
        return GetMin(root).val;
    }

    private Node GetNode(T val)
    {
        var current = root;
        while (current)
        {
            var cr = comparer.Compare(val, current.val);
            if (cr == 0) break;
            current = cr > 0 ? current.right :
                               current.left;
        }
        return current;
    }

    private Node GetMin(Node node)
    {
        Debug.Assert(node != null);
        while (node.left)
        {
            node = node.left;
        }
        return node;
    }

    private Node GetMax(Node node)
    {
        Debug.Assert(node != null);
        while (node.right)
        {
            node = node.right;
        }
        return node;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array is null)
        {
            throw new ArgumentNullException(nameof(array));
        }
        if (arrayIndex < 0 || arrayIndex >= array.Length)
        {
            throw new InvalidOperationException(nameof(arrayIndex));
        }
        if (array.Length - arrayIndex < Count)
        {
            throw new InvalidOperationException();
        }

        foreach (var item in this)
        {
            array[arrayIndex++] = item;
        }
    }

    public void CopyTo(Array array, int index)
    {
        if (array != null && array.Rank != 1)
        {
            throw new InvalidOperationException();
        }

        if (!(array is T[] arr))
        {
            throw new ArrayTypeMismatchException();
        }

        CopyTo(arr, index);
    }

    public void Clear()
    {
        root = null;
        Count = 0;
    }
     
    public IEnumerable<IEnumerable<T>> Levels() => Levels((node) => node.Val);

    public IEnumerable<IEnumerable<V>> Levels<V>(Func<INode<T>, V> query)
    {
        if (!root) yield break;
        var queue = new Queue<Node>();
        queue.Enqueue(root);
        while (queue.Count != 0)
        {
            var level = new List<V>();
            var n = queue.Count;
            while (n-- > 0)
            {
                var current = queue.Dequeue();
                if (current.left) queue.Enqueue(current.left);
                if (current.right) queue.Enqueue(current.right);
                level.Add(query(current));
            }
            yield return level;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new InorderEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected AVLTree(SerializationInfo info, StreamingContext context)
    {
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
    }

    private struct InorderEnumerator : IEnumerator<T>,
                                       IEnumerator,
                                       IEnumerable<T>,
                                       IEnumerable
    {
        private AVLTree<T> avlTree;
        private Stack<Node> stack;
        private readonly int ThreadId;
        private Node root;
        private int freq;
        public InorderEnumerator(AVLTree<T> avlTree)
        {
            Debug.Assert(avlTree != null);
            this.avlTree = avlTree;
            stack = new Stack<Node>((int)Math.Log2(avlTree.Count + 1));
            ThreadId = Environment.CurrentManagedThreadId;
            root = avlTree.root;
            freq = 0;
            Current = default;
        }

        public T Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            avlTree = null;
            stack = null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (ThreadId != Environment.CurrentManagedThreadId)
                return new InorderEnumerator(avlTree);
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool MoveNext()
        {
            if (--freq > 0)
            {
                return true;
            }

            while (root)
            {
                stack.Push(root);
                root = root.left;
            }

            if (stack.Count == 0)
                return false;

            root = stack.Pop();
            Current = root.val;
            freq = root.frequency;
            root = root.right;
            return true;
        }

        public void Reset()
        {
            root = avlTree.root;
            stack.Clear();
            freq = 0;
        }
    }
}
