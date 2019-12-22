    public interface INode<out T>
    {
        INode<T> Left { get; }
        INode<T> Right { get; }
        INode<T> Parent { get; }
        T Val { get; }
        int Frequency { get; }
    }
    public class RedBlackTree<T>
        : IEnumerable<T>,
          IEnumerable,
          ICollection<T>,
          ICollection,
          IReadOnlyCollection<T>
    {
        private class Node : INode<T>
        {
            public Node left;
            public Node right;
            public Node parent;
            public int frequency;
            public T val;
            public bool color;

            INode<T> INode<T>.Left => left;

            INode<T> INode<T>.Right => right;

            INode<T> INode<T>.Parent => parent;

            T INode<T>.Val => val;

            int INode<T>.Frequency => frequency;

            public Node(T val, Node parent = null, bool color = Red)
            {
                this.val = val;
                this.parent = parent;
                this.color = color;
                frequency = 1;
                left = right = null;
            }
            public bool IsLeaf() => left is null && right is null;
            public bool IsFull() => left != null && right != null;
            public bool HasChildren() => left != null || right != null;
            public Node Any() => left ?? right;
            public Node Sibling()
            {
                Debug.Assert(parent != null);
                return this == parent.left ? parent.right : parent.left;
            }

            public Node Other()
            {
                Debug.Assert(parent != null);
                if (parent.left)
                {
                    return parent.right ?
                        this == parent.right ? parent.left : parent.right :
                        parent.left;
                }
                return parent.right;
            }

            public static implicit operator bool(Node node) => node != null;
        }

        private const bool Black = false;
        private const bool Red = true;

        private readonly IComparer<T> comparer;
        private object _syncRoot;
        private Node root;
        private int version;
        public int Count { get; private set; }

        public bool IsReadOnly => false;

        public bool IsSynchronized => false;

        public object SyncRoot
        {
            get
            {
                if (_syncRoot is null)
                {
                    System.Threading.Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                }
                return _syncRoot;
            }
        }

        public RedBlackTree()
        {
            comparer = Comparer<T>.Default;
        }

        public RedBlackTree(IComparer<T> comparer)
        {
            this.comparer = comparer ?? Comparer<T>.Default;
        }

        public RedBlackTree(IEnumerable<T> collection)
            : this(collection, Comparer<T>.Default) { }

        public RedBlackTree(IEnumerable<T> collection, IComparer<T> comparer)
            : this(comparer)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (collection is RedBlackTree<T> enumerable)
            {
                if (enumerable.Count == 0)
                {
                    root = null;
                    Count = 0;
                    version = 0;
                    return;
                }

            }
            else
            {
                var map = new Dictionary<T, int>();

                foreach (var item in collection)
                {
                    if (map.ContainsKey(item))
                    {
                        map[item]++;
                    }
                    else
                    {
                        map.Add(item, 1);
                    }
                    Count++;
                }

                var keys = map.Keys.ToArray();
                Array.Sort(keys, comparer);
                root = BuildFromSortedArray(keys, map, 0, keys.Length - 1, Black, null);
                version = 0;
            }
        }

        private Node BuildFromSortedArray(T[] keys, Dictionary<T, int> freqs, int l, int r, bool color, Node parent)
        {
            if (l > r) return null;

            var mid = l + (r - l) / 2;

            var node = new Node(keys[mid], parent: parent);
            node.left = BuildFromSortedArray(keys, freqs, l, mid - 1, !color, node);
            node.right = BuildFromSortedArray(keys, freqs, mid + 1, r, !color, node);
            node.parent = parent;
            node.frequency = freqs[node.val];
            return node;
        }

        public INode<T> this[T val] => GetNode(val);

        private Node GetNode(T val)
        {
            var current = root;
            while (current)
            {
                var cr = comparer.Compare(val, current.val);
                if (cr == 0) return current;
                current = cr > 0 ? current.right : current.left;
            }
            return current;
        }

        private Node GetMax(Node root)
        {
            Debug.Assert(root != null);

            while (root.right)
            {
                root = root.right;
            }
            return root;
        }

        private Node GetMin(Node root)
        {
            Debug.Assert(root != null);

            while (root.left)
            {
                root = root.left;
            }
            return root;
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

        public void Add(T val)
        {
            if (!root)
            {
                root = new Node(val, color: Black);
                Count++;
                version++;
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
                        current.right = new Node(val, color: Red, parent: current);
                        break;
                    }
                    current = current.right;
                }
                else if (cr < 0)
                {
                    if (!current.left)
                    {
                        current.left = new Node(val, color: Red, parent: current);
                        break;
                    }
                    current = current.left;
                }
                else
                {
                    current.frequency++;
                    Count++;
                    version++;
                    return;
                }
            }

            Count++;
            version++;
            Balance(current);
        }

        //parent - child 
        private void Balance(Node node)
        {
            Debug.Assert(node != null);

            while (true)
            {
                if (node.color == Red)
                {
                    var sibling = node.Sibling();

                    if (!sibling || sibling.color == Black) break;


                    //if parent is equals to root
                    //         root                   Black  
                    //        /    \        ---->    /     \    
                    //  ->  node   sibling          Black  Black
                    //     return
                    //else
                    //             gParent          ->   *      
                    //            /                     /
                    //        parent                   Red  
                    //        /    \        ---->     /   \     
                    //  ->  node   sibling          Black  Black
                    //  *   gParent.color is red ?  continue : break

                    node.color = Black;
                    sibling.color = Black;

                    if (sibling.parent.parent)
                    {
                        node.parent.color = Red;
                        node = node.parent.parent;
                        continue;
                    }
                    return;
                }
                return;
            }

            //L
            if (node == node.parent.left)
            {
                //LL
                if (node.left && node.left.color == Red)
                {
                    //      parent                 node                Black           
                    //      /   \                 /    \              /     \
                    // -> node sibling   ---->  child parent         red    red
                    //    /  \                         /  \                /   \
                    // child  x                       x  sibling         black black
                    node.color = Black;
                    node.parent.color = Red;
                    RotateRight(node.parent);
                }
                //LR
                else
                {
                    Debug.Assert(node.right != null);
                    //      parent                parent           red                 child            black
                    //      /   \                 /    \          /  \                 /   \           /     \
                    // -> node sibling   ---->  child sibling  black black    ---->  node parent      red   red  
                    //    /  \                  /                /                   /      \          /      \
                    //   x   child         -> node             red                   x     sibling   black  black
                    //                         /               /      
                    //                        x             black                   
                    RotateLeft(node);
                    node = node.parent;
                    node.color = Black;
                    node.parent.color = Red;
                    RotateRight(node.parent);
                }
            }
            //R
            else
            {
                //RL
                if (node.left && node.left.color == Red)
                {
                    RotateRight(node);
                    node = node.parent;
                    node.color = Black;
                    node.parent.color = Red;
                    RotateLeft(node.parent);
                }
                //RR
                else
                {
                    //       parent                    node                Black           
                    //      /     \                   /    \              /     \
                    //    sibling node <-   ---->   parent  child        red    red
                    //            / \               /  \               /   \
                    //           x  child       sibling  x            black black
                    node.color = Black;
                    node.parent.color = Red;
                    RotateLeft(node.parent);
                }
            }
        }

        private void RotateLeft(Node node)
        {
            Debug.Assert(node != null);

            var other = node.right;

            node.right = other.left;
            if (other.left) other.left.parent = node;

            other.parent = node.parent;
            other.left = node;

            if (!node.parent) root = other;
            else if (node == node.parent.left) node.parent.left = other;
            else node.parent.right = other;

            node.parent = other;
        }

        private void RotateRight(Node node)
        {
            Debug.Assert(node != null);

            var other = node.left;

            node.left = other.right;
            if (other.right) other.right.parent = node;

            other.parent = node.parent;
            other.right = node;

            if (!node.parent) root = other;
            else if (node == node.parent.left) node.parent.left = other;
            else node.parent.right = other;

            node.parent = other;
        }

        public bool Contains(T val)
        {
            return GetNode(val);
        }

        public bool Remove(T val)
        {
            var node = GetNode(val);

            if (!node) return false;

            Count--;

            if (--node.frequency > 1) return true;

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
                    child.color = Black;
                    root = child;
                }
                return true;
            }

            if (RemoveInternal(node))
                Rebalance(node);
            return true;
        }


        private bool RemoveInternal(Node node)
        {
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
                if (node.color == Red) return false;
            }
            else
            {
                var child = node.Any();
                child.parent = node.parent;

                if (node == node.parent.left)
                {
                    node.parent.left = child;
                }
                else
                {
                    node.parent.right = child;
                }
                if (child.color == Red)
                {
                    child.color = Black;
                    return false;
                }
            }
            return true;
        }

        private void Rebalance(Node node)
        {

        }

        public void Clear()
        {
            root = null;
            Count = 0;
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
                throw new ArrayTypeMismatchException(nameof(array));
            }
            CopyTo(arr, index);
        }


        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        public IEnumerable<IEnumerable<T>> Levels() => Levels(root);
        public IEnumerable<IEnumerable<T>> Levels(INode<T> root)
        {
            if (root == null) yield break;
            var queue = new Queue<INode<T>>();
            queue.Enqueue(root);

            while (queue.Count != 0)
            {
                var n = queue.Count;
                var level = new List<T>(n);
                while (n-- > 0)
                {
                    var current = queue.Dequeue();
                    if (current.Left != null) queue.Enqueue(current.Left);
                    if (current.Right != null) queue.Enqueue(current.Right);
                    level.Add(current.Val);
                }
                yield return level;
            }
        }

        public IEnumerable<IEnumerable<T>> Paths() => Paths(root);
        public IEnumerable<IEnumerable<T>> Paths(INode<T> root)
        {
            if (root == null) yield break;
            var stack = new Stack<INode<T>>();
            var path = new List<T>();
            stack.Push(root);
            while (stack.Count != 0)
            {
                var current = stack.Pop();
                if (current == null)
                {
                    path.RemoveAt(path.Count - 1);

                }
                else if (current.Left == null && current.Right == null)
                {
                    yield return new List<T>(path) { current.Val };
                }
                else
                {
                    path.Add(current.Val);
                    stack.Push(null);
                    if (current.Right != null) stack.Push(current.Right);
                    if (current.Left != null) stack.Push(current.Left);
                }
            }
        }

#if DEBUG
        public IEnumerable<IEnumerable<string>> Colors()
        {
            if (!root) yield break;
            var queue = new Queue<Node>();
            queue.Enqueue(root);

            while (queue.Count != 0)
            {
                var n = queue.Count;
                var level = new List<string>(n);
                while (n-- > 0)
                {
                    var current = queue.Dequeue();
                    if (current.left) queue.Enqueue(current.left);
                    if (current.right) queue.Enqueue(current.right);
                    level.Add(current.color ? "Red" : "Black");
                }
                yield return level;
            }
        }
#endif 
        public struct Enumerator
            : IEnumerator<T>,
              IEnumerator,
              IEnumerable<T>,
              IEnumerable
        {
            private const bool Left = true;
            private const bool Right = false;
            private RedBlackTree<T> tree;
            private Stack<Node> stack;
            private Node root;
            private int frequency;
            private int version;
            private readonly int threadId;
            private bool state;
            internal Enumerator(RedBlackTree<T> tree)
            {
                this.tree = tree;
                root = tree.root;
                stack = new Stack<Node>();
                frequency = 0;
                version = tree.version;
                threadId = Environment.CurrentManagedThreadId;
                state = Left;
            }

            public IEnumerator<T> GetEnumerator()
            {
                if (threadId != Environment.CurrentManagedThreadId)
                {
                    return new Enumerator(tree);
                }
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public T Current
            {
                get
                {
                    if (root != null)
                        return root.val;
                    return default;
                }

            }

            object IEnumerator.Current
            {
                get
                {
                    if (root == null)
                    {
                        throw new InvalidOperationException();
                    }
                    return root.val;
                }
            }

            public void Dispose()
            {
                version = -1;
            }

            public bool MoveNext()
            {
                if (version != tree.version)
                {
                    throw new InvalidOperationException();
                }

                if (--frequency > 0)
                {
                    return true;
                }

                if (state == Right)
                {
                    root = root.right;
                    state = Left;
                }

                while (root)
                {
                    stack.Push(root);
                    root = root.left;
                }

                if (stack.Count == 0)
                {
                    root = null;
                    return false;
                }

                root = stack.Pop();
                frequency = root.frequency;
                state = Right;
                return true;
            }


            public void Reset()
            {
                if (version != tree.version)
                {
                    throw new InvalidOperationException();
                }
                root = tree.root;
                frequency = 0;
                stack.Clear();
            }
        }
    }
