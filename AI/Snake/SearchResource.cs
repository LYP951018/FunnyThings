using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    public static class SearchResource
    {
        public const int GridWidth = 12;
        public const int GridHeight = 12;
        static public readonly int[] XDirections = new int[] { -1, 1, 0, 0 };
        static public readonly int[] YDirections = new int[] { 0, 0, -1, 1 };
        static public bool[,] VisitedGraph = new bool[GridWidth, GridHeight];
        static public Coordinate[,] PathFound = new Coordinate[GridWidth, GridHeight];
        static public readonly Coordinate Nothing = new Coordinate(-1, -1);
        static public int[,] TailDistance = new int[GridWidth, GridHeight];
        static public Random RandomGen = new Random();
        static public List<List<Coordinate>> CandidatePaths = new List<List<Coordinate>>(10000);//magic number?
        public const int DfsDepth = 20;

        static public List<Coordinate> GetPath(Coordinate start, Coordinate end)
        {
            var path = new List<Coordinate>();
            while (end != start)
            {
                path.Add(end);
                end = PathFound[end.X, end.Y];
            }
            return path;
        }

        static public void Clear()
        {
            for(int i = 0;i < GridWidth;++i)
            {
                for(int j = 0;j < GridHeight;++j)
                {
                    VisitedGraph[i, j] = false;
                    PathFound[i, j] = Nothing;
                    TailDistance[i, j] = 0;
                }
            }
            CandidatePaths.Clear();
        }
    }

    public enum Direction
    {
        Up = -1, Down = 1, Left = -2, Right = 2
    }

    public enum GridState
    {
        Nothing = 0, Egg, Snake, Wall
    }

}
