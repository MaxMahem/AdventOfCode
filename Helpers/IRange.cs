namespace AdventOfCode.Helpers;

using System.Numerics;

public interface IRange<T> where T : INumber<T>
{
    T End { get; init; }
    T Start { get; init; }

    bool Contains(T value);
}