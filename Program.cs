using AdventOfCode.Helpers;

string testData1 = //  55 13
@"Time:      7  15   30
Distance:  9  40  200";

var solutions = new AdventSolutions();
var today = solutions.GetDay(2023, 7);

// await today.DownloadInputAsync();

// today.SetTestInput(testData1);
today.Part1().Part2();

// await today.CheckPart1Async();
// await today.CheckPart2Async();

// await Task.WhenAll(solutions.Select(async day => { await day.CheckPart1Async(); await day.CheckPart2Async(); }));

// today.Benchmark();