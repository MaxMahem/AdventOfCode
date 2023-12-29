namespace AdventOfCode.Helpers;

using Sprache;

public class SpracheParser
{
    public static readonly Parser<int> IntParser = Parse.Number.Select(int.Parse);
    public static readonly Parser<long> LongParser = Parse.Number.Select(long.Parse);
}
