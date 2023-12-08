namespace AdventOfCode.Helpers;

internal class MathHelper
{
    public static long GCD(long num1, long num2) {
        while (num1 != 0 && num2 != 0) {
            if (num1 > num2)
                num1 %= num2;
            else 
                num2 %= num1;
        }

        return num1 | num2;
    }

    public static long LCM(long num1, long num2) =>
        Math.Abs(num1 * num2) / GCD(num1, num2);

}
