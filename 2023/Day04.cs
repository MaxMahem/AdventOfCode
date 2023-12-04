namespace AdventOfCode._2023;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Sprache;

using AdventOfCodeSupport;

using AdventOfCode.Helpers;

public class Day04 : AdventBase {
    ImmutableList<TicketPair>? _tickets;

    protected override void InternalOnLoad() {
        _tickets = ScratchTicketsParser.TicketList.Parse(Input.Text).ToImmutableList();
    }

    protected override object InternalPart1() =>
        _tickets!.Select(pair => TicketPair.NaiveCardScores[pair.MatchingCount]).Sum();

    protected override object InternalPart2() {
        var ticketCounters = _tickets!.Select(ticket => new TicketCounter(1, ticket)).ToList();
        for (int index = 0; index < ticketCounters.Count; index++) {
            for (var extraTickets = ticketCounters[index].TicketPair.MatchingCount; extraTickets > 0; extraTickets--) {
                ticketCounters[index + extraTickets].Count += ticketCounters[index].Count;
            }
        }
        return ticketCounters.Select(counter => counter.Count).Sum();
    }

    private record TicketCounter(int Count, TicketPair TicketPair) {
        public int Count { get; set; } = Count;
    }
}

public class TicketPair(int id, IEnumerable<int> scratchNumbers, IEnumerable<int> winningNumbers) {
    public int Id { get; } = id;
    public IEnumerable<int> ScratchNumbers { get; } = scratchNumbers ?? throw new ArgumentNullException(nameof(scratchNumbers));
    public IEnumerable<int> WinningNumbers { get; } = winningNumbers ?? throw new ArgumentNullException(nameof(winningNumbers));
    public int MatchingCount { get; } = scratchNumbers.Intersect(winningNumbers).Count();

    /// <summary>A doubling series, 0, 1, 2, 4, 8, 16...</summary>
    public static readonly ImmutableArray<int> NaiveCardScores 
        = Enumerable.Range(-1, MAX_WINNING_NUMBERS).Select(index => index == -1 ? 0 : 1 << index).ToImmutableArray();
    
    public const int MAX_WINNING_NUMBERS = 25;
}

internal static class ScratchTicketsParser
{
    static readonly Parser<char> CardSeparator = Parse.Char('|');
    static readonly Parser<char> IdSeperator = Parse.Char(':');
    static readonly Parser<int> NumberParser = Parse.Number.Select(int.Parse);

    public static readonly Parser<int> CardId =
        from identifier in Parse.String("Card").Token()
        from id in NumberParser
        select id;

    public static readonly Parser<IEnumerable<int>> Numbers =
        from numbers in NumberParser.Token().Many()
        select numbers;

    public static readonly Parser<TicketPair> TicketPair =
        from id in CardId
        from colon in IdSeperator.Token()
        from ticketNumbers in NumberParser.Token().Many()
        from seperator in CardSeparator.Token()
        from winningNumbers in NumberParser.Token().Many()
        from eol in Parse.LineEnd.Optional()
        select new TicketPair(id, ticketNumbers, winningNumbers);

    public static readonly Parser<IEnumerable<TicketPair>> TicketList =
        from pairs in TicketPair.XMany().End()
        select pairs;
}