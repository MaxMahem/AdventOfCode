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
        this.almanac!.SeedRanges.SelectMany(seedRange => IEnumerableHelpers.Flatten(seedRange.Start, seedRange.Length))
                     .Select(seed => almanac.Maps.Aggregate(seed, (value, map) => map.MapValue(value))).Min();

    protected override object InternalPart2() => this.almanac!.MappedSeedRanges.First().Start;
}

public class Almanac {
    public IEnumerable<Range> SeedRanges { get; }
    public IEnumerable<AlmanacValueMap> Maps { get; }
    public IEnumerable<Range> MappedSeedRanges { get; }
    public Almanac(IEnumerable<long> seedData, IEnumerable<AlmanacValueMap> maps) {
        ArgumentNullException.ThrowIfNull(seedData);
        ArgumentNullException.ThrowIfNull(maps);

        this.SeedRanges       = seedData.SelectPair((start, length) => new Range(start, start + length)).ToImmutableArray();
        this.Maps             = maps.ToImmutableArray();
        this.MappedSeedRanges = MapSeedRanges().ToImmutableArray();
    }

    public const int MAX_RESULT_RANGES = 100;

    private IEnumerable<Range> MapSeedRanges() {
        StackSpan<Range> resultRanges = stackalloc Range[MAX_RESULT_RANGES];
        foreach (var seedRange in SeedRanges) {
            Range[] mappedRange = [seedRange];
            foreach (var map in Maps) {
                mappedRange = mappedRange.Select(range => map.MapRange(in range)).SelectMany(e => e).Merge().ToArray();
            }
            resultRanges.PushRange(mappedRange);
        }
        return resultRanges.ToArray().Merge();
    }
};

public class AlmanacValueMap(string fromName, string toName, IEnumerable<ValueMap> rangeMaps) {
    public const int MAX_RANGE_COUNT = 10;

    string FromName { get; } = fromName ?? throw new ArgumentNullException(nameof(fromName));
    string ToName   { get; } = toName   ?? throw new ArgumentNullException(nameof(toName));
    IReadOnlyList<ValueMap> RangeMaps { get; } = rangeMaps?.OrderBy(map => map.Source).ToImmutableArray()
            ?? throw new ArgumentNullException(nameof(rangeMaps));

    public long MapValue(long value) => 
        RangeMaps.Select(range => (Try: range.TryMapValue(value, out long outValue), Out: outValue))
                 .Where(t => t.Try).Select(t => t.Out).FirstOrDefault(value);

    /// <summary>Maps a given range against the range maps this class contains. 
    /// Any portions that do not fall within the range are passed through unmapped.
    /// Splits and creates new ranges from the input as necessary to do so.</summary>
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
        // push any remaining unmapped input into the mapped set unmapped.
        mappedRanges.PushStack(inputRanges);
        return mappedRanges.ToArray();
    }
}

public class ValueMap(long destStart, long sourceStart, long rangeLength) { 
    public Range Destination { get; } = new(destStart,   destStart   + rangeLength);
    public Range Source      { get; } = new(sourceStart, sourceStart + rangeLength);

    private long mappedValueOffset => Destination.Start - Source.Start;

    public Range MapRange(in Range range) {
        if (!Source.Contains(range)) throw new ArgumentException("Must be entirely within Source.", nameof(range));

        return range + this.mappedValueOffset;
    }

    /// <summary>Maps a given range, splitting it if necessary to do so.</summary>
    /// <returns>A tuple containing the possible Mapped, and two possible Unmapped portions of the range.</returns>
    public (Range? Mapped, Range? UnmappedL, Range? UnmappedR) MapSplit(in Range range) {
        var split = range.Split(Source);
        return (split.Inside is Range inside ? MapRange(inside) : null, split.Left, split.Right);
    }

    public long MapValue(long inValue) => Source.Contains(inValue) ? this.mappedValueOffset + inValue
                                                                   : throw new ArgumentException("Value not in range", nameof(inValue));

    public bool TryMapValue(long inValue, out long outValue) {
        if (!Source.Contains(inValue)) {
            outValue = default;
            return false;
        }

        outValue = MapValue(inValue);
        return true;
    }
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