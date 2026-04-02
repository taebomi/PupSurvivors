using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

namespace TaeBoMi.Data
{
    public class PriorityQueue<T> where T : IComparable<T>
    {
        private readonly List<T> _heap;

        public PriorityQueue(int capacity)
        {
            _heap = new List<T>(capacity);
        }

        public int Count => _heap.Count;

        public void Enqueue(T item)
        {
            _heap.Add(item);

            var i = _heap.Count - 1;
            while (i > 0)
            {
                var parent = (i - 1) / 2;
                if (_heap[parent].CompareTo(_heap[i]) > 0)
                {
                    (_heap[parent], _heap[i]) = (_heap[i], _heap[parent]);
                    i = parent;
                }
                else
                {
                    break;
                }
            }
        }

        public T Dequeue()
        {
            if (_heap.Count == 0)
            {
                throw new InvalidOperationException();
            }

            var root = _heap[0];

            _heap[0] = _heap[^1];
            _heap.RemoveAt(_heap.Count - 1);

            var i = 0;
            var last = _heap.Count - 1;
            while (i < last)
            {
                var child = i * 2 + 1;

                if (child < last && _heap[child].CompareTo(_heap[child + 1]) > 0)
                {
                    child++;
                }

                if (child > last || _heap[i].CompareTo(_heap[child]) < 0)
                {
                    break;
                }

                (_heap[i], _heap[child]) = (_heap[child], _heap[i]);
                i = child;
            }

            return root;
        }
    }
}