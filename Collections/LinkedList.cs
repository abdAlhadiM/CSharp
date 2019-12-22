public class LinkedList<T>
{
    private class Node
    {
        public Node next;
        public Node prev;
        public T val;

        public static implicit operator bool(Node node) => node != null;
    }

    private Node head;
    private readonly IEqualityComparer<T> equalityComparer;
    public int Count { get; private set; }
    public LinkedList()
    {
        head = new Node();
        head.next = head;
        head.prev = head;
        Count = 0;
        equalityComparer = EqualityComparer<T>.Default;
    }
    public void AddFirst(T val)
    {
        var node = new Node
        {
            val = val,
            next = head.next,
            prev = head
        };
        head.next = node;
        node.next.prev = node;
        Count++;
    }

    public void AddLast(T val)
    {
        var node = new Node
        {
            val = val,
            prev = head.prev,
            next = head
        };

        head.prev = node;
        node.prev.next = node;
        Count++;
    }

    public T this[int i]
    {
        get => GetAt(i);
        set => AddAt(i, value);
    }

    public T GetAt(int index)
    {
        if (index < 0 || index > Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        var current = head.next;
        while (index-- > 0)
        {
            current = current.next;
        }
        return current.val;
    }

    public void AddAt(int index, T val)
    {
        if (index < 0 || index > Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (index == 0) AddFirst(val);
        else if (index == Count) AddLast(val);
        else
        {
            var current = head.next;
            while (index-- > 0)
            {
                current = current.next;
            }

            var node = new Node
            {
                val = val,
                next = current,
                prev = current.prev
            };
            current.prev = node;
            node.prev.next = node;
            Count++;
        }
    }

    public void RemoveFirst()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException();
        }

        head.next = head.next.next;
        head.next.prev = head;
        Count--;
    }

    public void RemoveLast()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException();
        }


        head.prev = head.prev.prev;
        head.prev.next = null;
        Count--;
    }

    public bool RemoveAt(int index)
    {
        if (Count == 0 || index < 0 || index >= Count)
            return false;
        if (index == 0)
        {
            RemoveFirst();
            return true;
        }
        if (index == Count - 1)
        {
            RemoveLast();
            return true;
        }
        var removed = head.next;
        while (index-- > 0)
        {
            removed = removed.next;
        }

        removed.next.prev = removed.prev;
        removed.prev.next = removed.next;
        Count--;
        return true;
    }
}
