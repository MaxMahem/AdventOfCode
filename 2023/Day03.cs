namespace AdventOfCode._2023;

using AdventOfCode.Helpers;
using AdventOfCodeSupport;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

public class Day03 : AdventBase
{
    Grid? _grid;

    protected override void InternalOnLoad() {
        int y = 0;
        Dictionary<(int X, int Y), char> symbols = [];
        Dictionary<(int X, int Y), int> numbers  = [];

        foreach (var line in Input.Text.AsSpan().EnumerateLines()) {
            for (int index = 0; index < line.Length; index++) {
                

                var window = line[index..];
                int hitIndex = line[index..].IndexOfAnyExcept('.');
                if (hitIndex == -1) continue;

                char c;
                (int X, int Y) point = (index + hitIndex, y);
                if (!char.IsDigit(c = window[hitIndex])) {
                    symbols.Add(point, c);
                    
                    index += hitIndex; // advance our window to 1the hit position.
                    continue;
                }

                int number = 0;
                int digitLength = 0;
                while ((hitIndex + digitLength) < window.Length && char.IsAsciiDigit(c = window[hitIndex + digitLength])) {
                    number = number * 10 + (c - '0');
                    digitLength++;
                }

                numbers.Add(point, number);

                // advance our window to one past the last digit.
                index += hitIndex + digitLength - 1;
            }
            y++;
        }
        _grid = new Grid(symbols, numbers);
    }

    protected override object InternalPart1() {
        int sum = 0;
        foreach(var kvpNumber in _grid!.Numbers) {
            if (_grid.GetNeighbors(kvpNumber.Key, kvpNumber.Value).Any()) {
                sum += kvpNumber.Value;
            }
        }
        return sum;
    }

    protected override object InternalPart2() {
        int sum = 0;
        List<object> neigh = [];
        var stars = _grid!.Symbols.Where(kvp => kvp.Value == '*').ToArray();
        foreach(var kvpSymbol in _grid!.Symbols.Where(kvp => kvp.Value == '*')) {
            int[] neighbors = _grid.GetNeighbors(kvpSymbol.Key).Take(3).ToArray();
            neigh.Add(neighbors);
            if (neighbors.Length == 2) {
                // Console.WriteLine($"{kvpSymbol.Key, -10} :  {string.Join(' ', neighbors), -10} = {neighbors.Product()}");
                sum += neighbors.Product();
            }
        }
        return sum;
    }
}


public class Grid(IDictionary<(int X, int Y), char> symbols, IDictionary<(int X, int Y), int> numbers)
{
    public IReadOnlyDictionary<(int X, int Y), char> Symbols { get; } = symbols.ToImmutableDictionary();
    public IReadOnlyDictionary<(int X, int Y), int> Numbers { get; } = numbers.ToImmutableDictionary();

    public IEnumerable<char> GetNeighbors((int X, int Y) point, int number) {
        // search box
        (int X, int Y) tl = (point.X - 1,                   point.Y - 1);
        (int X, int Y) br = (point.X + number.Digits(), point.Y + 1);
        char symbol;

        // top and bottom row. 
        for (int x = tl.X; x <= br.X; x++) {
            if (Symbols.TryGetValue((x, tl.Y), out symbol)) yield return symbol;
            if (Symbols.TryGetValue((x, br.Y), out symbol)) yield return symbol;
        }

        // middle right.
        if (Symbols.TryGetValue((tl.X, point.Y), out symbol)) yield return symbol;
        if (Symbols.TryGetValue((br.X, point.Y), out symbol)) yield return symbol;
    }

    public IEnumerable<int> GetNeighbors((int X, int Y) point) {
        // search box
        (int X, int Y) tl = (point.X - 1, point.Y - 1);
        (int X, int Y) br = (point.X + 1, point.Y + 1);
        int number;

        // top and bottom row. X offset by 1 because left side is handled below.
        for (int x = tl.X + 1; x <= br.X; x++) {
            if (Numbers.TryGetValue((x, tl.Y), out number)) yield return number;
            if (Numbers.TryGetValue((x, br.Y), out number)) yield return number;
        }

        // middle right.
        if (Numbers.TryGetValue((br.X, point.Y), out number)) yield return number;

        // left side takes special handling. Search the left edge 3 places out for a number (up to 3 digits long)
        // if a number is found, yield and break, do no more searches on that row.
        for (int x = tl.X; x >= tl.X - 2; x--) { 
            if (Numbers.TryGetValue((x, tl.Y),    out number) && x + number.Digits() > tl.X) {
                yield return number; break; 
            }
        }
        for (int x = tl.X; x >= tl.X - 2; x--) {
            if (Numbers.TryGetValue((x, point.Y), out number) && x + number.Digits() > tl.X) { 
                yield return number; break; 
            }
        }
        for (int x = tl.X; x >= tl.X - 2; x--) { 
            if (Numbers.TryGetValue((x, br.Y),    out number) && x + number.Digits() > tl.X) { 
                yield return number; break; 
            }
        }
    }
}

public readonly ref struct ReadOnlySpanTextGrid {
    public readonly int Width;
    public readonly int Height;
    public readonly ReadOnlySpan<char> Text;

    private readonly int _terminatorLength;
    private readonly int _rawWidth;

    public ReadOnlySpanTextGrid(ReadOnlySpan<char> text, string terminator) {
        if (terminator.Length == 0) throw new ArgumentException("Cannot be empty.", nameof(terminator));

        Text = text;
        _terminatorLength = terminator.Length;

        Width  = Text.IndexOf(terminator[0]) - _terminatorLength + 1;
        Height = Text.Length / (Width + _terminatorLength);
        _rawWidth = Width + _terminatorLength + 1;
    }

    public char GetCharacter(int x, int y) {
        if (x < 0 || x >= Width)  throw new ArgumentOutOfRangeException(nameof(x), x, "Value beyond grid boundaries.");
        if (y < 0 || y >= Height) throw new ArgumentOutOfRangeException(nameof(y), y, "Value beyond grid boundaries.");

        int index = y * _rawWidth + x;

        return Text[index]; // 6, 3 # (42)
    }
}

public class Grid2(IDictionary<(int X, int Y), char> symbols, List<SortedList<int, int>> numbers)
{
    public IReadOnlyDictionary<(int X, int Y), char> Symbols { get; } = symbols.ToImmutableDictionary();
    public IReadOnlyList<SortedList<int, int>> Numbers { get; } = numbers.ToImmutableArray();

    public IEnumerable<char> GetNeighbors((int X, int Y) point, int number) {
        // search box
        (int X, int Y) tl = (point.X - 1, point.Y - 1);
        (int X, int Y) br = (point.X + number.Digits(), point.Y + 1);
        char symbol;

        // top and bottom row. 
        for (int x = tl.X; x <= br.X; x++) {
            if (Symbols.TryGetValue((x, tl.Y), out symbol)) yield return symbol;
            if (Symbols.TryGetValue((x, br.Y), out symbol)) yield return symbol;
        }

        // middle row. // x 3 y 4
        if (Symbols.TryGetValue((tl.X, point.Y), out symbol)) yield return symbol;
        if (Symbols.TryGetValue((br.X, point.Y), out symbol)) yield return symbol;

        yield break;
    }

    public IEnumerable<int> GetNeighbors((int X, int Y) point) {
        // search box
        (int X, int Y) tl = (point.X - 1, point.Y - 1);
        (int X, int Y) br = (point.X + 1, point.Y + 1);
        int number;

        // top and bottom row. 
        for (int x = tl.X; x <= br.X; x++) {
            if (Numbers[tl.Y].TryGetValue(x, out number)) yield return number;
            if (Numbers[br.Y].TryGetValue(x, out number)) yield return number;
        }

        // middle row.
        if (Numbers[point.Y].TryGetValue(tl.X, out number)) yield return number;
        if (Numbers[point.Y].TryGetValue(br.X, out number)) yield return number;

        // left side.
        for (int y = tl.Y; y <= br.Y; y++) {
//            Numbers[y].Keys.BinarySearch(x)
        }

        yield break;
    }
}