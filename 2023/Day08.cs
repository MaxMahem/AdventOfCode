namespace AdventOfCode._2023;

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
        _map!.Nodes.Where(kvp => kvp.Key[2] == 'A').Select(kvp => kvp.Value)
                   .Select(node => _map.Navigate(node, (Node n) => n.Key[2] == 'Z'))
                   .LCM();
}

public class GhostMap(string directions, IEnumerable<Node> nodes)
{
    public string Directions { get; } = string.IsNullOrEmpty(directions) ? throw new ArgumentException("Cannot be empty", nameof(directions))
                                                                         : directions;
    public IReadOnlyDictionary<string, Node> Nodes { get; } = nodes.ToImmutableDictionary(node => node.Key);

    public Node Step(Node node, char direction) => direction switch {
        'L' => Nodes[node.Left],
        'R' => Nodes[node.Right],
        _ => throw new ArgumentException("Invalid direction.", nameof(direction))
    };

    public List<(int, Node)> FindLoop(Node start) {
        List<(int, Node)> seenNodes = new(Nodes.Count);
        var navigator = Directions.GetLoopingEnumerator();
        Node currentNode = start;

        do {
            navigator.MoveNext();
            currentNode = Step(currentNode, navigator.Current);

            if (seenNodes.Contains((navigator.CurrentIndex, currentNode))) {
                seenNodes.Add((navigator.Current, currentNode));
                return seenNodes;
            }

            seenNodes.Add((navigator.Current, currentNode));
        } while (true);
    }

    public int Navigate(string start, string end) => Navigate(Nodes[start], Nodes[end]);
    public int Navigate(Node start, Node end) => Navigate(start, node => node == end);
    public int Navigate(Node start, Func<Node, bool> endPredicate) {
        var loopingEnumerator = Directions.GetLoopingEnumerator();
        Node current = start;

        int steps = 0;
        do {
            loopingEnumerator.MoveNext();
            current = Step(current, loopingEnumerator.Current);
            steps++;
        } while (!endPredicate(current));

        return steps;
    }
}

public record struct Node(string Key, string Left, string Right);

internal static class GhostMapParser
{
    static readonly Parser<string> MapKeyParser = Parse.LetterOrDigit.Repeat(3).Text();

    public static readonly Parser<string> Directions =
        from directions in Parse.Chars("LR").Until(Parse.LineEnd).Text()
        select directions;

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