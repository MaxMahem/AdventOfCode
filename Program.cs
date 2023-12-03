using AdventOfCodeSupport;

string testData =
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

string testData2 =
@"
.65...261..
....*......
.....453...";



var solutions = new AdventSolutions();
var today = solutions.GetDay(2023, 3);

// Console.WriteLine("TestData P1 = 467835");
//today.SetTestInput(testData2);

 await today.Part1().CheckPart1Async();
 await today.Part2().CheckPart2Async();

// today.Benchmark();