using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using static Snake.SearchResource;

namespace Snake
{
    public class EatingSnake
    {
        

        public EatingSnake(GridState[,] graph)
        {
            _graph = graph;
            var coordinate = new Coordinate();
            coordinate.X = GridWidth / 2;
            coordinate.Y = GridHeight / 2;
            graph[coordinate.X,coordinate.Y] = GridState.Snake;
            _snakeBody.AddFirst(coordinate);

        }

        public EatingSnake(EatingSnake snake)
        {
            var newGraph = new GridState[GridWidth, GridHeight];
            for(int i = 0;i < GridWidth;++i)
            {
                for(int j = 0;j < GridHeight;++j)
                {
                    newGraph[i, j] = snake._graph[i, j];
                }
            }

            _graph = newGraph;

            foreach(var c in snake._snakeBody)
            {
                _snakeBody.AddLast(c);
            }

            _currentDirection = snake.CurrentDirection;
            EggPos = snake.EggPos;           
        }

        private Coordinate GetDesination(Coordinate start,Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    start.X--;
                    break;
                case Direction.Down:
                    start.X++;
                    break;
                case Direction.Left:
                    start.Y--;
                    break;
                case Direction.Right:
                    start.Y++;
                    break;
            }
            return start;
        }

        public GridState Move()
        {
            var newCoordinate = GetDesination(Head, CurrentDirection);                         
            var last = _snakeBody.Last.Value;
            _graph[last.X, last.Y] = GridState.Nothing;
            var state = _graph[newCoordinate.X, newCoordinate.Y];
            switch (state)
            {               
                case GridState.Nothing:                                     
                    _snakeBody.RemoveLast();
                    _snakeBody.AddFirst(newCoordinate);
                    _graph[newCoordinate.X, newCoordinate.Y] = GridState.Snake;
                    break;
                case GridState.Egg:
                    _graph[newCoordinate.X, newCoordinate.Y] = GridState.Snake;
                    _graph[last.X, last.Y] = GridState.Snake;
                    _snakeBody.AddFirst(newCoordinate);
                    break;
                case GridState.Snake:
                    break;
            }
            return state;
        }

        public List<Coordinate> BfsSearch(Coordinate start, Coordinate end)
        {
            Queue<Coordinate> queue = new Queue<Coordinate>();
            queue.Enqueue(start);
            SearchResource.Clear();
            VisitedGraph[start.X, start.Y] = true;
            bool isFound = false;
            while (queue.Count != 0)
            {
                var c = queue.Dequeue();
                if (c == end)
                {
                    isFound = true;
                    break;
                }
                for (int i = 0; i < 4; ++i)
                {
                    var co = new Coordinate()
                    {
                        X = c.X + XDirections[i],
                        Y = c.Y + YDirections[i]
                    };
                    if ((IsValid(co) || co == end)  && !VisitedGraph[co.X, co.Y])
                    {
                        queue.Enqueue(co);
                        VisitedGraph[co.X, co.Y] = true;
                        PathFound[co.X, co.Y] = c;
                    }
                }
            }
            if (isFound)
            {
                return GetPath(start, end);
            }
            return null;
        }

        private void Dfs(List<List<Coordinate>> list,
            List<Coordinate> nowList,Coordinate now,int count)
        {
            if (count == DfsDepth) return;
            if(now == Tail)
            {
                list.Add(CopyList(nowList));
            }
            else
            {
                ++count;
                VisitedGraph[now.X, now.Y] = true;
                for (int i = 0; i < 4; ++i)
                {
                    var co = new Coordinate()
                    {
                        X = now.X + XDirections[i],
                        Y = now.Y + YDirections[i]
                    };
                    if ((IsValid(co) || co == Tail) && !VisitedGraph[co.X, co.Y])
                    {
                        
                        VisitedGraph[co.X, co.Y] = true;
                        nowList.Add(co);
                        Dfs(list, nowList, co,count);
                        VisitedGraph[co.X, co.Y] = false;
                        nowList.RemoveAt(nowList.Count - 1);
                    }
                }
            }
        }


        private Coordinate? FindLongestPath(Coordinate target, Coordinate start)
        {
            CandidatePaths.Clear();
            var nowList = new List<Coordinate>();
            SearchResource.Clear();
            Dfs(CandidatePaths, nowList, start,0);
            int maxLen = 0;
            List<Coordinate> maxList = null;
            foreach (var l in CandidatePaths)
            {
                if (l.Count > maxLen)
                {
                    maxLen = l.Count;
                    maxList = l;
                }
            }
            if (maxList == null) return null;
            return maxList.First();
        }

        static List<T> CopyList<T>(List<T> list)
            where T :struct
        {
            var newList = new List<T>(list.Count);
            foreach(var c in list)
            {
                newList.Add(c);
            }
            return newList;
        }


        static private Direction GetDirection(Coordinate start, Coordinate end)
        {
            if (start.Y == end.Y + 1)
                return Direction.Left;
            if (start.Y + 1 == end.Y)
                return Direction.Right;
            if (start.X + 1 == end.X)
                return Direction.Down;
            if (start.X == end.X + 1)
                return Direction.Up;
            throw new ArgumentException();
        }

        private bool IsValid(Coordinate coordinate)
        {
            var state = Graph[coordinate.X, coordinate.Y];
            return state != GridState.Wall && state != GridState.Snake;
        }

        public LinkedList<Coordinate> SnakeBody => _snakeBody;
        public Direction CurrentDirection
        {
            get
            {
                return _currentDirection;
            }
            set
            {
                if(_snakeBody.Count == 1)
                    _currentDirection = value;
                else
                {
                    //Debug.Assert((int)value + (int)CurrentDirection != 0);
                }                
                if ((int)value + (int)CurrentDirection != 0) 
                    _currentDirection = value;
            }
        }

        Coordinate? GetTheSafePath()
        {
            var fakeSnake = new EatingSnake(this);
            var path = fakeSnake.BfsSearch(fakeSnake.Head, EggPos);
            if (path == null) return null;
            Coordinate res = path.Last();
            while (true)
            {
                //每走一步做一次 Bfs               
                fakeSnake.CurrentDirection = GetDirection(fakeSnake.Head, path.Last());
                switch(fakeSnake.Move())
                {
                    case GridState.Egg:
                        if(fakeSnake.BfsSearch(fakeSnake.Head,fakeSnake.Tail) != null)
                        {
                            return res;
                        }
                        return null;
                    case GridState.Wall:
                    case GridState.Snake:
                        throw new InvalidOperationException();
                    case GridState.Nothing:                       
                        break;
                }
                path = fakeSnake.BfsSearch(fakeSnake.Head, EggPos);
                if (path == null) return null;
            }
        }

        Coordinate? GetTailPath()
        {
            return FindLongestPath(Tail, Head);
        }

        private bool Wander()
        {
            if (!PreWander())
                return ReWander();
            return true;
        }

        private bool PreWander()
        {
            var i = RandomGen.Next(0, 4);
            var co = new Coordinate
            {
                X = Head.X + XDirections[i],
                Y = Head.Y + YDirections[i]
            };
            if(IsValid(co) && BfsSearch(co, Tail) != null)
            {
                var direction = GetDirection(Head,co);
                CurrentDirection = direction;
                return (CurrentDirection == direction);
            }
            return false;
        }

        private bool ReWander()
        {
            for(int i = 0;i < 4;++i)
            {
                var co = new Coordinate
                {
                    X = Head.X + XDirections[i],
                    Y = Head.Y + YDirections[i]
                };
                if (IsValid(co))
                {
                    var direction = GetDirection(Head, co);
                    CurrentDirection = direction;
                    if (CurrentDirection == direction) return true;
                }               
            }
            return false;
        }
        public GridState? AutoMove()
        {
            if(_isLooping)
            {
                if (Wander())
                {
                    _isLooping = false;
                    _maybeLoop = false;
                }
                else return Normal();
            }
            else
                return Normal();
            return Move();                  
        }

        private GridState? Normal()
        {
            var path = GetTheSafePath();
            if (path == null)
            {
                return FollowTail();
            }
            else
            {
                CurrentDirection = GetDirection(Head, path.Value);
                return Move();
            }
        }

        private GridState? FollowTail()
        {
            var path = GetTailPath();
            if (path != null)
            {
                CurrentDirection = GetDirection(Head, path.Value);
                var desination = GetDesination(Head, CurrentDirection);
                if (_maybeLoop && _loopStart == desination)
                {
                    _isLooping = true;
                }
                if (!_maybeLoop)
                {
                    _maybeLoop = true;
                    _loopStart = desination;
                }
            }
            else
            {
                if (!Wander())
                    return null;
            }
            return Move();
        }

        public GridState[,] Graph => _graph;
        private GridState[,] _graph;
        public static readonly Brush SnakeBrush = Brushes.SkyBlue;
        public Coordinate Tail => _snakeBody.Last.Value;
        public Coordinate Head => _snakeBody.First.Value;
        private Direction _currentDirection = Direction.Up;
        private LinkedList<Coordinate> _snakeBody = new LinkedList<Coordinate>();
        public Coordinate EggPos { get; set; }
        private bool _maybeLoop = false, _isLooping = false;
        private Coordinate _loopStart = Nothing;
    }
}
