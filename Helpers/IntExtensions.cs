namespace AdventOfCode.Helpers;

public static partial class IntExtensions
{
    public static int DigitCount(this int number) => number switch {
        < 10         and > -10         => 1,
        < 100        and > -100        => 2,
        < 1000       and > -1000       => 3,
        < 10000      and > -10000      => 4,
        < 100000     and > -100000     => 5,
        < 1000000    and > -1000000    => 6,
        < 10000000   and > -10000000   => 7,
        < 100000000  and > -100000000  => 8,
        < 1000000000 and > -1000000000 => 9,
        _ => 10
    };

    public static bool IsEven(this int number) => number % 2 == 0;
}