namespace AdventOfCode.Helpers;

public record struct Rectangle<T>(T Width, T Height, T X, T Y) where T : INumber<T>, ISignedNumber<T> {
    public readonly bool ContainsPoint(Point<T> point) =>
        point.X >= X && point.X < X + Width &&
        point.Y >= Y && point.Y < Y + Height;
}