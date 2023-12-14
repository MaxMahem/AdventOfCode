namespace AdventOfCode.Helpers;

public record struct Point<T>(T X, T Y) where T : INumber<T>, ISignedNumber<T> {
    public static Point<T> operator +(Point<T> left, Point<T> right) => new(left.X + right.X, left.Y + right.Y);
    public static Point<T> operator -(Point<T> left, Point<T> right) => new(left.X - right.X, left.Y - right.Y);
}