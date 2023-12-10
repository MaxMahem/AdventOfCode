namespace AdventOfCode._2023;

using System.Collections;

using Sprache;

using AdventOfCode.Helpers;

public class Day08 : AdventBase
{
    GhostMap? _map;

    protected override void InternalOnLoad() {
        _map = GhostMapParser.MapParser.Parse(Input.Text);
    }

    protected override object InternalPart1() => _map!.Navigate("AAA", "ZZZ");

    protected override object InternalPart2() =>
        _map!.Nodes.Where(kvp => kvp.Key.Check('A', 2)).Select(kvp => kvp.Value)
                   .Select(node => _map.Navigate(node, (Node n) => n.Key.Check('Z', 2)))
                   .LCM();
}

public class GhostMap(Directions directions, IEnumerable<Node> nodes) {
    public Directions Directions { get; } = directions;

    public IReadOnlyDictionary<NodeKey, Node> Nodes { get; } = nodes.ToImmutableDictionary(node => node.Key);

    public int Navigate(string start, string end) => Navigate(Nodes[start], Nodes[end]);
    public int Navigate(Node start, Node end) => Navigate(start, node => node == end);
    public int Navigate(Node start, Func<Node, bool> endPredicate) {
        var loopingEnumerator = Directions.GetEnumerator().MakeLooping();
        Node current = start;

        int steps = 0;
        do {
            loopingEnumerator.MoveNext();
            current = Step(current, loopingEnumerator.Current);
            steps++;
        } while (!endPredicate(current));

        return steps;
    }

    private Node Step(Node node, bool direction) => direction ? Nodes[node.Left] : Nodes[node.Right];
}

public readonly struct Directions(string directions) : IEnumerable<bool> 
{
    private readonly BitArray _directions = string.IsNullOrEmpty(directions) ? throw new ArgumentException("Cannot be empty", nameof(directions))
                                                                             : EncodeDirections(directions);

    public IEnumerator<bool> GetEnumerator() => _directions.GetTypedEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _directions.GetTypedEnumerator();

    private static BitArray EncodeDirections(string directions) {
        BitArray encodedDirections = new(directions.Length);
        for (int directionIndex = 0; directionIndex < directions.Length; directionIndex++) {
            bool directionBit = directions[directionIndex] switch {
                'L' => true,
                'R' => false,
                _ => throw new ArgumentException("Invalid direction symbol.", nameof(directions))
            };
            encodedDirections[directionIndex] = directionBit;
        }
        return encodedDirections;
    }
}

public record struct Node(NodeKey Key, NodeKey Left, NodeKey Right) { }

public record struct NodeKey(int Key) {
    public NodeKey(IEnumerable<char> text) : this(Encode(text)) { }

    public static implicit operator NodeKey(string text) => new(text);

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

internal static class GhostMapParser
{
    private static readonly Parser<NodeKey> MapKeyParser = Parse.LetterOrDigit.Repeat(3).Select(e => new NodeKey(e));

    public static readonly Parser<Directions> Directions = Parse.Chars("LR").Until(Parse.LineEnd).Text().Select(s => new Directions(s));

    public static readonly Parser<Node> NodeParser =
        from key in MapKeyParser.Token()
        from equal in Parse.Char('=').Token()
        from open in Parse.Char('(')
        from left in MapKeyParser
        from deliminator in Parse.Char(',').Token()
        from right in MapKeyParser
        from close in Parse.Char(')')
        from eol in Parse.LineEnd.Optional()
        select new Node(key, left, right);

    public static readonly Parser<GhostMap> MapParser =
        from directions in Directions
        from nodes in NodeParser.XMany()
        select new GhostMap(directions, nodes);
}