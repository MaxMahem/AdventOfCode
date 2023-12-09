namespace AdventOfCode.Helpers;

internal class GenericMath
{
    public static T GCD<T>(T num1, T num2) 
        where T : INumber<T>, IDivisionOperators<T, T, T>, IBitwiseOperators<T, T, T>
        {
        while (num1 != T.Zero && num2 != T.Zero) {
            if (num1 > num2)
                num1 %= num2;
            else
                num2 %= num1;
        }

        return num1 | num2;
    }

    public static T LCM<T>(T num1, T num2)
        where T : INumber<T>, IDivisionOperators<T, T, T>, IBitwiseOperators<T, T, T>, ISignedNumber<T>
        => T.Abs(num1 * num2) / GCD(num1, num2);
}
