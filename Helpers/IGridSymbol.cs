namespace AdventOfCode.Helpers;

using IntPoint = GridPoint<int>;

public interface IGridSymbol
{
    public IntPoint Location { get; }
    public char Symbol { get; }
}
