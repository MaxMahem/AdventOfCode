namespace AdventOfCode._2023;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using AdventOfCode.Helpers;
using AdventOfCodeSupport;
using Sprache;

public class Day05 : AdventBase
{
    Almanac? almanac;
    protected override void InternalOnLoad() {
        almanac = SeedMapParser.Almanac.Parse(Input.Text);
    }

    protected override object InternalPart1() => 
        almanac!.SeedData.Select(seed => almanac.Maps.Aggregate(seed, (value, map) => map.MapValue(value))).Min();

    protected override object InternalPart2() {
        List<LongRange> resultRanges = [];
        foreach (var seedRange in almanac!.SeedRanges) {
            List<LongRange> workingSeedRanges = [seedRange];
            foreach (var map in almanac!.Maps) {
                List<LongRange> newRange = [];
                foreach (var range in workingSeedRanges) {
                    newRange.AddRange(map.MapRange(range));
                }
                workingSeedRanges = newRange.Order().ToList();
                long totalLength = workingSeedRanges.Select(range => range.Length).Sum();
            }
            resultRanges.AddRange(workingSeedRanges);
        }
        return resultRanges.Order().First().Start;
    }
}

public class Almanac {
    private readonly ImmutableArray<long> _seedData;
    public IEnumerable<LongRange> SeedRanges { get; }
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
    private static IEnumerable<LongRange> PairSeedData(ImmutableArray<long> seedData) {
        var seedRanges = new LongRange[seedData.Length / 2];
        for (int seedIndex = 0; seedIndex < seedData.Length - 1; seedIndex += 2) {
            long start = seedData[seedIndex], end = seedData[seedIndex + 1] + start;
            seedRanges[seedIndex/2] = new LongRange(start, end);
        }
        return seedRanges.ToImmutableArray();
    }
};

public class SeedMap(string fromName, string toName, IEnumerable<RangeMapping> rangeMaps)
{
    string FromName { get; } = fromName ?? throw new ArgumentNullException(nameof(fromName));
    string ToName   { get; } = toName   ?? throw new ArgumentNullException(nameof(toName));
    IEnumerable<RangeMapping> RangeMaps { get; } = rangeMaps?.OrderBy(map => map.SourceStart).ToImmutableArray()
            ?? throw new ArgumentNullException(nameof(rangeMaps));

    public long MapValue(long inValue) {
        foreach(var range in RangeMaps) {
            if (range.TryMapValue(inValue, out long outValue)) return outValue;
        }
        return inValue;
    }

    public IEnumerable<LongRange> MapRange(LongRange range) {
        Queue<LongRange> rangeQueue = [];
        rangeQueue.Enqueue(range);
        var mapEnumerator = RangeMaps.GetEnumerator();

        while (rangeQueue.Count > 0 && mapEnumerator.MoveNext()) {
            var currentMap = mapEnumerator.Current;
            (LongRange? nullableMappedRange, LongRange? nullableUnmappedRange) = currentMap.Source.Split(rangeQueue.Dequeue());
            switch ((nullableMappedRange, nullableUnmappedRange)) {
                case (LongRange mappedRange, LongRange unmappedRange): // range was split.
                    yield return currentMap.MapRange(mappedRange);
                    rangeQueue.Enqueue(unmappedRange);
                    break;
                case (LongRange mappedRange, null):     // range was mapped.
                    yield return currentMap.MapRange(mappedRange);
                    break;
                case (null, LongRange unmappedRange):   // range was unmapped.
                    rangeQueue.Enqueue(unmappedRange);
                    break;
                default:
                    throw new InvalidOperationException("Invalid range split returned.");
            }
        }
        if (rangeQueue.TryDequeue(out LongRange finalUnmappedRange)) yield return finalUnmappedRange;
    }
}

public record RangeMapping(long DestStart, long SourceStart, long RangeLength) { 
    public LongRange Destination { get; } = new(DestStart, DestStart + RangeLength);
    public LongRange Source { get; } = new(SourceStart, SourceStart + RangeLength);

    public LongRange MapRange(LongRange range) {
        if (!Source.Contains(range)) throw new ArgumentException("Range must be entirely within Source.", nameof(range));

        return new LongRange(MapValue(range.Start), MapValue(range.End));
    }

    public bool TryMapValue(long inValue, out long outValue) {
        outValue = inValue;
        if (!Source.Contains(inValue)) return false;

        outValue = MapValue(inValue);
        return true;
    }

    private long MapValue(long inValue)        => DestStart   - SourceStart + inValue;
};

/// <summary>A <see cref="long"/> based Range. Uses an [start, end) definition.</summary>
/// <param name="Start"></param>
/// <param name="End"></param>
public readonly record struct LongRange(long Start, long End) : IComparable<LongRange> {
    public long Length { get => End - Start; }
    public bool Contains(long value) => value >= Start && value < End;
    public bool Contains(LongRange other) => Start <= other.Start && other.End <= End;
    public bool Intersects(LongRange other) => Start < other.End && End > other.Start;
    public int CompareTo(LongRange other) => Start.CompareTo(other.Start);

    /// <summary>Splits <paramref name="other"/> by this range.</summary>
    /// <param name="other">The LongRange to split.</param>
    /// <returns>A tuple with nullable components corresponding to the portions of <paramref name="other"/>
    /// inside and outside.</returns>
	public (LongRange? inside, LongRange? outside) Split(LongRange other) {
	    if (this.Contains(other))    return (other, null); // entirely within, return other as inside.
	    if (!this.Intersects(other)) return (null, other); // entirely without, return other as outside.

	    // Determine the portion inside the intersecting range
	    long insideStart = Math.Max(other.Start, Start);
	    long insideEnd   = Math.Min(other.End,   End);
	    LongRange insideRange = new LongRange(insideStart, insideEnd);

	    // Determine the portion outside the intersecting range
	    long outsideStart = other.Start < Start ? other.Start : End;
	    long outsideEnd   = other.End   > End   ? other.End   : Start;
	    LongRange outsideRange = new LongRange(outsideStart, outsideEnd);

	    return (insideRange, outsideRange);
	}

    public LongRange Merge(LongRange other) {
        if (!Intersects(other)) throw new ArgumentException("Ranges do not overlap.");

        // new range will have the minimum possible start range and max possible end range.
        return new LongRange(Math.Min(Start, other.Start), Math.Max(End, other.End));
    }

    public bool TryMerge(LongRange other, out LongRange mergedRange) {
        if (!Intersects(other)) {  // Ranges do not overlap, cannot merge
            mergedRange = default;
            return false;
        }

        // new range will have the minimum possible start range and max possible end range.
        mergedRange = new LongRange(Math.Min(Start, other.Start), Math.Max(End, other.End));
        return true;
    }
}

public static class LongRangeExtensions
{
    public static IEnumerable<LongRange> Merge(this IEnumerable<LongRange> ranges) {
        var sortedRanges = ranges.Order().ToList();

        for (int index = 0; index < sortedRanges.Count - 1; index++) {
            LongRange current = sortedRanges[index], next = sortedRanges[index + 1];

            // insert merged range into the next index so it can be possibly merged with the next item.
            if (current.TryMerge(next, out LongRange merged)) sortedRanges[index + 1] = merged; 
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