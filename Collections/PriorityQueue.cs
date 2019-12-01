using System;
using System.Collections.Generic;

namespace P
{
    class PriorityQueue
    {
        private int[] arr;
        private int next;

        public PriorityQueue()
        {
            arr = new int[4];
            next = 0;
        }

        public void Push(int val)
        {
            if (next == arr.Length)
            {
                Array.Resize(ref arr, arr.Length * 2);
            }

            var index = next;
            while (index > 0)
            {
                var p = (index - 1) >> 1;
                if (arr[p] > val) break;
                arr[index] = arr[p];
                index = p;
            }
            arr[index] = val;
            next++;
        }

        public int Pop()
        {
            if (next == 0)
                throw new InvalidOperationException();
            if (next == 1)
                return arr[--next];
            next--;
            Swap(0, next);
            HeapifyDown(0, next);
            return arr[next];
        }

        private void HeapifyDown(int index, int count)
        {
            for (int maxChild = index * 2 + 1; maxChild < count; maxChild = index * 2 + 1)
            {
                if (maxChild + 1 < count && arr[maxChild + 1] > arr[maxChild]) maxChild++;
                if (arr[index] > arr[maxChild]) break;
                Swap(index, maxChild);
                index = maxChild;
            }
        }

        public int Count => next;

        private void Swap(int a, int b)
        {
            var temp = arr[a];
            arr[a] = arr[b];
            arr[b] = temp;
        }

        public IEnumerable<int> Data()
        {
            return arr;
        }
    }
}
