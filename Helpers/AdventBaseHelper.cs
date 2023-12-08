namespace AdventOfCode.Helpers;
using AdventOfCodeSupport;
using System.Threading.Tasks;

public static class AdventBaseHelper
{
    public static IEnumerable<Task> CheckBothParts(this AdventBase day) => new[] { day.CheckPart1Async(), day.CheckPart2Async() };
}