namespace AdventOfCode._2023;

public class Day07 : AdventBase
{
    protected override void InternalOnLoad() {

    }

    protected override object InternalPart1() {
        var stuff = CardParser.Parse(Input.Text).Select(t => (t.hand, Hand.GetScore(t.hand), t.bid))
                                                .OrderBy(t => t.Item2).Select((t, i) => (i + 1, t)).ToList();

        return stuff.Select(t => t.Item1 * t.t.bid).Sum();
    }

    protected override object InternalPart2() {
        // Temp.Solve(Input.Lines);

        var stuff = CardParser.Parse(Input.Text).Select(t => (t.hand, Hand.GetScoreWithJokers(t.hand), t.bid))
                                                .OrderBy(t => t.Item2).Select((t, i) => (i + 1, t)).ToList();

        return stuff.Select(t => t.Item1 * t.t.bid).Sum();
    }
}

public readonly record struct Hand
{
    public Hand(string hand) {
        Cards = hand.Select(c => new Card(c)).ToImmutableArray();
        Score = GetScore(hand);
    }
    public readonly IReadOnlyList<Card> Cards { get; }
    public readonly int Score { get; }

    public static int GetScore(IEnumerable<char> hand) {
        Span<int> seenCards = stackalloc int[Card.SYMBOL_RANKING.Length];
        int cardsScore = 0, handIndex = 0;

        foreach (var card in hand) {
            byte cardScore = Card.ScoreSymbol(card);

            // track how many times a card is seen. cardScore is a 0-13 value so can be used as an index.
            seenCards[cardScore]++;

            // push the cardScore into the score based on the index, first card score ends up first.
            cardsScore += cardScore << ((4 - handIndex) * 4);
            handIndex++;
        }

        // type functions as a pseudo-bitmask. 
        int type = 0;
        foreach (int count in seenCards) {
            type += (1 << count) - 1 - 1 * count;
        }
        // push the typeScore to be in front of the cardScore, so that hand valule is evaluated first.
        int typeScore = type << 20;

        return cardsScore + typeScore;
    }

    public static int GetScoreWithJokers(string hand) {
        int[] seenCards = new int[Card.SYMBOL_RANKING.Length];
        int cardsScore = 0, handIndex = 0;

        foreach (var card in hand) {
            byte cardScore = Card.ScoreSymbolWithJokers(card);

            // track how many times a card is seen. cardScore is a 0-13 value so can be used as an index.
            seenCards[cardScore]++;

            // push the cardScore into the score based on the index, first card score ends up first.
            cardsScore += cardScore << ((4 - handIndex) * 4);
            handIndex++;
        }

        // count the number of matches and highest card seen.
        int cardTypes = 0; int maxCount = 0;
        foreach(int count in seenCards.Skip(1)) {
            maxCount = Math.Max(count, maxCount);
            if (count != 0) cardTypes++;
        }
        int jokers = seenCards[0]; // jokers are the lowest value card, with first index. 

        // adjust the counts to take into acount jokers.
        maxCount += jokers;

        HandType type = (maxCount, cardTypes) switch {
            (5, _) => HandType.FiveOfAKind,
            (4, _) => HandType.FourOfAKind,
            (3, 2) => HandType.FullHouse,
            (3, _) => HandType.ThreeOfAKind,
            (2, 3) => HandType.TwoPair,
            (2, _) => HandType.OnePair,
            _      => HandType.HighCard,
        };

        // push the typeScore to be in front of the cardScore, so that hand valule is evaluated first.
        int typeScore = (int)type << 20;

        return cardsScore + typeScore;
    }

    enum HandType : byte { 
        FiveOfAKind  = 26,
        FourOfAKind  = 11,
        FullHouse    = 5,
        ThreeOfAKind = 4,
        TwoPair      = 2,
        OnePair      = 1,
        HighCard     = 0,
    }
}

public readonly record struct Card(char Symbol)
{
    public readonly short Score = (short)SYMBOL_RANKING.IndexOf(Symbol);

    public const string SYMBOL_RANKING = "23456789TJQKA";

    public static byte ScoreSymbol(char card) => card switch {
        'A' => 12,
        'K' => 11,
        'Q' => 10,
        'J' => 9,
        'T' => 8,
        <= '9' and >= '2' => (byte)(card - '2'),
        _ => throw new ArgumentException("Invalid card symbol.", nameof(card)),
    };

    public static byte ScoreSymbolWithJokers(char card) => card switch {
        'A' => 12,
        'K' => 11,
        'Q' => 10,
        'J' => 0,   // jacks/jokers score as 0.
        'T' => 9,   // 
        <= '9' and >= '2' => (byte)(card - '1'),
        _ => throw new ArgumentException("Invalid card symbol.", nameof(card)),
    };
}

internal static class CardParser {
    public static IEnumerable<(string hand, int bid)> Parse(string text) {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        int index = 0; char digit;
        do  {
            var hand = text[index..(index + 5)];
            index += 5;

            int bid = 0;
            while ((++index < text.Length) && char.IsAsciiDigit(digit = text[index])) {
                bid *= 10;
                bid += digit - '0';
            }
            
            yield return (hand.ToString(), bid);

            // Skip over EOL characters
            index = text.IndexOf('\n', index) + 1;
        } while (index != 0 && index < text.Length);
    }
}