namespace AdventOfCode._2023;

using System;
using System.Collections.Generic;

using CommunityToolkit.HighPerformance;
using MoreLinq;

public class Day09 : AdventBase
{
    IEnumerable<Sequence<long>>? _oasisSequences;

    protected override void InternalOnLoad() {
        _oasisSequences = OasisDataParser<long>.Parse(Input.Text);
    }

    protected override object InternalPart1() =>
        _oasisSequences!.Select(seq => seq.Differences.Reverse().Skip(1).Aggregate(0L, (num, seq) => seq.Last() + num)).Sum();

    protected override object InternalPart2() =>
        _oasisSequences!.Select(seq => seq.Differences.Reverse().Skip(1).Aggregate(0L, (num, seq) => seq.First() - num)).Sum();
}

public class Sequence<TNum>(IEnumerable<TNum> sequence)
    where TNum : INumber<TNum>, IAdditionOperators<TNum, TNum, TNum> 
{
    public IEnumerable<TNum> PrimeSequence { get; } = sequence ?? throw new ArgumentNullException(nameof(sequence));
    public IEnumerable<IEnumerable<TNum>> Differences { get; } = BuildDifferences(sequence);

    public static IEnumerable<IEnumerable<TNum>> BuildDifferences(IEnumerable<TNum> sequence) {
        List<IEnumerable<TNum>> differances = [sequence.ToList()];
        do {
            differances.Add(differances[^1].Pairwise((left, right) => right - left).ToImmutableArray());
        } while(!differances[^1].All(n => n == TNum.Zero));
        return differances.ToImmutableArray();
    }
}

internal static class OasisDataParser<TNum> 
    where TNum : INumber<TNum>, IAdditionOperators<TNum, TNum, TNum>, ISpanParsable<TNum>
{
    public static IEnumerable<Sequence<TNum>> Parse(string text) {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        List<Sequence<TNum>> sequences = [];

        foreach(var line in text.AsSpan().EnumerateLines()) {
            if (line.IsEmpty) continue;
            List<TNum> sequence = [];
            foreach (var numberSpan in line.Tokenize(' ')) {
                sequence.Add(TNum.Parse(numberSpan, null));
            }
            sequences.Add(new Sequence<TNum>(sequence.ToImmutableArray()));
        }        
        return sequences.ToImmutableArray();
    }
}