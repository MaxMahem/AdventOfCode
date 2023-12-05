namespace AdventOfCode._2023;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Sprache;

using AdventOfCodeSupport;
using AdventOfCode.Helpers;

using BoxSet = IReadOnlyDictionary<BoxColor, int>;

public class Day02 : AdventBase
{
    IEnumerable<Game>? _games;

    protected override void InternalOnLoad() {
        _games = GameParser.GameFile.Parse(Input.Text);
    }

    protected override object InternalPart1() => 
        _games!.Select(game => game.ValidateSetMaximums(maxRed: MAX_RED, maxGreen: MAX_GREEN, maxBlue: MAX_BLUE) ? game.Id : 0).Sum();

    protected override object InternalPart2() =>
        _games!.Select(game => game.GetMinPossibleSet().Values.Product()).Sum();

    public const int MAX_RED   = 12;
    public const int MAX_GREEN = 13;
    public const int MAX_BLUE  = 14;
}

public readonly record struct Game(int Id, IEnumerable<BoxSet> Rounds) {
    public readonly bool ValidateSetMaximums(int maxRed, int maxGreen, int maxBlue) => 
        Rounds.All(round => round.ValidateMaxiums(maxRed, maxGreen, maxBlue));

    public readonly BoxSet GetMinPossibleSet() => Rounds.Aggregate(BoxSetExtensions.MaxOfTwoSets);
}

public static class BoxSetExtensions {
    public static void Deconstruct(this BoxSet boxSet, out int red, out int green, out int blue) {
        red   = boxSet.TryGetValue(BoxColor.Red,   out red)   ? red   : 0;
        green = boxSet.TryGetValue(BoxColor.Green, out green) ? green : 0;
        blue  = boxSet.TryGetValue(BoxColor.Blue,  out blue)  ? blue  : 0;
    }
    public static bool ValidateMaxiums(this BoxSet boxSet, int maxRed, int maxGreen, int maxBlue) {
        (int red, int green, int blue) = boxSet;
        return red <= maxRed && green <= maxGreen && blue <= maxBlue;
    }
    public static BoxSet MaxOfTwoSets(this BoxSet leftSet, BoxSet rightSet) {
        (int lRed, int lGreen, int lBlue) = leftSet;
        (int rRed, int rGreen, int rBlue) = rightSet;
        return Create((Math.Max(lRed, rRed), Math.Max(lGreen, rGreen), Math.Max(lBlue, rBlue)));
    }

    public static BoxSet Create((int Red, int Green, int Blue) set) => new Dictionary<BoxColor, int>{
        { BoxColor.Red,   set.Red },
        { BoxColor.Green, set.Green },
        { BoxColor.Blue,  set.Blue }
    }.ToImmutableDictionary();
}
public enum BoxColor { Red, Green, Blue }

internal static class GameParser
{
    static readonly Parser<char> BoxSeparator = Parse.Char(',');
    static readonly Parser<char> SetSeparator = Parse.Char(';');
    static readonly Parser<char> IdSeperator = Parse.Char(':');
    static readonly Parser<int> NumberParser = Parse.Number.Select(int.Parse);
    
    public static readonly Parser<BoxColor> Color
        = Parse.Letter.AtLeastOnce().Text().Select(GetBoxColor);
    
    public static readonly Parser<int> GameId = 
        from identifier in Parse.String("Game").Token()
        from id in NumberParser
        select id;

    public static readonly Parser<KeyValuePair<BoxColor, int>> BoxCount =
        from count in NumberParser.Token()
        from color in Color.Token()
        select new KeyValuePair<BoxColor, int>(color, count);

    public static readonly Parser<BoxSet> BoxSets =
        from boxSets in BoxCount.DelimitedBy(BoxSeparator.Token())
        select boxSets.ToImmutableDictionary();

    public static readonly Parser<Game> Game =
        from id in GameId
        from colon in IdSeperator.Token()
        from sets in BoxSets.DelimitedBy(SetSeparator)
        from eol in Parse.LineEnd.Optional()
        select new Game(id, sets);

    public static readonly Parser<IEnumerable<Game>> GameFile =
        from games in Game.XMany().End()
        select games;

    static BoxColor GetBoxColor(string color) => color switch {
        "red"   => BoxColor.Red,
        "green" => BoxColor.Green,
        "blue"  => BoxColor.Blue,
        _ => throw new ArgumentException("Invalid Color.", nameof(color)),
    };
}