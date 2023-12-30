namespace AdventOfCode.Helpers;

public readonly record struct GridRectangle<T>(GridPoint<T> NWCorner, GridPoint<T> SECorner)
    where T : IBinaryNumber<T>, ISignedNumber<T> {
    public readonly GridPoint<T> SWCorner => (NWCorner.X, SWCorner.Y);
    public readonly GridPoint<T> NECorner => (NECorner.X, SWCorner.X);
    public readonly T Width => T.Abs(NWCorner.X - SECorner.X)  + T.One;
    public readonly T Height => T.Abs(NWCorner.Y - SECorner.Y) + T.One;

    public GridRectangle(T width, T height, T x, T y) : this((x, y), (x + width, y + height)) { }

    public GridRectangle(T width, T height) : this(width, height, T.Zero, T.Zero) { }

    public readonly bool ContainsPoint(GridPoint<T> point) =>
        point.X >= NWCorner.X && point.X < SWCorner.X &&
        point.Y >= NWCorner.Y && point.Y < SWCorner.Y;

    public readonly IEnumerable<GridPoint<T>> ContainedPoints {
        get {
            for (T x = NWCorner.X; x <= SECorner.X; x++) {
                for (T y = NWCorner.Y; y <= SECorner.Y; y++) {
                    yield return (x, y);
                }
            }
        }
    }

    public readonly IEnumerable<GridPoint<T>> Border {
        get {
            for (T x = this.NWCorner.X; x <= this.SECorner.X; x++) {
                yield return (x, NWCorner.Y); // Top side
                yield return (x, SECorner.Y); // Bottom side
            }

            for (T y = this.NWCorner.Y + T.One; y < this.SECorner.Y; y++) {
                yield return (NWCorner.X, y); // Left side
                yield return (SECorner.X, y); // Right side
            }
        }
    }

    public static implicit operator GridRectangle<T>((T Width, T Height) size) 
        => new(size.Width, size.Height, T.Zero, T.Zero);

    public readonly GridRectangle<T> Grow(T value) 
        => new((NWCorner.X - value, NWCorner.Y - value), (SECorner.X + value, SECorner.Y + value));
}