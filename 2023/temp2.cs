public class Temp {
    public static void Solve(IEnumerable<string> input) { 
        var parseLine = (string line, string cardOrder, string jokers) => {
            var parts = line.Split(' ');
            var hand = parts[0];
            var bid = int.Parse(parts[1]);

            HandType handType = HandType.FiveOfAKind;
            var handWithoutJokers = jokers != "" ? hand.Replace(jokers, "") : hand;
            var numJokers = hand.Length - handWithoutJokers.Length;
            var groups =
                  handWithoutJokers
                  .GroupBy(x => x)
                  .Select(x => x.Count())
                  .OrderByDescending(x => x)
                  .Concat(new[] { 0 })
                  .ToArray();
            groups[0] += numJokers;
            handType = groups switch {
                [5, ..] => HandType.FiveOfAKind,
                [4, ..] => HandType.FourOfAKind,
                [3, 2, ..] => HandType.FullHouse,
                [3, ..] => HandType.ThreeOfAKind,
                [2, 2, ..] => HandType.TwoPair,
                [2, ..] => HandType.OnePair,
                [..] => HandType.HighCard,
            };

        var weight = hand.Select((card, index) => cardOrder.IndexOf(card) << (4 * (5 - index))).Sum();

        return (hand, handType, weight, bid);
        };

        var solve = (string cardOrder, string jokers) => {
            var hands = input.Select(line => parseLine(line, cardOrder, jokers));
            var orderedHands = hands.OrderBy(x => x.handType).ThenBy(x => x.weight);
            var result = orderedHands.Select((hand, index) => hand.bid * (index + 1)).Sum();
            return result;
        };

        var solve2 = (string cardOrder, string jokers) => {
            var hands = input.Select(line => parseLine(line, cardOrder, jokers));
            var orderedHands = hands.OrderBy(x => x.handType).ThenBy(x => x.weight);
            foreach (var hand in orderedHands) Console.WriteLine(hand);
            return orderedHands;
        };

        var result3 = solve2("J23456789TQKA", "J");
        var result1 = solve("23456789TJQKA", "");
    Console.WriteLine($"Result1 = {result1}");

    var result2 = solve("J23456789TQKA", "J");
    Console.WriteLine($"Result2 = {result2}");

    }

    enum HandType
    {
        HighCard,
        OnePair,
        TwoPair,
        ThreeOfAKind,
        FullHouse,
        FourOfAKind,
        FiveOfAKind,
    };
}