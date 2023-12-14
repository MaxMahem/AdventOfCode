string testData1 = 
@"#.##..##.
..#.##.#.
##......#
##......#
..#.##.#.
..##..##.
#.#.##.#.

#...##..#
#....#..#
..##..###
#####.##.
#####.##.
..##..###
#....#..#";

string testData12 =
@"???.### 1,1,3
.??..??...?##. 1,1,3
?#?#?#?#?#?#?#? 1,3,1,6
????.#...#... 4,1,1
????.######..#####. 1,6,5
?###???????? 3,2,1";

var solutions = new AdventSolutions();
var today = solutions.GetDay(2023, 12);

await today.DownloadInputAsync();

// today.SetTestInput(testData12);
// today.Part1();
// today.Part2();

await today.CheckPart1Async();
await today.CheckPart2Async();

// await Task.WhenAll(solutions.Select(async day => { await day.CheckPart1Async(); await day.CheckPart2Async(); }));

// today.Benchmark();
