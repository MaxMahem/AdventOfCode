string testData1 =
@"0 3 6 9 12 15
1 3 6 10 15 21
10 13 16 21 30 45";

string testData2 =
@"10 13 16 21 30 45";


var solutions = new AdventSolutions();
var today = solutions.GetDay(2023, 9);

await today.DownloadInputAsync();

// today.SetTestInput(testData1);
// today.Part1();
// today.Part2();

await today.CheckPart1Async();
await today.CheckPart2Async();

// await Task.WhenAll(solutions.Select(async day => { await day.CheckPart1Async(); await day.CheckPart2Async(); }));

// today.Benchmark();
