using AdventOfCode.Helpers;
using AdventOfCodeSupport;

string testData1 = //  55 13
@"Time:      7  15   30
Distance:  9  40  200";

var solutions = new AdventSolutions();
var today = solutions.GetDay(2023, 6);

// await today.DownloadInputAsync();

// today.SetTestInput(testData1);
today.Part1().Part2();

bool testAll = false;
// bool testAll = true;

// await today.CheckPart1Async();
// await today.CheckPart2Async();

//await Task.WhenAll(testAll ? solutions.Select(day => day.CheckBothParts()).SelectMany(e => e)
//                           : today.CheckBothParts());

// today.Benchmark();