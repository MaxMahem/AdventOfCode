namespace AdventOfCode._2023;

using AdventOfCode.Helpers;
using Sprache;
using System.Text;
using CacheDict = Dictionary<(int, int), long>;

public class Day12 : AdventBase
{
    IEnumerable<SpringMaintanceRecord>? _maintanceRecords;

    protected override void InternalOnLoad() {
        _maintanceRecords = SpringMaintanceRecordParser.SpringMaintanceRecords.Parse(Input.Text);
    }

    protected override object InternalPart1() => _maintanceRecords!.Select(record => record.FindCombinations()).Sum();

    protected override object InternalPart2() => _maintanceRecords!.Select(record => record.Repeat(5).FindCombinations()).Sum();
}

public class SpringMaintanceRecord(IEnumerable<int> springSizes, string springRecord)
{
    public IEnumerable<int> SpringSizes { get; } = springSizes?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(springSizes));
    public string SpringRecord { get; } = springRecord ?? throw new ArgumentNullException(nameof(springRecord));

    public SpringMaintanceRecord Repeat(int copies) {
        var springSizes  = Enumerable.Repeat(SpringSizes,  copies).SelectMany(e => e);
        var springRecord = SpringRecord.RepeatJoin('?', copies);
        return new SpringMaintanceRecord(springSizes, springRecord);
    }

    public long FindCombinations() {
        CacheDict cache = [];
        return Solve(this.SpringRecord.AsSpan(), this.SpringSizes.ToArray().AsSpan());

        long Solve(ReadOnlySpan<char> maintianceRecord, ReadOnlySpan<int> springSizes) {
            switch (maintianceRecord) {
                case []:
                    return springSizes.Length > 0 ? 0 : 1;                               // If no more spring sizes when pattern empty, then a valid combination found.
                case ['.', ..]:
                    return CacheAndSolve(maintianceRecord.Trim('.'), springSizes);       // consume any following dots and recurse.
                case ['?', ..]:                                                          // for ? case, recurse as a dot and a hash
                    return CacheAndSolve(['#', .. maintianceRecord[1..]], springSizes) + // prepend hash, skip the first character and pass the remainding pattern.
                           CacheAndSolve(maintianceRecord[1..].Trim('.'), springSizes);  // skip the first character and any following dots, and pass the remainding pattern.
                case ['#', ..]:
                    if (springSizes.Length == 0) return 0; // if no more numbers, fail.
                    int nextSpringSize = springSizes[0];

                    // find out how much space remains for the next spring. If bigger than the next spring, fail.
                    var possibleSpringSpace = maintianceRecord.IndexOfAnyExcept('?', '#');
                    possibleSpringSpace = possibleSpringSpace == -1 ? maintianceRecord.Length : possibleSpringSpace;
                    if (nextSpringSize > possibleSpringSpace) return 0;

                    // check if this spring will consume the rest of the space, if so, apply the end check above.
                    // this combined with the above check ensures that pattern is at least one longer than current spring size.
                    if (nextSpringSize == maintianceRecord.Length) return springSizes[1..].Length > 0 ? 0 : 1;

                    // if next space is a live spring this combination is invalid.
                    if (maintianceRecord[nextSpringSize] == '#') return 0;

                    // all checks pass, this pattern is valid, consume spring and pattern and recurse.
                    return CacheAndSolve(maintianceRecord[(nextSpringSize + 1)..], springSizes[1..]);
                default:
                    throw new InvalidOperationException("Invalid pattern."); // should never get here.
            }
        }

        // Engine for memoization. Before solving each result is checked against the cache to see if a result already exists.
        // If it does, that is returned, otherwise, that result is solved, cached, and returned.
        long CacheAndSolve(ReadOnlySpan<char> pattern, ReadOnlySpan<int> springSizes) {
            var key = (pattern.Length, springSizes.Length);
            if (cache.TryGetValue(key, out var result)) return result;

            cache[key] = Solve(pattern, springSizes);
            return cache[key];
        }
    }
}

internal static class SpringMaintanceRecordParser {
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