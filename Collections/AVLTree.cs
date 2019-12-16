using System;
using System.Collections;
using System.Collections.Generic;

public class AVLTree<T> :
     IEnumerable<T>,
     IEnumerable,
     ICollection<T>,
     ICollection,
     IReadOnlyCollection<T>
{
    private class Node
    {
        public Node left;
        public Node right;
        public Node parent;
        public int height;
        public int frequency;
        public T val;

        public Node(T val)
        {
            this.val = val;
            parent = right = left = null;
            height = 0;
            frequency = 1;
        }

        public Node Any() => left ?? right;
        public bool IsFull() => left != null && right != null;
        public bool IsLeaf() => left is null && right is null;
        public bool HasChildren() => left != null && right != null;

        public static implicit operator bool(Node node) => node != null;
    }
    public bool IsSynchronized => false;
    public bool IsReadOnly => false;
    public object SyncRoot
    {
        get
        {
            if (_syncRoot == null)
            {
                System.Threading.Interlocked.CompareExchange(ref _syncRoot, new object(), 0);
            }
            return _syncRoot;
        }
    }

    public IComparer<T> Comparer => comparer;
    public int Count { get; private set; }

    private Node root;
    private object _syncRoot;
    private IComparer<T> comparer;


    public AVLTree()
    {
        comparer = Comparer<T>.Default;
    }

    public AVLTree(IComparer<T> comparer)
    {
        this.comparer = comparer ?? Comparer<T>.Default;
    }

    private int Height(Node node) => node == null ? -1 : node.height;
    private void UpdateHeight(Node node)
    {
        while (node)
        {
            node.height = 1 + Math.Max(Height(node.left), Height(node.right));
            node = node.parent;
        }
    }
    private int BalanceFactor(Node node) => Height(node.left) - Height(node.right);

    private void Balance(Node node, bool isLeft)
    {
        while (node)
        {
            var bf = BalanceFactor(node);
            var old = node;
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

    private Node RotateLeft(Node node)
    {
        var other = node.right;

        if (other)
        {
            node.right = other.left;
            node.height = 1 + Math.Max(Height(node.left), Height(node.right));

            if (other.left) other.left.parent = node;
            other.parent = node.parent;
            other.left = node;
        }

        if (!node.parent) root = other;
        else if (node == node.parent.left) node.parent.left = other;
        else node.parent.right = other;

        node.parent = other;

        return other;
    }

    private Node RotateRight(Node node)
    {
        var other = node.left;

        if (other)
        {
            node.left = other.right;
            node.height = 1 + Math.Max(Height(node.left), Height(node.right));

            if (other.right) other.right.parent = node;
            other.parent = node.parent;
            other.right = node;
        }


        if (!node.parent) root = other;
        else if (node == node.parent.left) node.parent.left = other;
        else node.parent.right = other;

        node.parent = other;
        return other;
    }

    public void Add(T val)
    {
        var node = new Node(val);

        if (root == null)
        {
            root = node;
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
                    current.right = node;
                    break;
                }
                current = current.right;
            }
            else if (cr < 0)
            {
                if (!current.left)
                {
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

        node.parent = current;
        Count++;
        UpdateHeight(current);
        Balance(current, node == current.left);
    }
    public bool Remove(T val)
    {
        var node = Get(val);

        if (!node)
            return false;

        if (--Count == 0)
        {
            root = null;
            return true;
        }
        if (--node.frequency > 0)
            return true;


        if (node.IsFull())
        {
            var min = GetMin(node.right);
            node.val = min.val;
            node.frequency = min.frequency;
            node = min;
        }

        if (node.IsLeaf())
        {
            if (node == node.parent.left)
            {
                node.parent.left = null;
            }
            else
            {
                node.parent.right = null;
            }
            UpdateHeight(node.parent);
        }
        else
        {
            var child = node.Any();
            if (node == root)
            {
                root = child;
                child.parent = null;
                UpdateHeight(root);
                return true;
            }
            var isLeft = node == node.parent.left;
            child.parent = node.parent;
            if (isLeft)
            {
                node.parent.left = child;
            }
            else
            {
                node.parent.right = child;
            }
            UpdateHeight(node.parent);
            Balance(node.parent, !isLeft);
        }

        return true;
    }

    public bool Contains(T val)
    {
        return Get(val);
    }

    private Node Get(T val)
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
        while (node.left)
        {
            node = node.left;
        }
        return node;
    }
    private Node GetMax(Node node)
    {
        while (node.right)
        {
            node = node.right;
        }
        return node;
    }

    public T GetMin()
    {
        if (root is null)
            throw new InvalidOperationException();
        return GetMin(root).val;
    }

    public T GetMax()
    {
        if (root is null)
            throw new InvalidOperationException();
        return GetMax(root).val;
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

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException();
        if (arrayIndex < 0 || arrayIndex >= array.Length)
            throw new InvalidOperationException();
        if (array.Length - arrayIndex < Count)
            throw new InvalidOperationException();
        foreach (var item in this)
        {
            array[arrayIndex++] = item;
        }
    }

    public void Clear()
    {
        root = null;
        Count = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new InorderEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private class InorderEnumerator
        : IEnumerator<T>,
          IEnumerator,
          IEnumerable<T>,
          IEnumerable
    {
        private readonly AVLTree<T> avlTree;
        private readonly Stack<Node> stack;
        private readonly int threadId;
        private Node root;
        private int freq;
        public InorderEnumerator(AVLTree<T> avlTree)
        {

            this.avlTree = avlTree;
            root = avlTree.root;
            stack = new Stack<Node>();
            threadId = Environment.CurrentManagedThreadId;
            freq = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (threadId != Environment.CurrentManagedThreadId)
                return new InorderEnumerator(avlTree);
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public T Current { get; private set; }
        object IEnumerator.Current => Current;

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
            if (stack.Count == 0) return false;
            root = stack.Pop();
            Current = root.val;
            freq = root.frequency;
            root = root.right;
            return true;
        }

        public void Reset()
        {
            root = avlTree.root;
        }

        public void Dispose()
        {
        }
    }
}
