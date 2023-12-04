using AdventOfCodeSupport;

string testData1 =
@"467..114..
...*......
..35..633.
......#...
617*......
.....+.58.
..592.....
......755.
...$.*....
.664.598..";

var solutions = new AdventSolutions();
var today = solutions.GetDay(2023, 3);

// Console.WriteLine("TestData P1 = 467835");
// today.SetTestInput(testData1);

 await today.Part1().CheckPart1Async();
 await today.Part2().CheckPart2Async();

today.Benchmark();