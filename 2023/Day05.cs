namespace AdventOfCode._2023;

using Sprache;

using AdventOfCode.Helpers;

using Range = Helpers.Range<long>;

public class Day05 : AdventBase
{
    Almanac? almanac;
    protected override void InternalOnLoad() {
        this.almanac = AlmanacParser.Almanac.Parse(Input.Text);
    }

    protected override object InternalPart1() => 
        this.almanac!.SeedData.Select(seed => almanac.Maps.Aggregate(seed, (value, map) => map.MapValue(value))).Min();

    protected override object InternalPart2() {
        List<Range> resultRanges = [];
        foreach (var seedRange in this.almanac!.SeedRanges) {
            Range[] mappedRange = [seedRange];
            foreach (var map in this.almanac!.Maps) {
                mappedRange = mappedRange.Select(range => map.MapRange(in range)).SelectMany(e => e).Merge().ToArray();
            }
            resultRanges.AddRange(mappedRange);
        }
        return resultRanges.Merge().First().Start;
    }
}

public class Almanac {
    private readonly ImmutableArray<long> _seedData;
    public IEnumerable<Range> SeedRanges { get; }
    public IEnumerable<long> SeedData => _seedData; 
    public IEnumerable<AlmanacValueMap> Maps { get; }
    public Almanac(IEnumerable<long> seedData, IEnumerable<AlmanacValueMap> maps) {
        ArgumentNullException.ThrowIfNull(seedData);
        ArgumentNullException.ThrowIfNull(maps);
        if (!seedData.Count().IsEven()) throw new ArgumentOutOfRangeException(nameof(seedData), "Elements must be even.");

        this._seedData  = seedData.ToImmutableArray();
        this.SeedRanges = PairSeedData(_seedData);
        this.Maps       = maps.ToImmutableArray();
    }

    /// <summary>Pair up the seed data.</summary>
    private static IEnumerable<Range> PairSeedData(ImmutableArray<long> seedData) {
        var seedRanges = new Range[seedData.Length / 2];
        for (int seedIndex = 0; seedIndex < seedData.Length - 1; seedIndex += 2) {
            long start = seedData[seedIndex];
            long end   = seedData[seedIndex + 1] + start;
            seedRanges[seedIndex/2] = new Range(start, end);
        }
        return seedRanges.ToImmutableArray();
    }
};

public class AlmanacValueMap(string fromName, string toName, IEnumerable<ValueMap> rangeMaps) {
    public const int MAX_RANGE_COUNT = 10;

    string FromName { get; } = fromName ?? throw new ArgumentNullException(nameof(fromName));
    string ToName   { get; } = toName   ?? throw new ArgumentNullException(nameof(toName));
    IReadOnlyList<ValueMap> RangeMaps { get; } = rangeMaps?.OrderBy(map => map.Source).ToImmutableArray()
            ?? throw new ArgumentNullException(nameof(rangeMaps));

    public long MapValue(long value) {
        foreach (var range in RangeMaps) {
            if (range.TryMapValue(value, out long mappedValue)) return mappedValue;
        }
        return value;
    }

    /// <summary>Maps a given range against these range maps this class contains. 
    /// Any portions that do not fall within the range are passed through unmapped.
    /// Splits and creates new ranges from the input as necessary to do so.</summary>
    /// <param name="inRange"></param>
    /// <returns>A list of mapped ranges </returns>
    public IEnumerable<Range> MapRange(in Range inRange) {
        StackSpan<Range> inputRanges  = stackalloc Range[MAX_RANGE_COUNT];
        StackSpan<Range> mappedRanges = stackalloc Range[MAX_RANGE_COUNT * 2];
        inputRanges.Push(inRange);
        var mapEnumerator = RangeMaps.GetEnumerator();

        // Dual enumeration. Range items will restack until they have all been mapped
        // maps will iterate until exhausted. Loop ends when either set is exausted. 
        // Test order is important here as TryPop has side effects (popping the item)
        while (mapEnumerator.MoveNext() && inputRanges.TryPop(out Range inputRange)) {
            var currentMap = mapEnumerator.Current;

            var mappedSplit = currentMap.MapSplit(inputRange);
            if (mappedSplit.UnmappedL is Range left)   inputRanges.Push(left);      // unmapped portions go back on the stack
            if (mappedSplit.UnmappedR is Range right)  inputRanges.Push(right);                    
            if (mappedSplit.Mapped    is Range inside) mappedRanges.Push(inside);
        }
        // empty any remaining unmapped items to mapped set (should only be at most only two.)
        while (inputRanges.TryPop(out Range finalRange)) mappedRanges.Push(finalRange);
        return mappedRanges.ToArray();
    }
}

public class ValueMap(long destStart, long sourceStart, long rangeLength) { 
    public Range Destination { get; } = new(destStart,   destStart   + rangeLength);
    public Range Source      { get; } = new(sourceStart, sourceStart + rangeLength);

    public Range MapRange(in Range range) {
        if (!Source.Contains(range)) throw new ArgumentException("Must be entirely within Source.", nameof(range));

        return new Range(MapValue(range.Start), MapValue(range.End));
    }

    /// <summary>Maps a given range, splitting it if necessary to do so.</summary>
    /// <returns>A tuple containing the possible Mapped, and two possible Unmapped portions of the range.</returns>
    public (Range? Mapped, Range? UnmappedL, Range? UnmappedR) MapSplit(in Range range) {
        var split = range.Split(Source);
        return (split.Inside is Range inside ? MapRange(inside) : null, split.Left, split.Right);
    }

    public bool TryMapValue(long inValue, out long outValue) {
        if (!Source.Contains(inValue)) {
            outValue = default;
            return false;
        }

        outValue = MapValue(inValue);
        return true;
    }

    private long MapValue(long inValue) => Destination.Start - Source.Start + inValue;
};

public class AlmanacParser : SpracheParser {
    public static readonly Parser<IEnumerable<long>> SeedIds =
        from identifier in Parse.String("seeds:").Token()
        from id         in LongParser.Token().XMany()
        select id;

    public static readonly Parser<ValueMap> MapRange =
        from destRangeStart in LongParser.Token()
        from srcRangeStart  in LongParser.Token()
        from rangeLength    in LongParser.Token()
        select new ValueMap(destRangeStart, srcRangeStart, rangeLength);

    public static readonly Parser<AlmanacValueMap> Map =
        from fromName  in Parse.Identifier(Parse.Letter, Parse.Letter)
        from to        in Parse.String("-to-")
        from toName    in Parse.Identifier(Parse.Letter, Parse.Letter).Token()
        from mapBreak  in Parse.String("map:")
        from eol       in Parse.LineEnd
        from mapRanges in MapRange.XMany()
        select new AlmanacValueMap(fromName, toName, mapRanges);

    public static readonly Parser<Almanac> Almanac =
        from seeds in SeedIds
        from maps  in Map.XMany().End()
        select new Almanac(seeds, maps);
}