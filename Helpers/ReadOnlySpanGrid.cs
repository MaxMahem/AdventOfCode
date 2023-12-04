namespace AdventOfCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public readonly ref struct ReadOnlySpanGrid
{
    public readonly int Width;
    public readonly int Height;
    public readonly ReadOnlySpan<char> Text;

    private readonly int _terminatorLength;
    private readonly int _rawWidth;

    public ReadOnlySpanGrid(ReadOnlySpan<char> text, string terminator) {
        if (terminator.Length == 0) throw new ArgumentException("Cannot be empty.", nameof(terminator));

        Text = text;
        _terminatorLength = terminator.Length;

        Width = Text.IndexOf(terminator[0]) - _terminatorLength + 1;
        Height = Text.Length / (Width + _terminatorLength);
        _rawWidth = Width + _terminatorLength + 1;
    }

    public char GetCharacter(int x, int y) {
        if (x < 0 || x >= Width)  throw new ArgumentOutOfRangeException(nameof(x), x, "Value beyond grid boundaries.");
        if (y < 0 || y >= Height) throw new ArgumentOutOfRangeException(nameof(y), y, "Value beyond grid boundaries.");

        int index = y * _rawWidth + x;

        return Text[index];
    }
}