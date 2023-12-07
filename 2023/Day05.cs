namespace AdventOfCode._2023;

using Sprache;

using AdventOfCode.Helpers;

public class Day05 : AdventBase
{
    Almanac? almanac;
    protected override void InternalOnLoad() {
        almanac = SeedMapParser.Almanac.Parse(Input.Text);
    }

    protected override object InternalPart1() => 
        almanac!.SeedData.Select(seed => almanac.Maps.Aggregate(seed, (value, map) => map.MapValue(value))).Min();

    protected override object InternalPart2() {
        List<Range<long>> resultRanges = [];
        foreach (var seedRange in almanac!.SeedRanges) {
            List<Range<long>> mappedRange = [seedRange];
            foreach (var map in almanac!.Maps) {
                mappedRange = mappedRange.Select(map.MapRange).SelectMany(e => e).Merge().ToList();
            }
            resultRanges.AddRange(mappedRange);
        }
        return resultRanges.Merge().First().Start;
    }
}

public class Almanac {
    private readonly ImmutableArray<long> _seedData;
    public IEnumerable<Range<long>> SeedRanges { get; }
    public IEnumerable<long> SeedData => _seedData; 
    public IEnumerable<SeedMap> Maps { get; }
    public Almanac(IEnumerable<long> seedData, IEnumerable<SeedMap> maps) {
        ArgumentNullException.ThrowIfNull(seedData);
        ArgumentNullException.ThrowIfNull(maps);
        if (!seedData.Count().IsEven()) throw new ArgumentOutOfRangeException(nameof(seedData), "Elements must be even.");

        this._seedData  = seedData.ToImmutableArray();
        this.SeedRanges = PairSeedData(_seedData);
        this.Maps       = maps.ToImmutableArray();
    }

    /// <summary>Pair up the seed data.</summary>
    private static IEnumerable<Range<long>> PairSeedData(ImmutableArray<long> seedData) {
        var seedRanges = new Range<long>[seedData.Length / 2];
        for (int seedIndex = 0; seedIndex < seedData.Length - 1; seedIndex += 2) {
            long start = seedData[seedIndex], end = seedData[seedIndex + 1] + start;
            seedRanges[seedIndex/2] = new Range<long>(start, end);
        }
        return seedRanges.ToImmutableArray();
    }
};

public class SeedMap(string fromName, string toName, IEnumerable<RangeMapping> rangeMaps)
{
    string FromName { get; } = fromName ?? throw new ArgumentNullException(nameof(fromName));
    string ToName   { get; } = toName   ?? throw new ArgumentNullException(nameof(toName));
    IReadOnlyList<RangeMapping> RangeMaps { get; } = rangeMaps?.OrderBy(map => map.SourceStart).ToImmutableArray()
            ?? throw new ArgumentNullException(nameof(rangeMaps));

    public long MapValue(long inValue) {
        foreach(var range in RangeMaps) {
            if (range.TryMapValue(inValue, out long outValue)) return outValue;
        }
        return inValue;
    }

    public IEnumerable<Range<long>> MapRange(Range<long> inRange) {
        Queue<Range<long>> ranges = [];
        ranges.Enqueue(inRange);
        var mapEnumerator = RangeMaps.GetEnumerator();

        // dual enumeration. Range items will reque until they have all been difinitively mapped (by the 2nd case).
        // map items will just iterate until gone. Loop ends when either list is exausted. 
        while (ranges.Count > 0 && mapEnumerator.MoveNext()) {
            var currentMap = mapEnumerator.Current;

            // split the front of the que with the current map
            (Range<long>? splitInside, Range<long>? splitOutside) = currentMap.Source.Split(ranges.Dequeue());

            // when split with another range, the split range will be either all inside, all outside, or split in two.
            switch ((splitInside, splitOutside)) {
                case (Range<long> inside, Range<long> outside): // range was split.
                    yield return currentMap.MapRange(inside); 
                    ranges.Enqueue(outside);                    // enque outside portion to check again.
                    break;
                case (Range<long> inside, null):                // range is entirely inside.
                    yield return currentMap.MapRange(inside);   // range section will exit loop here.
                    break;
                case (null, Range<long> outside):               // range is entirley outside.
                    ranges.Enqueue(outside);                    // enque to check again.
                    break;
                default:
                    throw new InvalidOperationException("Invalid range split returned.");
            }
        }
        // at most one item can remain in que when the maps are done. This range is outside and thus unmapped.
        if (ranges.TryDequeue(out Range<long> finalRange)) yield return finalRange;
    }
}

public record RangeMapping(long DestStart, long SourceStart, long RangeLength) { 
    public Range<long> Destination { get; } = new(DestStart,   DestStart   + RangeLength);
    public Range<long> Source      { get; } = new(SourceStart, SourceStart + RangeLength);

    public Range<long> MapRange(Range<long> range) {
        if (!Source.Contains(range)) throw new ArgumentException("Range must be entirely within Source.", nameof(range));

        return new Range<long>(MapValue(range.Start), MapValue(range.End));
    }

    public bool TryMapValue(long inValue, out long outValue) {
        if (!Source.Contains(inValue)) {
            outValue = default;
            return false;
        }

        outValue = MapValue(inValue);
        return true;
    }

    private long MapValue(long inValue)        => DestStart   - SourceStart + inValue;
};

/// <summary>A generic Range. Uses an [start, end) definition.</summary>
/// <param name="Start"></param>
/// <param name="End"></param>
public readonly record struct Range<T>(T Start, T End) : IComparable<Range<T>> where T : INumber<T> {
    public T Length { get => End - Start; }
    public bool Contains(T value) => value >= Start && value < End;
    public bool Contains(Range<T> other) => Start <= other.Start && other.End <= End;
    public bool Intersects(Range<T> other) => Start < other.End && End > other.Start;
    public int CompareTo(Range<T> other) => Start.CompareTo(other.Start);

    /// <summary>Splits <paramref name="other"/> by this range.</summary>
    /// <param name="other">The Range<T> to split.</param>
    /// <returns>A tuple with nullable components corresponding to the portions of <paramref name="other"/>
    /// inside and outside.</returns>
	public (Range<T>? inside, Range<T>? outside) Split(Range<T> other) {
	    if (this.Contains(other))    return (other, null); // entirely within,  return other as inside.
	    if (!this.Intersects(other)) return (null, other); // entirely without, return other as outside.

	    // Determine the portion inside the intersecting range
	    T insideStart = T.Max(other.Start, Start);
	    T insideEnd   = T.Min(other.End,   End);
	    Range<T> insideRange  = new Range<T>(insideStart,  insideEnd);

	    // Determine the portion outside the intersecting range
	    T outsideStart = other.Start < Start ? other.Start : End;
	    T outsideEnd   = other.End   > End   ? other.End   : Start;
	    Range<T> outsideRange = new Range<T>(outsideStart, outsideEnd);

	    return (insideRange, outsideRange);
	}

    public Range<T> Merge(Range<T> other) {
        if (!Intersects(other)) throw new ArgumentException("Ranges do not overlap.");

        return new Range<T>(T.Min(Start, other.Start), T.Max(End, other.End));
    }

    public bool TryMerge(Range<T> other, out Range<T> mergedRange) {
        if (!Intersects(other)) {
            mergedRange = default;
            return false;
        }

        mergedRange = new Range<T>(T.Min(Start, other.Start), T.Max(End, other.End));
        return true;
    }
}

public static class RangeExtensions {
    public static IEnumerable<Range<T>> Merge<T>(this IEnumerable<Range<T>> ranges) where T : INumber<T> {
        if (!ranges.Any()) yield break;
        var sortedRanges = ranges.Order().ToList();

        for (int index = 0; index < sortedRanges.Count - 1; index++) {
            Range<T> current = sortedRanges[index], next = sortedRanges[index + 1];

            // insert merged range into the next index so it can be possibly merged with the next item.
            if (current.TryMerge(next, out Range<T> merged)) sortedRanges[index + 1] = merged; 
            else yield return current;
        }

        yield return sortedRanges.Last();
    }
}

internal static class SeedMapParser
{
    static readonly Parser<long> NumberParser = Parse.Number.Select(long.Parse);

    public static readonly Parser<IEnumerable<long>> SeedIds =
        from identifier in Parse.String("seeds:").Token()
        from id in NumberParser.Token().XMany()
        select id;

    public static readonly Parser<RangeMapping> MapRange =
        from destRangeStart in NumberParser.Token()
        from srcRangeStart in NumberParser.Token()
        from rangeLength in NumberParser.Token()
        select new RangeMapping(destRangeStart, srcRangeStart, rangeLength);

    public static readonly Parser<SeedMap> Map =
        from fromName in Parse.Identifier(Parse.Letter, Parse.Letter)
        from to in Parse.String("-to-")
        from toName in Parse.Identifier(Parse.Letter, Parse.Letter).Token()
        from mapBreak in Parse.String("map:")
        from eol in Parse.LineEnd
        from mapRanges in MapRange.XMany()
        select new SeedMap(fromName, toName, mapRanges);

    public static readonly Parser<Almanac> Almanac =
        from seeds in SeedIds
        from maps in Map.XMany().End()
        select new Almanac(seeds, maps);
}