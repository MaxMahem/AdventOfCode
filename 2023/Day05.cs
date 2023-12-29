﻿using AdventOfCode.Helpers;

namespace AdventOfCode._2023;

using Sprache;

using AdventOfCode.Helpers;

using RangeLong = Range<long>;

public class Day05 : AdventBase
{
    Almanac? almanac;
    protected override void InternalOnLoad() {
        almanac = SeedMapParser.Almanac.Parse(Input.Text);
    }

    protected override object InternalPart1() => 
        almanac!.SeedData.Select(seed => almanac.Maps.Aggregate(seed, (value, map) => map.MapValue(value))).Min();

    protected override object InternalPart2() {
        List<RangeLong> resultRanges = [];
        foreach (var seedRange in almanac!.SeedRanges) {
            List<RangeLong> mappedRange = [seedRange];
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
    public IEnumerable<RangeLong> SeedRanges { get; }
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
    private static IEnumerable<RangeLong> PairSeedData(ImmutableArray<long> seedData) {
        var seedRanges = new RangeLong[seedData.Length / 2];
        for (int seedIndex = 0; seedIndex < seedData.Length - 1; seedIndex += 2) {
            long start = seedData[seedIndex];
            long end   = seedData[seedIndex + 1] + start;
            seedRanges[seedIndex/2] = new RangeLong(start, end);
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

    public IEnumerable<RangeLong> MapRange(RangeLong inRange) {
        Queue<RangeLong> ranges = [inRange];
        var mapEnumerator = RangeMaps.GetEnumerator();

        // dual enumeration. Range items will reque until they have all been mapped
        // maps will iterate until finished. Loop ends when either set is exausted. 
        while (ranges.Count > 0 && mapEnumerator.MoveNext()) {
            var currentMap = mapEnumerator.Current;

            // split the range at the front of the que with the current map
            (RangeLong? splitLeft, RangeLong? splitInside, RangeLong? splitRight) = ranges.Dequeue().Split(currentMap.Source);
            if (splitLeft   is RangeLong left)  ranges.Enqueue(left);                      // outside portions get enqued to get checked again.
            if (splitRight  is RangeLong right) ranges.Enqueue(right);                    
            if (splitInside is RangeLong inside) yield return currentMap.MapRange(inside); // inside portions are mapped and returned.
        }
        // empty and map any remaining items in que (should only be at most one.)
        while (ranges.TryDequeue(out RangeLong finalRange)) yield return finalRange;
    }
}

public record RangeMapping(long DestStart, long SourceStart, long RangeLength) { 
    public RangeLong Destination { get; } = new(DestStart,   DestStart   + RangeLength);
    public RangeLong Source      { get; } = new(SourceStart, SourceStart + RangeLength);

    public RangeLong MapRange(RangeLong range) {
        if (!Source.Contains(range)) throw new ArgumentException("Must be entirely within Source.", nameof(range));

        return new RangeLong(MapValue(range.Start), MapValue(range.End));
    }

    public bool TryMapValue(long inValue, out long outValue) {
        if (!Source.Contains(inValue)) {
            outValue = default;
            return false;
        }

        outValue = MapValue(inValue);
        return true;
    }

    private long MapValue(long inValue) => DestStart - SourceStart + inValue;
};

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

public class SeedMapParser : SpracheParser {
    public static readonly Parser<IEnumerable<long>> SeedIds =
        from identifier in Parse.String("seeds:").Token()
        from id         in LongParser.Token().XMany()
        select id;

    public static readonly Parser<RangeMapping> MapRange =
        from destRangeStart in LongParser.Token()
        from srcRangeStart  in LongParser.Token()
        from rangeLength    in LongParser.Token()
        select new RangeMapping(destRangeStart, srcRangeStart, rangeLength);

    public static readonly Parser<SeedMap> Map =
        from fromName  in Parse.Identifier(Parse.Letter, Parse.Letter)
        from to        in Parse.String("-to-")
        from toName    in Parse.Identifier(Parse.Letter, Parse.Letter).Token()
        from mapBreak  in Parse.String("map:")
        from eol       in Parse.LineEnd
        from mapRanges in MapRange.XMany()
        select new SeedMap(fromName, toName, mapRanges);

    public static readonly Parser<Almanac> Almanac =
        from seeds in SeedIds
        from maps  in Map.XMany().End()
        select new Almanac(seeds, maps);
}