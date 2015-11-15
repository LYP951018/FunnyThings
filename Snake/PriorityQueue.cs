using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    [DebuggerDisplay("Count = {Count}")]
    public class PriorityQueue<T> : IEnumerable<T>,
        System.Collections.ICollection,
        IReadOnlyCollection<T>
    {
        private List<T> _list;
        private IComparer<T> comparer;

        public int Count
        {
            get
            {
                return ((ICollection)_list).Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return ((ICollection)_list).IsSynchronized;
            }
        }

        public object SyncRoot
        {
            get
            {
                return ((ICollection)_list).SyncRoot;
            }
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_list).CopyTo(array, index);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((ICollection)_list).GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_list).GetEnumerator();
        }

        public PriorityQueue()
        {
            _list = new List<T>();
            comparer = Comparer<T>.Default;
        }

        public PriorityQueue(IComparer<T> comparer)
            :this()
        {
            if (comparer != null)
                this.comparer = comparer;
        }

        public PriorityQueue(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentException("capacity < 0!");
            _list = new List<T>(capacity);
            comparer = Comparer<T>.Default;
        }

        public PriorityQueue(int capacity, IComparer<T> comparer)
            :this(comparer)
        {
            Capacity = capacity;
        }

        public int Capacity
        {
            get
            {
                return _list.Capacity;
            }
            set
            {
                _list.Capacity = value;
            }
        }

        public bool IsEmpty()
        {
            return _list.Count == 0;
        }

        public void Clear()
        {
            _list.Clear();
        }

        public void Enqueue(T item)
        {
            _list.Add(item);
            //上滤
            int childIndex = _list.Count - 1;
            int parentIndex;
            for(;childIndex != 0;childIndex = parentIndex)
            {
                parentIndex = GetParentIndex(childIndex);
                if (comparer.Compare(_list[childIndex], _list[parentIndex]) < 0)
                {
                    Swap(_list, childIndex, parentIndex);
                }
                else break;
            }
        }

        public T Dequeue()
        {
            if (Count == 0)
                throw new InvalidOperationException();
            var removed = _list.First();
            //下滤
            int holeIndex = 0;
            int childIndex;
            for(; GetLeftChildIndex(holeIndex) < _list.Count - 1 ;holeIndex = childIndex)
            {
                childIndex = GetLeftChildIndex(holeIndex);
                if(childIndex + 1 < _list.Count - 1 && comparer.Compare(_list[childIndex + 1],_list[childIndex]) < 0)
                {
                    ++childIndex;
                }
                if (comparer.Compare(_list[childIndex], _list.Last()) < 0)
                    _list[holeIndex] = _list[childIndex];
                else break;
            }
            _list[holeIndex] = _list.Last();
            _list.RemoveAt(_list.Count - 1);
            return removed;
        }

        public void Enqueue(params T[] list)
        {
            foreach(var c in list)
            {
                Enqueue(c);
            }
        }

        private static void Swap<TObject>(List<TObject> list,int i,int j)
        {
            if(i != j)
            {
                TObject t = list[i];
                list[i] = list[j];
                list[j] = t;
            }
        }

        private int GetLeftChildIndex(int i)
        {
            return 2 * i + 1;
        }

        private int GetRightChildIndex(int i)
        {
            return GetLeftChildIndex(i) + 1;
        }

        private int GetParentIndex(int i)
        {
            Debug.Assert(i > 0);
            return (i - 1) / 2;
        }
    }
}
