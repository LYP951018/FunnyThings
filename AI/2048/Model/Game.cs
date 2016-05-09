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
    }

    public class Game
    {
        public const int BoardWidth = 4;
        public const int BoardHeight = 4;

        private byte[,] _numbers = new byte[BoardHeight, BoardWidth];
        private TransformInfo[,] _transformInfo = new TransformInfo[BoardHeight, BoardWidth];

        private byte[,] Numbers => _numbers;
        public TransformInfo[,] Transformations => _transformInfo;

        private Direction _currentDirection;

        public Game()
        {
            ResetTransformation();
            Logger("---------------------Game starts!-------------------------");
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
            foreach (var i in Range(0, _numbers.GetLength(0)))
                foreach (var j in Range(0, _numbers.GetLength(1)))
                    yield return Convert(_numbers[i, j]);                   
        }

        private IEnumerable<int> PositiveSequence(int minValue, int maxValue)
        {
            for (int i = minValue; i < maxValue; ++i)
                yield return i;
        }

        private IEnumerable<int> NegativeSequence(int minValue, int maxValue)
        {
            for (int i = minValue; i < maxValue; ++i)
                yield return maxValue - 1 - i;
        }

        private IEnumerable<int> GenerateSequence(int minValue, int maxValue)
        {
            switch(_currentDirection)
            {
                case Direction.Down:
                case Direction.Right:
                    return NegativeSequence(minValue, maxValue);
                case Direction.Up:
                case Direction.Left:
                    return PositiveSequence(minValue, maxValue);
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
                    foreach(var i in Range(0, BoardHeight))
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
                    foreach (var i in Range(0, BoardHeight))
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

        public ResultInfo Update(Direction direction)
        {
            ResetTransformation();
            _currentDirection = direction;
            Logger($"Moved to {direction}");
            var resultInfo = new ResultInfo();
            for (int j = 0; j < 4; ++j)
            {              
                foreach (var i in GenerateSequence(1, BoardWidth))
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
                            Logger($"{currentPos} combined with 1 {frontPrevPos}");
                        }
                        else 
                        {
                            var front = Numbers.GetValue(frontPos);
                            if (front == current)
                            {
                                resultInfo.HasMoved = true;
                                MergeCell(currentPos, frontPos);
                                Logger($"{currentPos} combined with 2 {frontPos}");
                            }                                                  
                        }
                    }
                }
            }
            var sb = new StringBuilder();
            int i2 = 0;
            foreach(var n in TraverseNumbers())
            {
                sb.Append(n);
                ++i2;
                if (i2 % 4 == 0)
                    sb.Append('\n');
            }
            Logger(sb.ToString());
            return resultInfo;
        }

        public void ResetTransformation()
        {
            foreach (var i in Range(0, BoardHeight))
                foreach (var j in Range(0, BoardWidth))
                    Transformations[i, j] = new TransformInfo
                    {
                        Destination = Coordinate.Nowhere,
                        WasNew = false,
                        PreviousNumber = byte.MaxValue
                    };
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
                Transformations.Set2DValue(frontCell, new TransformInfo
                {
                    WasNew = wasNew,
                    Destination = Coordinate.Nowhere,
                    PreviousNumber = byte.MaxValue
                });
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

        public void Logger(string lines)
        {
#if DEBUG
            // Write the string to a file.append mode is enabled so that the log
            // lines get appended to  test.txt than wiping content and writing the log
            using (var file = new StreamWriter(".\\log.txt", true))
            {
                file.WriteLine(lines);
            }
#endif
        }
    }
}
