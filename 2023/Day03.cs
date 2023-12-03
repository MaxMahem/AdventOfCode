namespace AdventOfCode._2023;

using AdventOfCode.Helpers;
using AdventOfCodeSupport;
using RBush;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;



public class Day03 : AdventBase
{
    Grid? _grid;

    protected override void InternalOnLoad() {
        int y = 0;
        Dictionary<(int X, int Y), int> numbers  = [];
        Dictionary<(int X, int Y), char> symbols = [];

        ReadOnlySpanTextGrid grid = new ReadOnlySpanTextGrid(Input.Text.AsSpan(), "\r\n");
        grid.GetCharacter(6, 3);

        foreach (var line in Input.Text.AsSpan().EnumerateLines()) {
            for (int index = 0; index < line.Length; index++) {
                var window = line[index..];
                int hitIndex = line[index..].IndexOfAnyExcept('.');
                if (hitIndex == -1) continue;

                char c;
                (int X, int Y) point = (index + hitIndex, y);
                if (!char.IsDigit(c = window[hitIndex])) {
                    symbols.Add(point, c);
                    
                    index += hitIndex; // advance our window to one past the hit position.
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
        foreach(var kvpSymbol in _grid!.Symbols.Where(kvp => kvp.Value == '*')) {
            var neighboors = _grid.GetNeighbors(kvpSymbol.Key).ToArray();
            if (neighboors.Length == 2) sum += neighboors.Product();
        }
        return sum;
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

public class Grid(IDictionary<(int X, int Y), char> symbols, IDictionary<(int X, int Y), int> numbers) {
    public IReadOnlyDictionary<(int X, int Y), char> Symbols { get; }= symbols.ToImmutableDictionary();
    public IReadOnlyDictionary<(int X, int Y), int>  Numbers { get; }= numbers.ToImmutableDictionary();

    public IEnumerable<char> GetNeighbors((int X, int Y) point, int number) {
        // search box
        (int X, int Y) tl = (point.X - 1,                   point.Y - 1);
        (int X, int Y) br = (point.X + number.DigitCount(), point.Y + 1);
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
            if (Numbers.TryGetValue((x, tl.Y), out number)) yield return number;
            if (Numbers.TryGetValue((x, br.Y), out number)) yield return number;
        }

        // middle row.
        if (Numbers.TryGetValue((tl.X, point.Y), out number)) yield return number;
        if (Numbers.TryGetValue((br.X, point.Y), out number)) yield return number;

        yield break;
    }
}