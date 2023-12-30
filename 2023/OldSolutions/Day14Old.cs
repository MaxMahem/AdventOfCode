namespace AdventOfCode._2023;

using AdventOfCode.Helpers;

using IntPoint = Helpers.GridPoint<int>;

public class DayOld14 : AdventBase
{
    SatDishRockMapOld? _dishRockMap;

    protected override void InternalOnLoad() {
        _dishRockMap = new (SatDishMapParser.Parse(Input.Text));
    }

    protected override object InternalPart1() => _dishRockMap!.TiltN().Weigh();

    protected override object InternalPart2() => _dishRockMap!.Cycle(CYCLES).Weigh();

    public const int CYCLES = 1_000_000_000;
}

public class SatDishRockMapOld
{
    public IReadOnlyDictionary<IntPoint, IRock> RockMap { get; }
    public GridRectangle<int> Boundary { get; }

    public SatDishRockMapOld(SatDishMap satDishRockMap) {
        this.RockMap  = satDishRockMap.Rocks.ToImmutableDictionary(rock => rock.Location);
        this.Boundary = satDishRockMap.Boundary;
    }

    private SatDishRockMapOld(IReadOnlyDictionary<IntPoint, IRock> rockMap, GridRectangle<int> boundary) {
        this.RockMap = rockMap;
        this.Boundary = boundary;
    }

    public int Weigh() => RockMap.Values.OfType<RollingRock>().Select(rock => Boundary.Height - rock.Location.Y).Sum();

    /// <summary>Translates the current map into a BigInteger bitmask for memoization.</summary>
    public BigInteger GetValue() {
        int byteArraySize = Boundary.Width * Boundary.Height / BYTE_SIZE + 1;
        Span<byte> byteArray = stackalloc byte[byteArraySize];

        foreach (var coordinate in this.RockMap.Keys) {
            int bitIndex = coordinate.X * Boundary.Width + coordinate.Y;

            int byteIndex = bitIndex / BYTE_SIZE;
            int bitPosition = bitIndex % BYTE_SIZE;

            byteArray[byteIndex] |= (byte)(1 << bitPosition);
        }
        return new BigInteger(byteArray, true);
    }

    public override string ToString() {
        int bufferSize = Boundary.Height * (Boundary.Width + Environment.NewLine.Length);
        return string.Create(bufferSize, this, MapToBuffer);
    }

    public SatDishRockMapOld TiltN() => Tilt(North, PointEnumerator(EnumXLR, EnumYUD));
    public SatDishRockMapOld TiltS() => Tilt(South, PointEnumerator(EnumXLR, EnumYDU));
    public SatDishRockMapOld TiltE() => Tilt(East, PointEnumerator(EnumXRL, EnumYUD));
    public SatDishRockMapOld TiltW() => Tilt(West, PointEnumerator(EnumXLR, EnumYUD));

    public SatDishRockMapOld Cycle(int cycles) {
        Dictionary<BigInteger, int> cache = [];
        SatDishRockMapOld cycledMap = this;

        for (int cycle = 0; cycle < cycles; cycle++) {
            cycledMap = cycledMap.TiltN().TiltW().TiltS().TiltE();
            BigInteger currentValue = cycledMap.GetValue();

            if (cache.TryGetValue(currentValue, out var cached)) {
                var remainingCycles = cycles - cycle - 1;
                var loopSize = cycle - cached;

                var loopRemaining = remainingCycles % loopSize;
                cycle = cycles - loopRemaining - 1;
            }
            cache[currentValue] = cycle;
        }
        return cycledMap;
    }

    private SatDishRockMapOld Tilt(IntPoint direction, IEnumerable<IntPoint> pointIteration) {
        Dictionary<IntPoint, IRock> newMap = [];

        foreach (var currentPoint in pointIteration) {
            RockMap.TryGetValue(currentPoint, out IRock? rock);
            switch (rock) {
                case FixedRock fixedRock:
                    newMap[fixedRock.Location] = fixedRock;
                    break;
                case RollingRock rollingRock:
                    var newRock = Roll(rollingRock);
                    newMap[newRock.Location] = newRock;
                    break;
                case null:
                    break;
                default:
                    throw new InvalidOperationException("Invalid rock type");
            }
        }
        return new SatDishRockMapOld(newMap, Boundary);

        // moves the rock to the next valid location in the new map.
        RollingRock Roll(RollingRock rock) {
            (IntPoint nextLocation, IntPoint currentLocation) = (rock.Location + direction, rock.Location);
            while (!newMap.ContainsKey(nextLocation) && Boundary.ContainsPoint(nextLocation)) {
                (nextLocation, currentLocation) = (currentLocation + direction, nextLocation);
            }
            return new RollingRock(currentLocation);
        }
    }

    private static IEnumerable<IntPoint> PointEnumerator(IEnumerable<int> enumerator1, IEnumerable<int> enumerator2) =>
        enumerator1.SelectMany(x => enumerator2.Select(y => new IntPoint(x, y)));

    private IEnumerable<int> EnumXLR => Enumerable.Range(0, Boundary.Width);
    private IEnumerable<int> EnumXRL => Enumerable.Range(0, Boundary.Width).Select(x => Boundary.Width - x - 1);
    private IEnumerable<int> EnumYUD => Enumerable.Range(0, Boundary.Height);
    private IEnumerable<int> EnumYDU => Enumerable.Range(0, Boundary.Height).Select(y => Boundary.Height - y - 1);

    private static IntPoint North = new(0, -1);
    private static IntPoint South = new(0, +1);
    private static IntPoint East = new(+1, 0);
    private static IntPoint West = new(-1, 0);

    private static void MapToBuffer(Span<char> buffer, SatDishRockMapOld state) {
        for (int y = 0, offset = 0; y < state.Boundary.Height; y++) {
            for (int x = 0; x < state.Boundary.Width; x++) {
                state.RockMap.TryGetValue(new(x, y), out IRock? rock);
                buffer[offset..(offset + state.Boundary.Width)][x] = rock switch {
                    IRock => rock.Symbol,
                    null => '.',
                };
            }
            offset += state.Boundary.Width;

            Environment.NewLine.AsSpan().CopyTo(buffer[offset..]);
            offset += Environment.NewLine.Length;
        }
    }

    const int BYTE_SIZE = sizeof(byte) * 8;
}