namespace AdventOfCode._2023;

public class Day07 : AdventBase
{
    IEnumerable<HandBid>? _handBids;
    
    protected override void InternalOnLoad() {
        _handBids = CardParser.Parse(Input.Text).ToList();
    }

    protected override object InternalPart1() {
        var game = new CamelPokerGame();

        return _handBids!.Select(hb => (Score: game.ScoreHand(hb.Hand), hb.Bid))
                         .OrderBy(t => t.Score).Select((t, i) => (i + 1) * t.Bid).Sum();
    }

    protected override object InternalPart2() {
        var game = new CamelPokerGame('J');

        return _handBids!.Select(hb => (Score: game.ScoreHand(hb.Hand), hb.Bid))
                         .OrderBy(t => t.Score).Select((t, i) => (i + 1) * t.Bid).Sum();
    }
}

public readonly record struct HandBid(string Hand, int Bid);

public class CamelPokerGame(char? wildcard = null) {
    public char? Wildcard { get; } = wildcard;

    private readonly Func<char, byte> _cardScorer = wildcard is char wc ? card => Card.GetScore(card, (char)wildcard)
                                                                        : Card.GetScore;

    private readonly HandTyper _handTyper = wildcard is not null ? TypeHandWildcard : TypeHand;

    public Hand CreateHand(string hand) => new(hand, ScoreHand(hand));

    public int ScoreHand(string hand) {
        Span<byte> seenCards = stackalloc byte[Card.VALID_SYMBOLS.Length + 1];
        int cardsScore = 0, handIndex = 0;

        // score all the cards in the hand, and track how many have been seen.
        foreach (char card in hand) {
            byte cardScore = _cardScorer(card);

            // track how many times a card is seen. cardScore is a 0-13 value so can be used as an index.
            seenCards[cardScore]++;

            // push the cardScore into the score based on the index, first card score ends up first.
            cardsScore += cardScore << ((4 - handIndex) * 4);
            handIndex++;
        }

        // push the typeScore to be in front of the cardScore, so that hand valule is evaluated first.
        Hand.HandType type = _handTyper(seenCards);
        int typeScore = (int)type << 20;

        return cardsScore + typeScore;
    }

    // determines the hand type given a count of the number of seen cards, for hands without wildcards.
    private static Hand.HandType TypeHand(Span<byte> seenCards) {
        // turns the count of cards seen into a pseudo-bitmask.
        int handType = 0;
        foreach (int count in seenCards) {
            handType += (1 << count) - 1 - count;
        }
        return (Hand.HandType)handType;
    }

    // determines the hand type given a count of the number of seen cards, for games with wildcards.
    private static Hand.HandType TypeHandWildcard(Span<byte> seenCards) {
        int wildcards = seenCards[0];                   // wildcards are the lowest value card, at the first index. 
        if (wildcards == 0) return TypeHand(seenCards); // if no wildcard use the default (faster) typing method.

        // check the seen cards to find the number of types cards seen and the max matching
        int cardTypes = 0; int maxCount = 0;
        foreach (var count in seenCards[1..]) {
            if (count != 0) cardTypes++;
            maxCount = Math.Max(maxCount, count);
        }

        // wildcards match with any card, so increase the max matches to take into acount.
        maxCount += seenCards[0];
        return (maxCount, cardTypes) switch {   // using the max count and the number of types seen a type can be determined.
            (5, _) => Hand.HandType.FiveOfAKind,
            (4, _) => Hand.HandType.FourOfAKind,
            (3, 2) => Hand.HandType.FullHouse,
            (3, _) => Hand.HandType.ThreeOfAKind,
            (2, 3) => Hand.HandType.TwoPair,
            (2, _) => Hand.HandType.OnePair,
             _     => Hand.HandType.HighCard,
        };
    }

    public delegate Hand.HandType HandTyper(Span<byte> span);
}

public readonly record struct Hand(string HandText, int Score) : IComparable<Hand> {
    public HandType Type { get; } = (HandType)(Score >> 20);

    public int CompareTo(Hand other) => Score.CompareTo(other.Score);

    public enum HandType : byte { 
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
    public byte Score { get; } = GetScore(Symbol);

    public Card(byte score) : this(GetSymbol(score)) { }
    
    // Gets the numeric value of this card, 1-13 (0 reserved for wildcards).
    public static byte GetScore(char card) => card switch {
        'A' => 13,
        'K' => 12,
        'Q' => 11,
        'J' => 10,
        'T' => 9,
        <= '9' and >= '2' => (byte)(card - '1'),
        _ => throw new ArgumentException("Invalid card symbol.", nameof(card)),
    };

    // Gets the numeric value of this card, with a wildcard 0-13, old value will be skipped.
    public static byte GetScore(char card, char wildcard) =>
        card != wildcard ? GetScore(card) : (byte)0;

    public static char GetSymbol(byte score) => score switch {
        13 => 'A',
        12 => 'K',
        11 => 'Q',
        10 => 'J',
        9  => 'T',
        <= 8 and >= 1 => (char)(score + '1'),
        0 => '*',
        _ => throw new ArgumentException("Invalid card score.", nameof(score)),
    };

    public const string VALID_SYMBOLS = "AKQJT98765432";
}

internal static class CardParser {
    public static IEnumerable<HandBid> Parse(string text) {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        int index = 0; char digit;
        do  {
            // parse the hand.
            var hand = text[index..(index + 5)];
            index += 5;

            // parse the bid.
            int bid = 0;
            while ((++index < text.Length) && char.IsAsciiDigit(digit = text[index])) {
                bid *= 10;
                bid += digit - '0';
            }
            
            yield return new HandBid(hand.ToString(), bid);

            // Skip over EOL characters
            index = text.IndexOf('\n', index) + 1;
        } while (index != 0 && index < text.Length);
    }
}