string testData14 =
@"O....#....
O.OO#....#
.....##...
OO.#O....O
.O.....O#.
O.#..O.#.#
..O..#O..O
.......O..
#....###..
#OO..#....";

string testData13 = 
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

var solutions = new AdventSolutions();
var today = solutions.GetDay(2023, 14);

await today.DownloadInputAsync();

// today.SetTestInput(testData14);
// today.Part1();
// today.Part2();

await today.CheckPart1Async();
await today.CheckPart2Async();

// await Task.WhenAll(solutions.Select(async day => { await day.CheckPart1Async(); await day.CheckPart2Async(); }));

today.Benchmark();
