using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using static System.Linq.Enumerable;

namespace _2048.Model
{
    public enum Direction : byte
    {
        Up = 0, Left, Down, Right
    }

    //TODO: class or struct?
    public struct TransformInfo
    {
        public Coordinate Destination { get; set; }
        public bool WasNew { get; set; }
        public uint PreviousNumber { get; set; }
    }

    public struct ResultInfo
    {
        public bool HasMoved { get; set; }
        public bool HasWon { get; set; }
        public uint MergedCount { get; set; }
    }

    public class Game
    {
        public const int BoardWidth = 4;
        public const int BoardHeight = 4;

        private byte[,] _numbers = new byte[BoardHeight, BoardWidth];
        static private TransformInfo[,] _transformInfo = new TransformInfo[BoardHeight, BoardWidth];

        private byte[,] Numbers => _numbers;
        public TransformInfo[,] Transformations => _transformInfo;

        private static readonly int[] _directions = new int[] { -4, 4, -1, 1 };
        private static readonly int[] _rightSequence = new int[] { 2, 1, 0 };
        private static readonly int[] _leftSequence = new int[] { 1, 2, 3 };

        private Direction _currentDirection;
        private Direction CurrentDirection
        {
            get
            {
                return _currentDirection;
            }
            set
            {
                _currentDirection = value;
            }
        }
        public Game()
        {
            ResetTransformation();
            //Logger("---------------------Game starts!-------------------------");
        }

        public Game(Game other)
        {
            Array.Copy(other._numbers, _numbers, 16);
            //Range(0, 4).ForEach(i => Range(0, 4).ForEach(j => _numbers[i, j] = other._numbers[i, j]));
            //ResetTransformation();
        }

        public void Reset(Game other)
        {
            Array.Copy(other._numbers, _numbers, 16);
        }

        public void SetNumber(Coordinate c, uint v)
        {
            Debug.Assert(v != 1);
            byte value;
            if (v == 0)
                value = 0;
            else
                value = (byte)(GetOneBitPos(v));
            _numbers.Set2DValue(c, value);
        }

        public void SetNumber(int i, int j, uint v)
        {
            SetNumber(new Coordinate(i, j), v);
        }

        public uint GetNumber(Coordinate c)
        {
            return Convert(Numbers.GetValue(c));            
        }

        public uint GetNumber(int i, int j)
        {
            return GetNumber(new Coordinate(i, j));
        }

        public uint GetOneBitPos(uint n)
        {
            const uint mask = 1;
            uint pos = 0;
            while(true)
            {
                if ((mask & n) != 0)
                    break;
                n >>= 1;
                ++pos;
            }
            return pos;
        }

        private uint Convert(byte value)
        {
            if (value == 0)
                return 0;
            return (uint)(1 << value);
        }

        public IEnumerable<uint> TraverseNumbers()
        {
            for(int i = 0; i < BoardHeight; ++i)
                for(int j = 0; j < BoardWidth; ++j)
                    yield return Convert(_numbers[i, j]);                   
        }

        private int[] GenerateSequence()
        {
            switch(_currentDirection)
            {
                case Direction.Down:
                case Direction.Right:
                    return _rightSequence;
                case Direction.Up:
                case Direction.Left:
                    return _leftSequence;
                default:
                    throw new InvalidOperationException();
            }
        }

        private struct FindResult
        {
            public int Position { get; set; }
            public int PreviousPosition { get; set; }
        }

        private int ConvertPosition(int position)
        {
            switch(_currentDirection)
            {
                case Direction.Down:
                case Direction.Right:
                    return position;
                case Direction.Left:
                case Direction.Up:
                    return 3 - position;
                default:
                    throw new InvalidOperationException();
            }
        }

        private FindResult FindFirstNonEmpty(uint vec, int start)
        {
            var currentPos = start;
            uint currentMask = (uint)(0xFF << (currentPos << 3));
            byte current;

            while (currentPos < 4)
            {
                current = (byte)((vec & currentMask) >> (currentPos << 3));
                if (current == 0)
                {
                    ++currentPos;
                    currentMask <<= 8;
                }
                else
                    break;
            }

            switch(_currentDirection)
            {
                case Direction.Down:
                case Direction.Right:
                    return new FindResult { PreviousPosition = currentPos - 1, Position = currentPos };
                case Direction.Up:
                case Direction.Left:
                    {
                        var position = 3 - currentPos;
                        //Debug.Assert(position != start);
                        return new FindResult { PreviousPosition = position + 1, Position = position };
                    }
                default:
                    throw new InvalidOperationException();
            }
        }

        private uint PackAsUInt(int columnOrRow)
        {
            uint vec = 0;
            switch(_currentDirection)
            {
                case Direction.Up:
                    for(int i = 0; i < BoardHeight; ++i)
                    {
                        vec <<= 8;
                        vec |= Numbers[i, columnOrRow];                       
                    }
                    break;
                case Direction.Down:
                    for(int i = BoardHeight - 1; i >= 0; --i)
                    {
                        vec <<= 8;
                        vec |= Numbers[i, columnOrRow];                       
                    }
                    break;
                case Direction.Left:
                    for (int i = 0; i < BoardHeight; ++i)
                    {
                        vec <<= 8;
                        vec |= Numbers[columnOrRow, i];                       
                    }
                    break;
                case Direction.Right:
                    for (int i = BoardHeight - 1; i >= 0; --i)
                    {
                        vec <<= 8;
                        vec |= Numbers[columnOrRow, i];                      
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return vec;
        }

        private bool IsEdge(int pos)
        {
            switch(_currentDirection)
            {
                case Direction.Down:
                case Direction.Right:
                    return pos == 3;
                case Direction.Left:
                case Direction.Up:
                    return pos == 0;
                default:
                    throw new InvalidOperationException();
            }
        }

        private bool IsEmpty(Coordinate c)
        {
            return Numbers.GetValue(c) == 0;
        }

        public bool HasLost()
        {
            //A dirty trick.
            var prevNumbers = Numbers;
            var prevTransformations = Transformations;
            var prevDirection = _currentDirection;
            _numbers = new byte[BoardHeight, BoardWidth];            
            _transformInfo = new TransformInfo[BoardHeight, BoardWidth];
            var directions = new Direction[] { Direction.Up, Direction.Right, Direction.Left, Direction.Down };
            bool hasSolution = false;
            foreach(var direction in directions)
            {
                foreach (var i in Range(0, BoardHeight))
                    foreach (var j in Range(0, BoardWidth))
                        _numbers[i, j] = prevNumbers[i, j];
                if (Update(direction).HasMoved == true)
                {
                    hasSolution = true;
                    break;
                }
            }                
            //Restore states.
            _numbers = prevNumbers;
            _transformInfo = prevTransformations;
            _currentDirection = prevDirection;
            return hasSolution;
        }

        public IEnumerable<Coordinate> GetEmptyCells()
        {
            return from i in Range(0, 4)
                   from j in Range(0, 4)
                   where _numbers[i, j] == 0
                   select new Coordinate(i, j);
        }

        private int SmoothnessInternal(uint list)
        {
            int smoothness = 0;
            if (list > 0)
            {
                const uint mask = 0xFF;
                var previous = (byte)(list & mask);
                list >>= 8;
                byte current;
                while (list != 0)
                {
                    current = (byte)(list & mask);
                    smoothness -= Math.Abs(current - previous);
                    list >>= 8;
                    previous = current;
                }
            }
            return smoothness;
        }

        public int Smoothness()
        {
            int smoothness = 0;
            uint list = 0;
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    var number = _numbers[i, j];
                    if (number != 0)
                    {
                        list <<= 8;
                        list |= number;
                    }
                }
                smoothness += SmoothnessInternal(list);
                list = 0;
            }

            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    var number = _numbers[j, i];
                    if (number != 0)
                    {
                        list <<= 8;
                        list |= number;
                    }
                }
                smoothness += SmoothnessInternal(list);
                list = 0;
            }
            return smoothness;
        }

        public int Monotonicity()
        {
            int left = 0, right = 0, up = 0, down = 0;
            for(int i = 0; i < 4; ++i)
            {
                int current = 0;
                int next = 1;
                while(next < 4)
                {
                    while (next < 4 && _numbers[i, next] == 0)
                        ++next;
                    if (next >= 4) --next;
                    var currentValue = _numbers[i, current];
                    var nextValue = _numbers[i, next];
                    if (currentValue < nextValue)
                        left += currentValue - nextValue;
                    else
                        right += nextValue - currentValue;
                    current = next;
                    ++next;
                }                
            }
            for (int i = 0; i < 4; ++i)
            {
                int current = 0;
                int next = 1;
                while (next < 4)
                {
                    while (next < 4 && _numbers[next, i] == 0)
                        ++next;
                    if (next >= 4) --next;
                    var currentValue = _numbers[current, i];
                    var nextValue = _numbers[next, i];
                    if (currentValue < nextValue)
                        up += currentValue - nextValue;
                    else
                        down += nextValue - currentValue;
                    current = next;
                    ++next;
                }
            }
            return Math.Max(left, right) + Math.Max(up, down);
        }

        public int MaxNumber()
        {
            int ret = int.MinValue;
            for(int i = 0; i < 4; ++i)
                for(int j = 0; j < 4; ++j)
                    ret = Math.Max(ret, _numbers[i, j]);
            return ret;
        }

        public int Coherence()
        {
            var directions = new Direction[] { Direction.Up, Direction.Left, Direction.Right, Direction.Down };
            var game = new Game(this);
            int ret = int.MinValue;
            foreach (var direction in directions)
            {
                ret = Math.Max((int)game.Update(direction).MergedCount, ret);
                for (int i = 0; i < 4; ++i)
                    for (int j = 0; j < 4; ++j)
                        game.AddCellTrivial(i, j, _numbers[i, j]);
            }
            return ret;
        }

        private unsafe void PushStack(ref int* waterline, int value)
        {
            *waterline = value;
            ++waterline;
        }

        private unsafe int PopStack(ref int* waterline)
        {
            --waterline;
            return *waterline;
        }

        private unsafe void DispersionInternal(ref ushort hasVisited, int* stack, ref int* waterline)
        {
            //找到一个起点。
            var visited = hasVisited;
            int pos = 0;
            ushort mask = 1;
            while ((visited & mask) != 0)
            {
                mask <<= 1;
                ++pos;
            }

            PushStack(ref waterline, pos);
            hasVisited |= (ushort)(1 << pos);
            while(stack != waterline)
            {
                var newPos = PopStack(ref waterline);
                for(int i = 0; i < 4; ++i)
                {
                    var p = newPos + _directions[i];
                    if (p >= 0 && p < 16)
                    {
                        if ((hasVisited & (ushort)(1 << p)) == 0)
                        {
                            hasVisited |= (ushort)(1 << p);
                            //var number = _numbers[p / 4, p % 4];
                            //Debug.Assert(number != 0);                            
                            PushStack(ref waterline, p);
                        }
                    }                                            
                }
            }
        }

        //分散度
        public unsafe int Dispersion()
        {
            //找图的连通分量。从每个顶点做 DFS，把经过的顶点都做以标记。      
            ushort hasVisited = 0;
            int res = 0;
            //预先将为 0 的顶点标记位已访问。
            for(int i = 0; i < 4; ++i)
                for(int j = 0; j < 4; ++j)
                    if (_numbers[i, j] == 0)
                        hasVisited |= (ushort)(1 << (i * 4) + j);

            var stack = stackalloc int[16];
            int* waterline = stack;
            while (hasVisited != 0xFFFF)
            {
                DispersionInternal(ref hasVisited, stack, ref waterline);
                ++res;
            }
            return res;
        }

        public override int GetHashCode()
        {
            return Numbers.GetHashCode();
        }
       
        public bool Equals(Game other)
        {
            for (int i = 0; i < BoardHeight; ++i)
                for (int j = 0; j < BoardWidth; ++j)
                    if (Numbers[i, j] != other.Numbers[i, j]) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as Game);
        }

        public ResultInfo Update(Direction direction)
        {
            ResetTransformation();
            CurrentDirection = direction;
            //Logger($"Moved to {direction}");
            var resultInfo = new ResultInfo();
            for (int j = 0; j < 4; ++j)
            {              
                foreach (var i in GenerateSequence())
                {
                    var vec = PackAsUInt(j);
                    var currentPos = MakeCoordinate(i, j);
                    var current = Numbers.GetValue(currentPos);
                    if (current != 0)
                    {                     
                        var findResult = FindFirstNonEmpty(vec, ConvertPosition(i) + 1);
                        var frontPos = MakeCoordinate(findResult.Position, j);
                        var frontPrevPos = MakeCoordinate(findResult.PreviousPosition, j);
                        if(IsEdge(findResult.PreviousPosition) || (currentPos != frontPrevPos && 
                            !CoundBeMerged(currentPos, frontPos)))
                        {
                            MergeCell(currentPos, frontPrevPos);
                            resultInfo.HasMoved = true;
                            //Logger($"{currentPos} combined with 1 {frontPrevPos}");
                        }
                        else 
                        {
                            var front = Numbers.GetValue(frontPos);
                            if (front == current)
                            {
                                if (front == 10)
                                    resultInfo.HasWon = true;
                                resultInfo.HasMoved = true;
                                MergeCell(currentPos, frontPos);
                                resultInfo.MergedCount += 1;
                                //Logger($"{currentPos} combined with 2 {frontPos}");                           
                            }                                                  
                        }
                    }
                }
            }
            //LogGrid();
            return resultInfo;
        }

        private void ResetInternal(ref TransformInfo info)
        {
            info.Destination = Coordinate.Nowhere;
            info.WasNew = false;
            info.PreviousNumber = byte.MaxValue;
        }

        public void ResetTransformation()
        {
            for (int i = 0; i < BoardHeight; ++i)
                for (int j = 0; j < BoardHeight; ++j)
                    ResetInternal(ref Transformations[i, j]);
        }

        private Coordinate MakeCoordinate(int i, int j)
        {
            switch (_currentDirection)
            {
                case Direction.Left:
                case Direction.Right:
                    return new Coordinate(j, i);
                case Direction.Up:
                case Direction.Down:
                    return new Coordinate(i, j);
                default:
                    break;
            }
            throw new InvalidOperationException();
        }

        private void AddCell(Coordinate c, byte value)
        {
            Numbers.Set2DValue(c, value);
            Transformations.Set2DValue(c, new TransformInfo
            {
                WasNew = true,
                Destination = Coordinate.Nowhere,
                PreviousNumber = byte.MaxValue
            });
        }

        public void RemoveCellTrivial(Coordinate c)
        {
            _numbers[c.X, c.Y] = 0;
        }

        public void RemoveCellTrivial(int i, int j)
        {
            _numbers[i, j] = 0;
        }

        public void AddCellTrivial(int i, int j, byte value)
        {
            _numbers[i, j] = value;
        }

        public void AddCellTrivial(Coordinate c, byte value)
        {
            _numbers[c.X, c.Y] = value;
        }

        public ushort GetBitmap()
        {
            ushort res = 0;
            for(int i = 3; i >= 0; --i)
                for(int j = 3; j >= 0; --j)
                {
                    res <<= 1;
                    res |= (ushort)(_numbers[i, j] == 0 ? 0 : 1);
                }
            return res;
        }

        public int EmptyNumberCount()
        {
            int count = 0;
            for (int i = 0; i < 4; ++i)
                for (int j = 0; j < 4; ++j)
                    if (_numbers[i, j] == 0) ++count;
            return count;
        }

        private bool CoundBeMerged(Coordinate backCell, Coordinate frontCell)
        {
            var desInfo = Transformations.GetValue(frontCell);
            var frontValue = Numbers.GetValue(frontCell);
            var backValue = Numbers.GetValue(backCell);
            return desInfo.WasNew == false && frontValue == backValue;
        }

        private void MergeCell(Coordinate backCell, Coordinate frontCell)
        {
            Debug.Assert(backCell != frontCell);
            var desInfo = Transformations.GetValue(frontCell);
            if(!desInfo.WasNew)
            {
                var frontValue = Numbers.GetValue(frontCell);
                var backValue = Numbers.GetValue(backCell);
                bool wasNew = frontValue == backValue;
                Numbers.Set2DValue(frontCell, (byte)(backValue + (wasNew ? 1 : 0)));
                Numbers.Set2DValue(backCell, default(byte));
                //ResetInternal(ref Transformations[frontCell.X, frontCell.Y]);
                Transformations[frontCell.X, frontCell.Y].WasNew = wasNew;
                Transformations.Set2DValue(backCell, new TransformInfo
                {
                    WasNew = false,
                    Destination = frontCell,
                    PreviousNumber = Convert(backValue)
                });
            }            
        }

        public Coordinate GenerateEgg()
        {
            var avaliableCells = from x in Range(0, Numbers.GetLength(0))
                                 from y in Range(0, Numbers.GetLength(1))
                                 where Numbers[x, y] == 0
                                 select new Coordinate(x, y);
            var avaliableCount = avaliableCells.Count();
            var index = rand.Next(0, avaliableCount);
            var c = avaliableCells.ElementAt(index);
            AddCell(c, 1);
            Logger($"Egg generated at pos {c}");
            return c;
        }

        private static Random rand = new Random((int)DateTime.Now.ToBinary());

        [Conditional("DEBUG")]
        public void LogGrid()
        {
            var sb = new StringBuilder();
            int i2 = 0;
            foreach (var n in TraverseNumbers())
            {
                sb.Append(n);
                ++i2;
                if (i2 % 4 == 0)
                    sb.Append('\n');
            }
            Logger(sb.ToString());
        }

        [Conditional("DEBUG")]
        static public void Logger(string lines)
        {

            // Write the string to a file.append mode is enabled so that the log
            // lines get appended to  test.txt than wiping content and writing the log
            using (var file = new StreamWriter(".\\log.txt", true))
            {
                file.WriteLine(lines);
            }
        }
    }
}
