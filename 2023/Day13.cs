namespace AdventOfCode._2023;

using AdventOfCode.Helpers;
using System.Collections;

public class Day13 : AdventBase
{
    IEnumerable<ReflectionMap>? reflectionMaps;

    protected override void InternalOnLoad() {
        reflectionMaps = ReflectionMapsParser.Parse(Input.Text);
    }

    protected override object InternalPart1() {
        int sum = 0;
        foreach(var map in reflectionMaps) {
            sum += map.FindReflectionsY();
        }
        return 0;
    }

    protected override object InternalPart2() {

        return 0;
    }
}

public class ReflectionMap(IEnumerable<BitArray> mirrorLines) {
    private IReadOnlyList<BitArray> MirrorLines { get; } = mirrorLines.ToImmutableArray();
    public int Width { get; } = mirrorLines.FirstOrDefault()?.Length ?? 0;
    public int Height { get; } = mirrorLines.Count();

    public bool? this[int x, int y] {
        get {
            if (x < 0 || x >= Width) return null;
            if (y < 0 || y >= Height) return null;
            return this.MirrorLines[y][x];
        }
    }

    public int FindReflectionsY() {
        int yMirrorIndex = 1;
        for (;yMirrorIndex < Height-1; yMirrorIndex++) {
            bool reflection = false;
            for (int reflectionDistance = 1; reflectionDistance < Height; reflectionDistance++) {
                if (!CompareY(yMirrorIndex - reflectionDistance, yMirrorIndex + reflectionDistance - 1)) {
                    reflection = false;
                    break;
                } else {
                    reflection = true;
                }
            }
            if (reflection) return yMirrorIndex;
        }
        return -1;
    }

    private bool CompareY(int y1, int y2) {
        if (y1 < 0 || y1 > Height) return true;
        if (y2 < 0 || y2 > Height) return true;

        for (int x = 0; x < Width; x++) {
            if (this[y1, x] != this[y2, x]) return false;
        }

        return true;
    }
}

public static class ReflectionMapsParser
{
    public static IEnumerable<ReflectionMap> Parse(string input) {
        ArgumentNullException.ThrowIfNull(input);

        var text = input.AsSpan();

        List<ReflectionMap> reflectionMaps = [];
        List<BitArray> mirrors = [];

        foreach (var line in text.EnumerateLines()) {
            if (line.IsWhiteSpace()) {
                ReflectionMap reflectionMap = new(mirrors);
                reflectionMaps.Add(reflectionMap);
                mirrors.Clear();
                continue;
            }

            mirrors.Add(line.CreateBitArray(ParseSymbol));
        }

        if (mirrors.Count != 0) {
            ReflectionMap reflectionMap = new(mirrors);
            reflectionMaps.Add(reflectionMap);
        }

        return reflectionMaps;
    }

    public readonly static IReadOnlySet<char> ValidCharacters = "#".ToImmutableHashSet();

    private static bool ParseSymbol(char c) => c switch {
        '#' => true,
        '.' => false,
        _ => throw new ArgumentException("Invalid symbol.", nameof(c)),
    };
}