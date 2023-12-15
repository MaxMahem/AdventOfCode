namespace AdventOfCode.Helpers;

using IntPoint = Point<int>;

public interface IGridSymbol
{
    public IntPoint Location { get; }
    public char Symbol { get; }
}
