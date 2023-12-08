namespace AdventOfCode.Helpers;

using System.Collections;

public interface IIndexedEnumerator<T> : IEnumerator<T>
{
    public int CurrentIndex { get; }
}

public static class StringHelper
{
    public static IIndexedEnumerator<char> GetLoopingEnumerator(this string source) => new StringLooper(source);

    /// <summary>A string enumerator that will continuously loop over a string.</summary>
    /// <param name="source">The string to enumerate.</param>
    public class StringLooper(string source) : IIndexedEnumerator<char>
    {
        private readonly CharEnumerator _enumurator = string.IsNullOrEmpty(source) ? throw new ArgumentException("Cannot be empty", nameof(source))
                                                                                   : source.GetEnumerator();
        /// <inheritdoc/>
        public char Current => _enumurator.Current;
        /// <inheritdoc/>
        object IEnumerator.Current => _enumurator.Current;
        /// <summary>The enumerators current positon within the string. -1 before enumeration starts.</summary>
        public int CurrentIndex { get; private set; } = -1;

        /// <inheritdoc/>
        public void Dispose() => _enumurator.Dispose();

        /// <summary>Moves to the next location in the string, or back ot the start when the end is reached.</summary>
        /// <returns>Always true.</returns>
        public bool MoveNext()
        {
            if (_enumurator.MoveNext())
            {
                CurrentIndex++;
                return true;
            }

            Reset();
            return MoveNext();
        }

        /// <inheritdoc/>
        public void Reset()
        {
            CurrentIndex = -1;
            _enumurator.Reset();
        }
    }
}
