using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexChart.RegexParser
{
    public class FlatSet<TValue> : ICollection<TValue>
        where TValue : IComparable
    {
        private TValue[] values;
        private int _size;
        public int Capacity
        {
            get
            {
                return values.Length;
            }
            set
            {
                if(value != values.Length)
                {
                    if (value < _size)
                        throw new ArgumentException("capacity less than size.");
                    if(value > 0)
                    {
                        var newValues = new TValue[value];
                        if(_size > 0)
                        {
                            Array.Copy(values, 0, newValues, 0, _size);
                        }
                        values = newValues;
                    }
                    else
                    {
                        values = emptyValues;
                    }
                }
            }
        }

        public TValue this[int index]
        {
            get
            {
                return values[index];
            }
        }

        public int Count
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return _size;
            }
        }

        static TValue[] emptyValues = new TValue[0];

        private const int _defaultCpacity = 4;
        private const int MaxArrayLength = 0x7FEFFFFF;

        public FlatSet()
        {
            values = emptyValues;
            _size = 0;
        }

        public FlatSet(FlatSet<TValue> rhs)
        {
            values = new TValue[rhs.values.Length];
            Array.Copy(rhs.values, values, rhs._size);
            _size = rhs._size;
        }

        public void Clear()
        {
            if(_size > 0)
            {
                Array.Clear(values, 0, _size);
                _size = 0;
            }
        }

        public FlatSet(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentException("capacity < 0!");
            values = new TValue[capacity];
        }

        public void Add(TValue value)
        {
            var i = Array.BinarySearch<TValue>(values,0,_size, value);
            if (i >= 0)
                throw new ArgumentException("Duplicated element.");
            Insert(~i, value);
        }

        public bool Contains(TValue item)
        {
            return IndexOf(item) >= 0;
        }

        public int IndexOf(TValue item)
        {
            int ret = Array.BinarySearch<TValue>(values, 0, _size, item);
            return ret >= 0 ? ret : -1;
        }

       
        private void EnsureCapacity(int min)
        {
            int newCapacity = values.Length == 0 ? _defaultCpacity : values.Length * 2;
            if ((uint)newCapacity > MaxArrayLength) newCapacity = MaxArrayLength;
            if (newCapacity < min) newCapacity = min;
            Capacity = newCapacity;
        }

        private void Insert(int index,TValue value)
        {
            if (_size == values.Length) EnsureCapacity(_size + 1);
            if(index < _size)
            {
                Array.Copy(values, index, values, index + 1, _size - index);
            }
            values[index] = value;
            _size++;
        }

        private TValue GetByIndex(int index)
        {
            if (index < 0 || index >= _size)
                throw new IndexOutOfRangeException();
            return values[index];
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        bool ICollection<TValue>.IsReadOnly
        {
            get { return false; }
        }

        public void CopyTo(TValue[] array)
        {
            CopyTo(array, 0);
        }

        public void CopyTo(TValue[] array,int arrayIndex)
        {
            Array.Copy(values, 0, array, arrayIndex, _size);
        }

        public void CopyTo(int index,TValue[] array,int arrayIndex,int count)
        {
            if (_size - index < count)
                throw new ArgumentException();
            Array.Copy(values, index, array, arrayIndex, count);
        }


        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)_size)
                throw new ArgumentOutOfRangeException();
            _size--;
            if (index < _size)
                Array.Copy(values, index + 1, values, index, _size - index);
        }

        public bool Remove(TValue item)
        {
            int index = IndexOf(item);
            if(index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        private struct Enumerator : IEnumerator<TValue>
        {
            private FlatSet<TValue> _flatSet;
            private TValue current;
            private int index;

            public TValue Current => current;

            internal Enumerator(FlatSet<TValue> flatSet)
            {
                _flatSet = flatSet;
                current = default(TValue);
                index = 0;
            }

            public void Dispose()
            {               
            }

            public bool MoveNext()
            {
                var localSet = _flatSet;
                if((uint)index < (uint)localSet._size)
                {
                    current = localSet.values[index];
                    index++;
                    return true;
                }
                index = localSet._size + 1;
                current = default(TValue);
                return false;
            }

            Object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (index == 0 || index > _flatSet._size)
                        throw new IndexOutOfRangeException();
                    return current;
                }
            }

            void System.Collections.IEnumerator.Reset()
            {
                index = 0;
                current = default(TValue);
            }
        }
    }
}
