using _2048.Model;

namespace _2048
{
    public static class GameUtilities
    {
        static public int ToIndex(this Coordinate c)
        {
            return c.X * Game.BoardHeight + c.Y;
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
    }
}
