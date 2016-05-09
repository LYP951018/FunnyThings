namespace _2048
{
    public struct Coordinate
    {
        private int _x, _y;

        public int X { get { return _x; } set { _x = value; } }
        public int Y { get { return _y; } set { _y = value; } }

        public static readonly Coordinate Nowhere = new Coordinate(-1, -1);

        public Coordinate(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public override string ToString()
        {
            return $"{X}{Y}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (GetType() != obj.GetType()) return false;
            return Equals((Coordinate)obj);
        }

        public bool Equals(Coordinate other)
        {
            return other.X == X && other.Y == Y;
        }

        public override int GetHashCode()
        {
            return (X.GetHashCode() << 16) ^ Y.GetHashCode();
        }

        public static bool operator ==(Coordinate lhs, Coordinate rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Coordinate lhs, Coordinate rhs)
        {
            return !(lhs == rhs);
        }
    }
}
