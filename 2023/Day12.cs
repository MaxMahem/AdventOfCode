namespace AdventOfCode._2023;

using System.Diagnostics.CodeAnalysis;

using Sprache;

using AdventOfCode.Helpers;

using CacheDict = Dictionary<(string, int[]), long>;

public class Day12 : AdventBase
{
    IEnumerable<SpringMaintanceRecord>? _maintanceRecords;

    protected override void InternalOnLoad() {
        _maintanceRecords = SpringMaintanceRecordParser.SpringMaintanceRecords.Parse(Input.Text);
    }

    protected override object InternalPart1() {
        long sum = 0;
        foreach(var maintanceRecord in _maintanceRecords!) {
            var pattern = maintanceRecord.SpringRecord.AsSpan();
            sum += maintanceRecord.Solve();
        }
        return sum;
    }

    protected override object InternalPart2() {
        long sum = 0;
        foreach (var maintanceRecord in _maintanceRecords!) {
            var expandedRecord = maintanceRecord.Repeat(5);
            var pattern = expandedRecord.SpringRecord.AsSpan();
            sum += expandedRecord.Solve();
        }
        return sum;
    }
}

public class SpringMaintanceRecord(IEnumerable<int> springSizes, string springRecord)
{
    public IEnumerable<int> SpringSizes { get; } = springSizes.ToImmutableArray();
    public string SpringRecord { get; } = springRecord;

    private readonly CacheDict _cache = new(new MaintKeyComparer());

    public SpringMaintanceRecord Repeat(int copies) {
        var springSizes  = Enumerable.Repeat(SpringSizes,  copies).SelectMany(e => e);
        var springRecord = Enumerable.Repeat(SpringRecord, copies);
        return new SpringMaintanceRecord(springSizes, string.Join('?', springRecord));
    }

    public long Solve() => Solve(this.SpringRecord.AsSpan(), this.SpringSizes);

    private long Solve(ReadOnlySpan<char> pattern, IEnumerable<int> springSizes) {
        if (_cache.TryGetValue(new (pattern.ToString(), springSizes.ToArray()), out long cachedResult)) return cachedResult;
        switch(pattern) {
            case []:                                          // No more spring patterns, exit path of recurssion.
                return springSizes.Any() ? 0 : 1;             // If no more spring sizes when pattern empty, then a valid combination found.
            case ['.', ..]:
                return CacheAndCompute(pattern.Trim('.'), springSizes); // consume any following dots and recurse.
            case ['?', ..]:                                          // for ? case, recurse as a dot and a hash
                return CacheAndCompute(['#', .. pattern[1..]], springSizes) + // prepend hash, skip the first character and pass the remainding pattern.
                       CacheAndCompute(pattern[1..].Trim('.'), springSizes);  // skip the first character and any following dots, and pass the remainding pattern.
            case ['#', ..]:                                   // hash 
                if (!springSizes.Any()) return 0; // if no more numbers, fail.
                int nextSpringSize = springSizes.First();

                // find out how much space remains for the next spring. If bigger than the spring, fail.
                var possibleSpringSpace = pattern.IndexOfAnyExcept('?', '#');
                possibleSpringSpace = possibleSpringSpace == -1 ? pattern.Length : possibleSpringSpace;
                if (nextSpringSize > possibleSpringSpace) return 0;

                // check if this spring will consume the rest of the space, if so, apply the end check above.
                if (nextSpringSize == pattern.Length) return springSizes.Skip(1).Any() ? 0 : 1;

                // if next space is a live spring this combination is invalid.
                if (pattern[nextSpringSize] == '#') return 0;

                // all checks pass, this pattern is valid, consume spring and recurse.
                return CacheAndCompute(pattern[(nextSpringSize+1)..], springSizes.Skip(1));
            default:
                throw new InvalidOperationException("Invalid pattern, should never get here.");
        }
    }
    private long CacheAndCompute(ReadOnlySpan<char> pattern, IEnumerable<int> springSizes) {
        var key = (pattern.ToString(), springSizes.ToArray());
        if (_cache.TryGetValue(key, out var result)) return result;
        _cache[key] = Solve(pattern, springSizes);
        return _cache[key];
    }

    private class MaintKeyComparer : IEqualityComparer<(string, int[])>     {
        public bool Equals((string, int[]) x, (string, int[]) y) => x.Item1 == y.Item1 && x.Item2.SequenceEqual(y.Item2);
        public int GetHashCode([DisallowNull] (string, int[]) obj) =>
            HashCode.Combine(obj.Item1, obj.Item2.Aggregate(0, HashCode.Combine));
    }
}

internal static class SpringMaintanceRecordParser
{
    static readonly Parser<int> NumberParser = Parse.Number.Select(int.Parse);

    public static readonly Parser<IEnumerable<int>> SpringSizes =
        NumberParser.DelimitedBy(Parse.Char(','));

    public static readonly Parser<string> SpringRecord =
        Parse.Chars("?#.").XMany().Text();

    public static readonly Parser<SpringMaintanceRecord> SpringMaintanceRecord =
        from springRecord in SpringRecord
        from whiteSpace in Parse.WhiteSpace
        from springSizes in SpringSizes
        select new SpringMaintanceRecord(springSizes, springRecord);

    public static readonly Parser<IEnumerable<SpringMaintanceRecord>> SpringMaintanceRecords =
        SpringMaintanceRecord.DelimitedBy(Parse.LineEnd);
}