using System;

namespace RegexChart.RegexParser
{
    public struct CharRange : IComparable
    {
        public char Begin { get; set; }
        public char End { get; set; }

        public CharRange(char begin,char end)
        {
            if(begin < end)
            {
                Begin = begin;
                End = end;
            }
            else
            {
                Begin = end;
                End = Begin;
            }           
        }

        public CharRange(char c)
        {
            Begin = End = c;
        }

        public bool IsValid()
        {
            return Begin <= End;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            return Equals((CharRange)obj);
        }

        public bool IsOverlapped(CharRange rhs)
        {
            return !(this < rhs || this > rhs);
        }

        public bool Equals(CharRange obj)
        {
            return Begin == obj.Begin && End == obj.End;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            var charObj = (CharRange)obj;
            if (this.Equals(charObj)) return 0;
            if (this < charObj) return -1;
            return 1;           
        }

        public override int GetHashCode()
        {
            return Begin.GetHashCode() << 4 ^ End.GetHashCode() << 8;
        }

        public static bool operator ==(CharRange lhs,CharRange rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(CharRange lhs, CharRange rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator < (CharRange lhs, CharRange rhs)
        {
            return lhs.End < rhs.Begin;
        }

        public static bool operator >(CharRange lhs, CharRange rhs)
        {
            return rhs.End < lhs.Begin;
        }

        public static bool operator <=(CharRange lhs, CharRange rhs)
        {
            return lhs < rhs || lhs == rhs;
        }

        public static bool operator >=(CharRange lhs, CharRange rhs)
        {
            return rhs <= lhs;
        }
    }
}
