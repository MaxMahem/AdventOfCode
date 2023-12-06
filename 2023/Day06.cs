namespace AdventOfCode._2023;

using AdventOfCode.Helpers;
using AdventOfCodeSupport;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

public class Day06 : AdventBase
{
    IEnumerable<RaceData>? _raceDataPart1;
    RaceData _raceDataPart2;
    protected override void InternalOnLoad() {
        _raceDataPart1 = RaceDataParser.RaceData.Parse(Input.Text);
        var newTime = _raceDataPart1.Select(rd => rd.Time).Reverse().Concatinate();
        var newDist = _raceDataPart1.Select(rd => rd.Distance).Reverse().Concatinate();
        _raceDataPart2 = new RaceData(newTime, newDist);
    }

    protected override object InternalPart1() => 
        _raceDataPart1!.Select(race => { var (endHold, startHold) = race.MinimumWinningHolds; return endHold - startHold + 1; }).Product();

    protected override object InternalPart2() {
        var (endHold, startHold) = _raceDataPart2.MinimumWinningHolds;
        return endHold - startHold + 1;
    }
}

public readonly record struct RaceData(long Time, long Distance) {
    public IEnumerable<(long hold, long distance)> GetWinningCombinations() {
        var (endHold, startHold) = this.MinimumWinningHolds;
        for (; startHold < endHold; startHold++) {
            long distance = startHold * (Time - startHold);
            if (distance > Distance) yield return (startHold, distance);
        }
    }

    public (long endHold, long startHold) MinimumWinningHolds { get; } = CalculateMinimumHold(Time, Distance + 1);
        
    public static (long endHold, long startHold) CalculateMinimumHold(long time, long distance)
    {
        long hold1 = (long)Math.Floor(  (time + Math.Sqrt(time * time - 4 * distance)) / 2);
        long hold2 = (long)Math.Ceiling((time - Math.Sqrt(time * time - 4 * distance)) / 2);
	    return (hold1, hold2);
    }
}

internal static class RaceDataParser
{
    static readonly Parser<int> NumberParser = Parse.Number.Select(int.Parse);

    public static readonly Parser<IEnumerable<int>> Times =
        from identifier in Parse.String("Time:").Token()
        from times in NumberParser.Token().XMany()
        select times;
    
    public static readonly Parser<IEnumerable<int>> Distances =
        from identifier in Parse.String("Distance:").Token()
        from distance in NumberParser.Token().XMany()
        from eol in Parse.LineEnd.Optional()
        select distance;

    public static readonly Parser<IEnumerable<RaceData>> RaceData =
        from times in Times
        from distances in Distances
        select times.Zip(distances).Select((data) => new RaceData(data.First, data.Second));
}