namespace AdventOfCode.Helpers;

using System.Collections;

public static class IEnumeratorHelper {
    public static IEnumerator<T> MakeLooping<T>(this IEnumerator<T> enumerator) => new LoopingEnumerator<T>(enumerator);
    public static IEnumerator MakeLooping(this IEnumerator enumerator) => new LoopingEnumerator(enumerator);

    private class LoopingEnumerator(IEnumerator enumerator) : IEnumerator {
        private readonly IEnumerator enumerator = enumerator;

        public object Current => this.enumerator.Current;

        /// <summary>Moves to the next location in the string, or back ot the start when the end is reached.</summary>
        /// <returns>Always true.</returns>
        public bool MoveNext() {
            if (this.enumerator.MoveNext()) return true;

            Reset();
            return MoveNext();
        }

        public void Reset() => this.enumerator.Reset();
    }

    private class LoopingEnumerator<T>(IEnumerator<T> enumerator) : IEnumerator<T> {
        private readonly IEnumerator<T> enumerator = enumerator;

        public T Current => this.enumerator.Current;

        object IEnumerator.Current => ((IEnumerator)this.enumerator).Current;

        public void Dispose() => this.enumerator.Dispose();
        
        /// <summary>Moves to the next location in the string, or back ot the start when the end is reached.</summary>
        /// <returns>Always true.</returns>
        public bool MoveNext() {
            if (this.enumerator.MoveNext()) return true;

            Reset();
            return MoveNext();
        }

        public void Reset() => this.enumerator.Reset();
    }
}
