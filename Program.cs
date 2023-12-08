using AdventOfCode._2023;
using AdventOfCode.Helpers;

string testData1 = //  55 13
@"32T3K 765
T55J5 684
KK677 28
KTJJT 220
QQQJA 483";

// Part 1: 6592
// Part 2: 6839

string testData2 =
@"2345A 1
Q2KJJ 13
Q2Q2Q 19
T3T3J 17
T3Q33 11
2345J 3
J345A 2
32T3K 5
T55J5 29
KK677 7
KTJJT 34
QQQJA 31
JJJJJ 37
JAAAA 43
AAAAJ 59
AAAAA 61
2AAAA 23
2JJJJ 53
JJJJ2 41";

var solutions = new AdventSolutions();
var today = solutions.GetDay(2023, 7);

// await today.DownloadInputAsync();

// today.SetTestInput(testData2);
// today.Part1().Part2();

await today.CheckPart1Async();
await today.CheckPart2Async();

// await Task.WhenAll(solutions.Select(async day => { await day.CheckPart1Async(); await day.CheckPart2Async(); }));

// today.Benchmark();