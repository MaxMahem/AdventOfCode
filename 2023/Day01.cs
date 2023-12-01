namespace AdventOfCode;

using AdventOfCodeSupport;
using Iced.Intel;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;

public class Day01 : AdventBase
{
    protected override void InternalOnLoad() {
        // Optional override, runs before Part1/2.
        // Benchmarked separately.
    }

    protected override object InternalPart1() {
        var enumerator = Input.Text.GetEnumerator();
        int sum = 0;
        int resultDigit;

        // this while will step through all "lines". If empty will just fall through.
        do {
            // find the first digit.
            resultDigit = GetNextDigit(enumerator);

            // AoC code does not appear to have any cases with *no* digits in a line,
            // but we'll handle that case just in case.
            if (resultDigit == 0) continue;

            // if there are no more digits in the line, then the first digit is the second digit.
            // set the first digit as the default value.
            int firstDigit  = resultDigit;
            int secondDigit = resultDigit;

            // this while will step through until the end of line or end of enumeration, not just the next digit.
            // multiple digits may be found, we only care about the last one.
            while ((resultDigit = GetNextDigit(enumerator)) != 0) {
                // if a digit is found, update the second digit. If no digit found, keep the same digit.
                secondDigit = resultDigit != 0 ? resultDigit : secondDigit;
            }

            sum += firstDigit * 10 + secondDigit;
        } while (enumerator.MoveNext());
        return sum;

        // Gets the Next Digit, or advances to the next line, or end of enumeration.
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
        // for example, if the letters 'eigh' have been seen, then the 7th value with have a value of 4
        // meaning the 5th index should be checked next
        Span<int> wordsMatchedLetters = stackalloc int[WordNumbers.Length];
        int sum = 0;
        int resultDigit;

        // this while will step through all "lines". If empty will just fall through.
        do {
            // find the first digit.
            resultDigit = GetNextDigit2(enumerator, wordsMatchedLetters);

            // AoC code does not appear to have any cases with *no* digits in a line,
            // but we'll handle that case just in case.
            if (resultDigit == 0) continue;

            // if there are no more digits in the line, then the first digit is the second digit.
            // set the first digit as the default value.
            int firstDigit  = resultDigit;
            int secondDigit = resultDigit;

            // this while will step through until the end of line or end of enumeration, not just the next digit.
            // multiple digits may be found, we only care about the last one.
            while ((resultDigit = GetNextDigit2(enumerator, wordsMatchedLetters)) != 0) {
                // if a digit is found, update the second digit. If no digit found, keep the same digit.
                secondDigit = resultDigit != 0 ? resultDigit : secondDigit;
            }

            sum += firstDigit * 10 + secondDigit;
        } while (enumerator.MoveNext());

        return sum;
    }

    private static int GetNextDigit2(CharEnumerator charEnumerator, Span<int> wordsMatchedLetters) {
        var resultDigit = 0;

        while (resultDigit == 0 && charEnumerator.MoveNext()) {
            char character = charEnumerator.Current;
            // handle number digit case.
            if (char.IsAsciiDigit(character)) return charEnumerator.Current - '0';

            // handle EOL character
            if (character == '\r') return 0;

            // letter case. Itterate through all words in our map.
            for (int wordIndex = 0; wordIndex < WordNumbers.Length; wordIndex++) {
                string word = WordNumbers[wordIndex].Word;
                
                // get how far into the current word a candiate character should match
                int matchedLetters = wordsMatchedLetters[wordIndex];

                // check if the current character matches
                if (word[matchedLetters] == character) {
                    // increase the matched letter count for that word
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
                    // check again because the first letter could repeat
                    matchedLetters = 0;
                    if (word[matchedLetters] == character)
                        wordsMatchedLetters[wordIndex]++;
                }
            }
        }
        // case for end of enumeration.
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