namespace AdventOfCode._2022;
public class Day02 : AdventBase
{
    IEnumerable<RPS.Round>? _rounds;

    protected override void InternalOnLoad() {
        _rounds = RPSGuideParser.Parse(Input.Text);
    }

    protected override object InternalPart1() => _rounds!.Sum(round => round.Score);

    protected override object InternalPart2() => 0;
}

public class RPS { 
    public readonly record struct Round(Throw PlayerThrow, Throw OpponentThrow) {
        public int Score { get; } = ScoreRound(PlayerThrow, OpponentThrow);

        public Round(char playerThrow, char opponetThrow) : this(ParseSymbol(playerThrow), ParseSymbol(opponetThrow)) { }

        public const int WIN_VALUE  = 6;
        public const int DRAW_VALUE = 3;
        public const int LOSS_VALUE = 0;

        public const int ROCK_VALUE     = 1;
        public const int PAPER_VALUE    = 2;
        public const int SCISSORS_VALUE = 3;

        public static int ScoreThrow(Throw rpsThrow) => (int)rpsThrow;

        public static int ScoreRound(Throw playerThrow, Throw opponentThrow) {
            int gameScore = playerThrow == opponentThrow ? DRAW_VALUE 
                : (playerThrow, opponentThrow) switch {
                (Throw.Rock,     Throw.Paper)     => LOSS_VALUE,
                (Throw.Rock,     Throw.Scissors)  => WIN_VALUE,
                (Throw.Paper,    Throw.Scissors)  => LOSS_VALUE,
                (Throw.Paper,    Throw.Rock)      => WIN_VALUE,
                (Throw.Scissors, Throw.Rock)      => LOSS_VALUE,
                (Throw.Scissors, Throw.Paper)     => WIN_VALUE,
                _ => throw new InvalidOperationException("Invalid throws recieved.")
            };
            return gameScore + ScoreThrow(playerThrow);
        }

        void BuildRoundTable() {
            foreach(var playerThrow in new char ['A', 'B', 'C']) {
                foreach(var opponentThrow in new char['X', 'Y', 'Z']) {

                }
            }
        }

        private static RPS.Throw ParseSymbol(char symbol) => symbol switch {
            'A' => RPS.Throw.Rock,
            'B' => RPS.Throw.Paper,
            'C' => RPS.Throw.Scissors,
            'X' => RPS.Throw.Rock,
            'Y' => RPS.Throw.Paper,
            'Z' => RPS.Throw.Scissors,
            _ => throw new ArgumentException("Invalid symbol.", nameof(symbol)),
        };
    }

    public enum Throw {
        Rock     = Round.ROCK_VALUE,
        Paper    = Round.PAPER_VALUE,
        Scissors = Round.SCISSORS_VALUE,
    };


}

internal static class RPSGuideParser
{
    public static IEnumerable<RPS.Round> Parse(string text) {
        List<RPS.Round> rounds = [];
        foreach (var line in text.AsSpan().EnumerateLines()) {
            if (line.IsWhiteSpace()) continue;
            rounds.Add(new(line[0], line[2]));
        }
        return rounds;
    }
}