namespace Snake
{
    public struct Coordinate
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coordinate(int x,int y)
        {
            X = x;
            Y = y;
        }

        public override int GetHashCode()
        {
            return (X.GetHashCode() + Y.GetHashCode()).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this.GetType() != obj.GetType()) return false;
            return Equals((Coordinate)obj);
        }

        public bool Equals(Coordinate rhs)
        {
            return X == rhs.X && Y == rhs.Y;
        }

        public static bool operator == (Coordinate lhs,Coordinate rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Coordinate lhs, Coordinate rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
