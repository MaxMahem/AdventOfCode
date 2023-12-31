namespace AdventOfCode.Helpers;

public interface IGridPoint<T> where T : IBinaryNumber<T>, ISignedNumber<T> {
    T X { get; }
    T Y { get; }
}

public readonly record struct GridPoint<T>(T X, T Y) : IGridPoint<T> where T : IBinaryNumber<T>, ISignedNumber<T> {
    public static GridPoint<T> operator +(GridPoint<T> left, GridPoint<T> right) => new(left.X + right.X, left.Y + right.Y);
    public static GridPoint<T> operator -(GridPoint<T> left, GridPoint<T> right) => new(left.X - right.X, left.Y - right.Y);

    public static implicit operator GridPoint<T>((T X, T Y) point) => new(point.X, point.Y);

    public static readonly GridPoint<T> Origin = new(+T.Zero, +T.Zero);

    public static double Distance(IGridPoint<int> p1, IGridPoint<int> p2) {
        (int deltaX, int deltaY) = (p2.X - p1.X, p2.Y - p1.Y);
        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }
}

public readonly record struct GridDirection<T>() : IGridPoint<T> where T : IBinaryNumber<T>, ISignedNumber<T> {
    public T X { get; }
    public T Y { get; }

    private GridDirection(T x, T y) : this() => (this.X, this.Y) = (x, y);

    public void Deconstruct(out T X, out T Y) => (X, Y) = (this.X, this.Y);

    // public static implicit operator Direction<T>((T X, T Y) point)   => new(point.X,     point.Y);
    public static implicit operator GridPoint<T>(GridDirection<T> direction) => new(direction.X, direction.Y);
    public static implicit operator (T X, T Y)(GridDirection<T> direction)   => (direction.X, direction.Y);

    public static readonly GridDirection<T> None  = new(+T.Zero, +T.Zero);

    public static readonly GridDirection<T> North = new(+T.Zero, -T.One);
    public static readonly GridDirection<T> South = new(+T.Zero, +T.One);
    public static readonly GridDirection<T> East  = new(+T.One,  +T.Zero);
    public static readonly GridDirection<T> West  = new(-T.One,  +T.Zero);

    public GridPoint<T> ToPoint() => new(this.X, this.Y);
}