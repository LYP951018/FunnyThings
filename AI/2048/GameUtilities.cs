using _2048.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _2048
{
    public static class GameUtilities
    {
        static public int ToIndex(this Coordinate c)
        {
            return c.X * Game.BoardHeight + c.Y;
        }

        static public int ToIndex(int x, int y)
        {
            return x * Game.BoardHeight + y;
        }

        static public Coordinate ToCoordinate(int index)
        {
            var row = index / Game.BoardWidth;
            var column = index % Game.BoardHeight;
            return new Coordinate(row, column);
        }

        static public T GetValue<T>(this T[,] arr, Coordinate c)
        {
            return arr[c.X, c.Y];
        }

        static public T Set2DValue<T>(this T[,] arr, Coordinate c, T newValue)
        {
            var oldValue = arr.GetValue(c);
            arr[c.X, c.Y] = newValue;
            return oldValue;
        }

        static public void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (var x in source)
                action(x);
        }

        static public void Communicate<TSource>(this IEnumerable<TSource> source, Action<TSource, TSource> action)
        {
            if (source.Any())
            {
                TSource previous = source.First(), current;
                foreach (var v in source.Skip(1))
                {
                    current = v;
                    action(previous, current);
                    previous = current;
                }
            }
        }
    }
}
