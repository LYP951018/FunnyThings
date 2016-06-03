using System.Diagnostics;

namespace _2048
{
    public unsafe struct StaticList
    {
        public StaticList(int* arr)
        {
            _items = arr;
            _waterline = arr;
        }

        public void Add(int value)
        {
            *Waterline = value;
            ++_waterline;
            Debug.Assert(Count < 16);
        }

        public int Back()
        {
            return *Waterline;
        }

        public int Pop()
        {
            --_waterline;
            return *Waterline;
        }

        public int Count
        {
            get
            {
                return (int)(Waterline - _items);
            }
        }

        public int this[int id]
        {
            get
            {
                Debug.Assert(id < Count);
                return _items[id];
            }
            set
            {
                Debug.Assert(id < Count);
                _items[id] = value;
            }
        }

        private int* _items;
        private int* _waterline;
        public int* Waterline => _waterline;
    }
}
