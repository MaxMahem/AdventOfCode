namespace AdventOfCodeMax._2023;

using AdventOfCodeSupport;
using System;

public class Day01 : AdventBase
{
    protected override object InternalPart1() {
        var enumerator = Input.Text.GetEnumerator();
        int sum = 0;
        int resultDigit;

        do {
            int firstDigit = GetNextDigit(enumerator);

            // AoC code does not appear to have any cases with *no* digits in a line, but handle that anyways.
            if (firstDigit == 0) continue;

            // secondDigit is firstDigit if no other digit is found.
            int secondDigit = firstDigit;

            // step through all remaining digits until end of line
            while ((resultDigit = GetNextDigit(enumerator)) != 0) {
                // if a new digit is found, it becomes the second digit, otherwise we keep the old one.
                secondDigit = resultDigit != 0 ? resultDigit : secondDigit;
            }

            sum += firstDigit * 10 + secondDigit;
        } while (enumerator.MoveNext());
        return sum;

        // Gets the Next Digit, or 0 if EoL.
        static int GetNextDigit(CharEnumerator charEnumerator) {
            while (charEnumerator.MoveNext() && charEnumerator.Current != '\r') {
                if (char.IsAsciiDigit(charEnumerator.Current)) return charEnumerator.Current - '0';
            }
            return 0;
        }
    }

    protected override object InternalPart2() {
        var enumerator = Input.Text.GetEnumerator();

        // wordsMatchedLetters keeps track of how many matching letters have been seen in every word
        Span<int> wordsMatchedLetters = stackalloc int[WordNumbers.Length];
        int sum = 0;
        int resultDigit;

        do {
            int firstDigit = GetNextDigit(enumerator, wordsMatchedLetters);

            // AoC code does not appear to have any cases with *no* digits in a line, but handle that anyways.
            if (firstDigit == 0) continue;

            // secondDigit is firstDigit if no other digit is found.
            int secondDigit = firstDigit;

            // step through all remaining digits until end of line
            while ((resultDigit = GetNextDigit(enumerator, wordsMatchedLetters)) != 0) {
                // if a digit is found, update the second digit. If no digit found, keep the same digit.
                secondDigit = resultDigit != 0 ? resultDigit : secondDigit;
            }

            sum += firstDigit * 10 + secondDigit;
        } while (enumerator.MoveNext());

        return sum;
    }

    // Gets the Next Digit, or 0 if EoL.
    private static int GetNextDigit(CharEnumerator charEnumerator, Span<int> wordsMatchedLetters) {
        var resultDigit = 0;

        while (resultDigit == 0 && charEnumerator.MoveNext()) {
            char character = charEnumerator.Current;

            if (char.IsAsciiDigit(character)) return charEnumerator.Current - '0';
            if (character == '\r') return 0;

            // letter case. Itterate through all words in our map.
            for (int wordIndex = 0; wordIndex < WordNumbers.Length; wordIndex++) {
                string word = WordNumbers[wordIndex].Word;
                
                // get how far into the current word a candiate character should match
                int matchedLetters = wordsMatchedLetters[wordIndex];

                if (word[matchedLetters] == character) {
                    wordsMatchedLetters[wordIndex]++;

                    // if the word has a number of letters matched equal to the word length, its a digit.
                    // reset the match counter for it and set it as our return value.
                    if (word.Length == wordsMatchedLetters[wordIndex]) {
                        wordsMatchedLetters[wordIndex] = 0;

                        // value is not return here because remaining words need to be checked in case of overlaps
                        resultDigit = WordNumbers[wordIndex].Digit; 
                    }
                }
                // if the current letter didn't match, reset the matched letter count for that word.
                else {
                    wordsMatchedLetters[wordIndex] = 0;

                    // check again for matches because the first letter could repeat
                    matchedLetters = 0;
                    if (word[matchedLetters] == character)
                        wordsMatchedLetters[wordIndex]++;
                }
            }
        }
        return resultDigit;
    }


    private static readonly (string Word, int Digit)[] WordNumbers = [
        ("one", 1),
        ("two", 2),
        ("three", 3),
        ("four", 4),
        ("five", 5),
        ("six", 6),
        ("seven", 7),
        ("eight", 8),
        ("nine", 9),
    ];
}