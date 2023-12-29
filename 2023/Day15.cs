namespace AdventOfCode._2023;

using CommunityToolkit.HighPerformance;

using AdventOfCode.Helpers;

using Lens = (string Label, int FocalLength);

public class Day15 : AdventBase
{
    IEnumerable<InitializationStep> _intializationSequence = Enumerable.Empty<InitializationStep>();
    readonly LensLibrary _lensLibrary = new();

    protected override void InternalOnLoad() {
        _intializationSequence = [.. Input.Text.AsSpan().Trim().Tokenize(',')];
    }

    protected override object InternalPart1() => _intializationSequence.Sum(hash => hash.StepHash);

    protected override object InternalPart2() {
        foreach (var step in _intializationSequence) {
            _lensLibrary.ApplyStep(step);
        }
        return _lensLibrary.Power;
    }
}

public class LensLibrary {
    public IReadOnlyList<LensBox> LensBoxes { get; } 
        = Enumerable.Range(0, byte.MaxValue + 1).Select(id => new LensBox((byte)id)).ToImmutableArray();

    public bool ApplyStep(InitializationStep step) => step switch {
            RemoveStep            => LensBoxes[step.BoxId].RemoveLens(step.Label),
            InsertStep insertStep => LensBoxes[step.BoxId].AddOrReplace(step.Label, insertStep.Lens),
            _ => throw new ArgumentException("Invalid step type", nameof(step)),
        };

    public int Power => this.LensBoxes.Sum(lensBox => lensBox.Power);
}

public readonly struct LensBox(byte id) {
    readonly List<Lens> _lenses = [];
    public byte BoxId { get; } = id;
    
    // The BoxId factor is constant so has been factored out of the aggregate.
    public readonly int Power => _lenses.Index(1).Aggregate(0, (power, kvp) => power + kvp.Value.FocalLength * kvp.Key) * (this.BoxId + 1);

    public bool AddOrReplace(string label, Lens newLens) {
        if (ReplaceLens(label, newLens)) return true;
        _lenses.Add(newLens);
        return true;
    }
    public bool ReplaceLens(string label, Lens newLens) => _lenses.ReplaceFirst(lens => lens.Label == label, newLens);
    public bool RemoveLens(string label) => _lenses.RemoveFirst(lens => lens.Label == label);
}

public record RemoveStep(string Sequence) : InitializationStep(Sequence);

public record InsertStep(string Sequence) : InitializationStep(Sequence) {
    public byte FocalLength { get; } = (byte)(Sequence[^1] - '0');
    public Lens Lens => (Label, FocalLength);
}

public abstract record InitializationStep(string Sequence) {
    public string Sequence { get; } = !string.IsNullOrEmpty(Sequence) ? Sequence : throw new ArgumentException("Cannot be null or empty.", nameof(Sequence));
    public byte   StepHash => Sequence.Aggregate((byte)0, HashChar);
    public string Label { get; } = Sequence.TakeWhile(char.IsAsciiLetter).StringConcat();
    public byte   BoxId { get; } = Sequence.TakeWhile(char.IsAsciiLetter).Aggregate((byte)0, HashChar);

    public static implicit operator InitializationStep(ReadOnlySpan<char> sequence) => Create(sequence);

    public static InitializationStep Create(ReadOnlySpan<char> sequence) => sequence switch {
            [.., '-']               => new RemoveStep(sequence.ToString()),
            [.., >= '0' and <= '9'] => new InsertStep(sequence.ToString()),
            [.., _] => throw new ArgumentException("Invalid character in sequence.", nameof(sequence)),
            []      => throw new ArgumentException("Empty sequence.", nameof(sequence)),
        };

    static byte HashChar(byte hash, char c) => (byte)((hash + c) * 17 % 256);
}