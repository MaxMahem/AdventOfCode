namespace AdventOfCode._2023;

using System.Collections;

using Sprache;

using AdventOfCode.Helpers;

public class Day08 : AdventBase {
    GhostMap? _map;

    protected override void InternalOnLoad() {
        _map = GhostMapParser.MapParser.Parse(Input.Text);
    }

    protected override object InternalPart1() => _map!.Navigate("AAA", "ZZZ");

    protected override object InternalPart2() =>
        _map!.Nodes.Where(kvp => kvp.Key.Check('A', 2)).Select(kvp => kvp.Value)
                   .Select(node => _map.Navigate(node, (GhostMapNode n) => n.Key.Check('Z', 2)))
                   .LCM();
}

public class GhostMap(GhostMapDirections directions, IEnumerable<GhostMapNode> nodes) {
    public GhostMapDirections Directions { get; } = directions;

    public IReadOnlyDictionary<GhostMapNodeKey, GhostMapNode> Nodes { get; } = nodes.ToImmutableDictionary(node => node.Key);

    public int Navigate(string start, string end) => Navigate(Nodes[start], Nodes[end]);
    public int Navigate(GhostMapNode start, GhostMapNode end) => Navigate(start, node => node == end);
    public int Navigate(GhostMapNode start, Func<GhostMapNode, bool> endPredicate) {
        var loopingEnumerator = Directions.GetEnumerator().MakeLooping();
        GhostMapNode current = start;

        int steps = 0;
        do {
            loopingEnumerator.MoveNext();
            current = Step(current, loopingEnumerator.Current);
            steps++;
        } while (!endPredicate(current));

        return steps;
    }

    private GhostMapNode Step(GhostMapNode node, bool direction) => direction ? Nodes[node.Left] : Nodes[node.Right];
}

public readonly struct GhostMapDirections(string directions) : IEnumerable<bool> {
    private readonly BitArray directions = string.IsNullOrEmpty(directions) ? throw new ArgumentException("Cannot be empty", nameof(directions))
                                                                            : directions.CreateBitArray(ParseDirection);

    public IEnumerator<bool> GetEnumerator() => directions.GetTypedEnumerator();
    IEnumerator IEnumerable.GetEnumerator()  => directions.GetTypedEnumerator();

    private static bool ParseDirection(char direction) => direction switch {
        'L' => true,
        'R' => false,
        _ => throw new ArgumentException("Invalid direction symbol.", nameof(direction))
    };
}

public record struct GhostMapNode(GhostMapNodeKey Key, GhostMapNodeKey Left, GhostMapNodeKey Right);

public record struct GhostMapNodeKey(int Key) {
    public GhostMapNodeKey(IEnumerable<char> text) : this(Encode(text)) { }

    public static implicit operator GhostMapNodeKey(string text) => new(text);

    public static int Encode(IEnumerable<char> text) {
        int key = 0; int index = 0;
        foreach (char c in text) {
            key |= c << index++ * 8;
            if (index > 4) throw new ArgumentException("Text to large to encode", nameof(text));
        }
        return key;
    }

    /// <summary>Checks if this key coresponds to a symbol a specific positon.</summary>
    /// <param name="index">The 0 based index of the symbol to check.</param>
    public readonly bool Check(char symbol, int index) =>
        (char)((this.Key >> (8 * index)) & 0xFF) == symbol;
}

public class GhostMapParser : SpracheParser {
    private static readonly Parser<GhostMapNodeKey> MapKeyParser = Parse.LetterOrDigit.Repeat(3).Select(e => new GhostMapNodeKey(e));

    public static readonly Parser<GhostMapDirections> Directions = Parse.Chars("LR").Until(Parse.LineEnd).Text().Select(s => new GhostMapDirections(s));

    public static readonly Parser<GhostMapNode> NodeParser =
        from key   in MapKeyParser.Token()
        from equal in Parse.Char('=').Token()
        from open  in Parse.Char('(')
        from left  in MapKeyParser
        from delim in Parse.Char(',').Token()
        from right in MapKeyParser
        from close in Parse.Char(')')
        from eol   in Parse.LineEnd.Optional()
        select new GhostMapNode(key, left, right);

    public static readonly Parser<GhostMap> MapParser =
        from directions in Directions
        from nodes      in NodeParser.XMany()
        select new GhostMap(directions, nodes);
}