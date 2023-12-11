namespace AdventOfCode.Helpers;

public record struct Point<T>(T X, T Y) where T : INumber<T>, ISignedNumber<T>;
