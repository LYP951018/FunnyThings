﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexChart.RegexParser
{
    public class Expression
    {

    }

    //字符集合
    //\d == [0-9] etc.
    public class CharSetExpression : Expression
    {
        public SortedSet<CharRange> Ranges { get; set; }
        public bool IsReverse { get; set; }

        public bool AddRangeWithConflict(char begin,char end)
        {
            var range = new CharRange(begin, end);
            foreach (var r in Ranges)
            {
                if (!(range < r || range > r)) return false;
            }
            Ranges.Add(range);
            return true;
        }

        public void Add(char begin,char end)
        {
            Ranges.Add(new CharRange(begin, end));
        }

        public void Add(char c)
        {
            Ranges.Add(new CharRange(c));
        }
    }

    //重复
    public class LoopExpression : Expression
    {
        public Expression Looped { get; set; }
        int Min { get; set; }
        int Max { get; set; }
        bool IsGreedy { get; set; }

        public LoopExpression(int min,int max,bool isGreedy,Expression looped = null)
        {
            Min = min;
            Max = max;
            IsGreedy = isGreedy;
            Looped = looped;
        }
    }

    //串联
    public class SequenceExpression : Expression
    {
        public Expression Left { get; set; }
        public Expression Right { get; set; }

        public SequenceExpression(Expression left,Expression right)
        {
            Left = left;
            Right = right;
        }
    }

    public class BeginExpression : Expression
    {

    }

    public class EndExpression : Expression
    {

    }

    public class CaptureExpression : Expression
    {
        public string Name { get; set; }
        public Expression Sub { get; set; }

        public CaptureExpression(string name, Expression sub)
        {
            Name = name;
            Sub = sub;
        }

        //匿名
        public CaptureExpression(Expression sub)
        {
            Name = null;       
            Sub = sub;
        }
    }

    public class PositiveExpression : Expression
    {
        public Expression Matched { get; set; }

        public PositiveExpression(Expression subExpression)
        {
            Matched = subExpression;
        }
    }

    public class NegativeExpression : Expression
    {
        public Expression Matched { get; set; }

        public NegativeExpression(Expression subExpression)
        {
            Matched = subExpression;
        }
    }

    public class ReferenceExpression : Expression
    {
        public string Name { get; set; }

        public ReferenceExpression(string name)
        {
            Name = name;
        }
    }

    public class RegexExpression
    {

    }

    public class MatchExpression : Expression
    {
        public string Name { get; set; }
        public int Index { get; set; }

        public MatchExpression(string name,int index)
        {
            Name = name;
            Index = index;
        }
    }

}