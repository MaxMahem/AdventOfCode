namespace AdventOfCode._2023;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Sprache;

using AdventOfCodeSupport;

using AdventOfCode.IEnumerableHelpers;


using BoxSet = IReadOnlyDictionary<BoxColor, int>;

public class Day02 : AdventBase
{
    IEnumerable<Game>? _games;

    protected override void InternalOnLoad() {
        _games = GameParser.GameSet.Parse(Input.Text);
    }

    protected override object InternalPart1() => 
        _games!.Select(game => game.Validate(maxRed: MAX_RED, maxGreen: MAX_GREEN, maxBlue: MAX_BLUE) ? game.Id : 0).Sum();

    protected override object InternalPart2() =>
        _games!.Select(game => game.GetMinPossibleSet().Values.Product()).Sum();

    public const int MAX_RED   = 12;
    public const int MAX_GREEN = 13;
    public const int MAX_BLUE  = 14;
}

public readonly record struct Game(int Id, IEnumerable<BoxSet> Rounds) {
    public readonly bool Validate(int maxRed, int maxGreen, int maxBlue) => 
        Rounds.All(round => round.Validate(maxRed, maxGreen, maxBlue));

    public readonly BoxSet GetMinPossibleSet() {
        (int minRed, int minBlue, int minGreen) = (0, 0, 0);
        foreach (var boxSet in Rounds) {
            (int red, int green, int blue) = boxSet;
            (minRed, minGreen, minBlue) = (Math.Max(minRed, red), Math.Max(minGreen, green), Math.Max(minBlue, blue));
        }
        return new Dictionary<BoxColor, int>{ 
            { BoxColor.Red,   minRed }, 
            { BoxColor.Green, minGreen }, 
            { BoxColor.Blue,  minBlue } 
        };
    }
}

public static class BoxSetExtensions {
    public static bool Validate(this BoxSet boxSet, int maxRed, int maxGreen, int maxBlue) {
        (int red, int green, int blue) = boxSet;
        return red <= maxRed && green <= maxGreen && blue <= maxBlue;
    }

    public static void Deconstruct(this BoxSet boxSet, out int red, out int green, out int blue) {
        red   = boxSet.TryGetValue(BoxColor.Red,   out red)   ? red   : 0;
        green = boxSet.TryGetValue(BoxColor.Green, out green) ? green : 0;
        blue  = boxSet.TryGetValue(BoxColor.Blue,  out blue)  ? blue  : 0;
    }
}
public enum BoxColor { Red, Green, Blue }

internal static class GameParser
{
    static readonly Parser<char> BoxSeparator = Parse.Char(',');
    static readonly Parser<char> SetSeparator = Parse.Char(';');
    static readonly Parser<int> NumberParser = Parse.Number.Select(int.Parse);
    public readonly static Parser<BoxColor> ColorParser 
        = Parse.Letter.AtLeastOnce().Text().Select(GetBoxColor);
    
    public readonly static Parser<int> GameId = 
        from identifier in Parse.String("Game").Token()
        from id in NumberParser
        select id;

    public readonly static Parser<KeyValuePair<BoxColor, int>> BoxCount =
        from count in NumberParser.Token()
        from color in ColorParser.Token()
        select new KeyValuePair<BoxColor, int>(color, count);

    public readonly static Parser<IReadOnlyDictionary<BoxColor, int>> BoxSets =
        from boxSets in BoxCount.DelimitedBy(BoxSeparator.Token())
        select boxSets.ToImmutableDictionary();

    public static Parser<Game> Game =
        from id in GameId
        from colon in Parse.String(":").Token()
        from sets in BoxSets.DelimitedBy(SetSeparator)
        from eol in Parse.LineEnd.Optional()
        select new Game(id, sets);

    public static Parser<IEnumerable<Game>> GameSet =
        from games in Game.XMany().End()
        select games;

    static BoxColor GetBoxColor(string color) => color switch {
        "red"   => BoxColor.Red,
        "green" => BoxColor.Green,
        "blue"  => BoxColor.Blue,
        _ => throw new ArgumentException("Invalid Color.", nameof(color)),
    };
}