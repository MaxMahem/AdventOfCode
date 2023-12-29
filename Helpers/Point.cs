namespace AdventOfCode.Helpers;

public readonly record struct Point<T>(T X, T Y) where T : INumber<T>, ISignedNumber<T> {
    public static Point<T> operator +(Point<T> left, Point<T> right) => new(left.X + right.X, left.Y + right.Y);
    public static Point<T> operator -(Point<T> left, Point<T> right) => new(left.X - right.X, left.Y - right.Y);

    public static implicit operator Point<T>((T X, T Y) point) => new(point.X, point.Y);

    public static readonly Point<T> Origin = new(+T.Zero, +T.Zero);
}

public readonly record struct Direction<T>() where T : INumber<T>, ISignedNumber<T> {
    public T X { get; }
    public T Y { get; }

    private Direction(T x, T y) : this() => (this.X, this.Y) = (x, y);

    public void Deconstruct(out T X, out T Y) => (X, Y) = (this.X, this.Y);

    // public static implicit operator Direction<T>((T X, T Y) point)   => new(point.X,     point.Y);
    public static implicit operator Point<T>(Direction<T> direction) => new(direction.X, direction.Y);

    public static readonly Direction<T> None  = new(+T.Zero, +T.Zero);

    public static readonly Direction<T> North = new(+T.Zero, -T.One);
    public static readonly Direction<T> South = new(+T.Zero, +T.One);
    public static readonly Direction<T> East  = new(+T.One,  +T.Zero);
    public static readonly Direction<T> West  = new(-T.One,  +T.Zero);

    public Point<T> ToPoint() => new(this.X, this.Y);
}