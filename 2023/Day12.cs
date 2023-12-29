namespace AdventOfCode._2023;

using AdventOfCode.Helpers;

using Sprache;

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
        return Dispatch(this.SpringRecord.AsSpan(), this.SpringSizes.ToArray().AsSpan());

        long Dispatch(ReadOnlySpan<char> maintRecord, ReadOnlySpan<int> springSizes) => maintRecord switch {
            []        => CheckRecordEnd(springSizes),
            ['.', ..] => CacheAndSolve(maintRecord.Trim('.'), springSizes),       // consume any following dots and recurse.
            ['?', ..] => CacheAndSolve(['#', .. maintRecord[1..]], springSizes) + // for ? case, recurse as a dot and a hash
                         CacheAndSolve(maintRecord[1..].Trim('.'), springSizes),  
            ['#', ..] => CheckLiveSpring(maintRecord, springSizes),
            _ => throw new InvalidOperationException("Invalid pattern."),
        };

        // If no more spring sizes when pattern empty, then a valid combination found.
        long CheckRecordEnd(ReadOnlySpan<int> springSizes) => springSizes.Length > 0 ? 0 : 1;

        long CheckLiveSpring(ReadOnlySpan<char> maintRecord, ReadOnlySpan<int> springSizes) {
            if (springSizes.Length == 0) return 0; // if no more springs, fail.
            int currentSpring = springSizes[0];

            // Check if enough space remains for the spring.
            var possibleSpace = maintRecord.IndexOfAnyExcept('?', '#');
            possibleSpace = possibleSpace == -1 ? maintRecord.Length : possibleSpace;
            if (currentSpring > possibleSpace) return 0;

            if (currentSpring == maintRecord.Length) return CheckRecordEnd(springSizes[1..]);

            // the above two checks ensure maintianceRecord is bigger than currentSpring.
            // if next space after the springSize is a live spring this combination is invalid.
            if (maintRecord[currentSpring] == '#') return 0;

            // all checks pass, this pattern is valid, consume spring and pattern and recurse.
            return CacheAndSolve(maintRecord[(currentSpring + 1)..], springSizes[1..]);
        }

        long CacheAndSolve(ReadOnlySpan<char> pattern, ReadOnlySpan<int> springSizes) {
            var key = (pattern.Length, springSizes.Length);
            if (cache.TryGetValue(key, out var result)) return result;

            cache[key] = Dispatch(pattern, springSizes);
            return cache[key];
        }
    }
}

public class SpringMaintanceRecordParser : SpracheParser {
    public static readonly Parser<IEnumerable<int>> SpringSizes =
        IntParser.DelimitedBy(Parse.Char(','));

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