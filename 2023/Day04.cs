namespace AdventOfCode._2023;

using Sprache;

public class Day04 : AdventBase {
    ImmutableList<TicketPair>? _tickets;

    protected override void InternalOnLoad() {
        _tickets = ScratchTicketsParser.TicketList.Parse(Input.Text).ToImmutableList();
    }

    protected override object InternalPart1() =>
        _tickets!.Select(pair => TicketPair.NaiveCardScores[pair.MatchingCount]).Sum();

    protected override object InternalPart2() {
        Span<int> ticketCounter = stackalloc int[_tickets!.Count];
        ticketCounter.Fill(1); // ticket counter starts at one, since there is one of every ticket to start.

        int sum = 0;
        for (int index = 0; index < ticketCounter.Length; index++) {
            // itterate through the extra tickets that are awarded for winning
            // starts at one because the extra ticket will be beyond the current ticket.
            for (int extraTickets = 1; extraTickets <= _tickets![index].MatchingCount && extraTickets + index < ticketCounter.Length; extraTickets++) {
                // increment the corresponding counter by the number of tickets of the current type.
                ticketCounter[index + extraTickets] += ticketCounter[index];
            }
            sum += ticketCounter[index];
        }

        return sum;
    }
}

public class TicketPair(int id, IEnumerable<int> scratchNumbers, IEnumerable<int> winningNumbers) {
    public int Id { get; } = id;
    public IEnumerable<int> ScratchNumbers { get; } = scratchNumbers ?? throw new ArgumentNullException(nameof(scratchNumbers));
    public IEnumerable<int> WinningNumbers { get; } = winningNumbers ?? throw new ArgumentNullException(nameof(winningNumbers));
    public int MatchingCount { get; } = scratchNumbers.Intersect(winningNumbers).Count();

    /// <summary>A doubling series, 0, 1, 2, 4, 8, 16...</summary>
    public static readonly ImmutableArray<int> NaiveCardScores 
        = Enumerable.Range(-1, WINNING_NUMBER_COUNT).Select(index => index == -1 ? 0 : 1 << index).ToImmutableArray();
    
    public const int WINNING_NUMBER_COUNT = 25;
}

internal static class ScratchTicketsParser {
    static readonly Parser<int> NumberParser = Parse.Number.Select(int.Parse);
    static readonly Parser<char> IdSeperator = Parse.Char(':');
    static readonly Parser<char> CardSeperator = Parse.Char('|');

    public static readonly Parser<int> CardId =
        from identifier in Parse.String("Card").Token()
        from id in NumberParser
        select id;

    public static readonly Parser<TicketPair> TicketPair =
        from id in CardId
        from colon in IdSeperator
        from ticketNumbers in NumberParser.Token().Many()
        from seperator in CardSeperator
        from winningNumbers in NumberParser.Token().Many()
        from eol in Parse.LineEnd.Optional()
        select new TicketPair(id, ticketNumbers, winningNumbers);

    public static readonly Parser<IEnumerable<TicketPair>> TicketList =
        from pairs in TicketPair.XMany().End()
        select pairs;
}