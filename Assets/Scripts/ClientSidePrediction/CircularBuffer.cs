using System;
using System.Collections;
using System.Collections.Generic;

namespace Dropt
{
    public class CircularBuffer<T> : IEnumerable<T> // Implement IEnumerable<T> to enable iteration
    {
        T[] buffer;
        int bufferSize;

        public CircularBuffer(int bufferSize)
        {
            this.bufferSize = bufferSize;
            buffer = new T[bufferSize];
        }

        public void Add(T item, int index) => buffer[index % bufferSize] = item;
        public T Get(int index) => buffer[index % bufferSize];
        public void Clear() => buffer = new T[bufferSize];

        // Implement the GetEnumerator method to return an enumerator for the buffer
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < bufferSize; i++)
            {
                yield return buffer[i];
            }
        }

        // Explicit implementation of non-generic IEnumerable
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

/*

namespace Dropt
{
    public class CircularBuffer<T>
    {
        T[] buffer;
        int bufferSize;

        public CircularBuffer(int bufferSize)
        {
            this.bufferSize = bufferSize;
            buffer = new T[bufferSize];
        }

        public void Add(T item, int index) => buffer[index % bufferSize] = item;
        public T Get(int index) => buffer[index % bufferSize];
        public void Clear() => buffer = new T[bufferSize];
    }
}
*/