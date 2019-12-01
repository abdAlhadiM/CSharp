using System;

namespace Q
{
    class Queue
    {
        private int[] arr;
        private int head;
        private int tail;
        private int size;
        private int capacity;

        public Queue()
        {
            arr = new int[4];
            head = size = tail = 0;
            capacity = 4;
        }

        public void Enqueue(int val)
        {
            if (size == capacity)
            {
                Resize();
            }

            arr[tail] = val;
            tail = (1 + tail) % capacity;
            size++;
        }

        public int Dequeue()
        {
            if (size == 0) throw new InvalidOperationException();

            var removed = arr[head];
            arr[head] = default;

            if (--size == 0)
            {
                head = 0;
                tail = 0;
            }
            else
            {
                head = (1 + head) % capacity;
            }

            return removed;
        }

        public bool IsEmpty() => size == 0;
        public bool IsFull() => size == capacity;

        public int Front
        {
            get
            {
                if (size == 0)
                    throw new InvalidOperationException();

                return arr[head];
            }
        }

        public int Rear
        {
            get
            {
                if (size == 0)
                    throw new InvalidOperationException();
                return tail == 0 ? arr[capacity - 1] : arr[tail - 1];
            }
        }

        private void Resize()
        {
            var newArray = new int[capacity * 2];
            if (head == 0)
            {
                Array.Copy(arr, 0, newArray, 0, size);
            }
            else
            {
                Array.Copy(arr, head, newArray, 0, size - head);
                Array.Copy(arr, 0, newArray, size - head, tail);
                head = 0;
            }
            arr = newArray;
            capacity *= 2;
            tail = size;
        }
    }
}
