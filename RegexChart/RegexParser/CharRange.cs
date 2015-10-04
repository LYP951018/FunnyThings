using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexChart.RegexParser
{
    public struct CharRange
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

        public bool Equals(CharRange obj)
        {
            return Begin == obj.Begin && End == obj.End;
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
            return rhs < lhs;
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
