namespace Miz.Util.Span;

/// <summary>A fixed size last-in-first-out collection (stack) of instances of <typeparamref name="T"/>. 
/// Wraps a provided <see cref="Span{T}"/> providing stack-like access.</summary>
/// <remarks>Because the stack pattern ensures each item is initialized before it is read, this object can safetly be
/// used with uninitialized Spans or arrays for better performance.</remarks>
/// <typeparam name="T">The type of values in the <see cref="SpanStack{T}"/>.</typeparam>
public ref struct SpanStack<T>
{
    /// <summary>Underlying array. Items will be "pushed" in here, starting with the first at the top and working down.
    /// That way when the data is returned as an array, it will be in the correct "reverse" order.</summary>
    readonly Span<T> array;

    /// <summary>Index of the current top of the stack.</summary>
    int index;

    /// <summary>Gets the current number of items in this <see cref="SpanStack{T}"/>.</summary>
    public readonly int Count => array.Length - index;

    /// <summary>Gets the capacity of the <see cref="SpanStack{T}"/>.</summary>
    public readonly int Capacity => array.Length;

    /// <summary>Indicates if the <see cref="SpanStack{T}"/> is empty.</summary>
    public readonly bool IsEmpty => index >= array.Length;

    /// <summary>Constructs a new empty <see cref="SpanStack{T}"/> wrapping <paramref name="span"/>.</summary>
    /// <param name="span">The <see cref="Span{T}"/> to wrap.</param>
    public SpanStack(Span<T> span) {
        this.array = span;
        this.index = span.Length;
    }

    #region Stack like methods

    /// <summary>Removes all items from the <see cref="SpanStack{T}"/>.</summary>
    public void Clear() => index = array.Length;

    /// <summary>Returns the item at the top of the <see cref="SpanStack{T}"/> without removing it.</summary>
    /// <returns>The item at the top of the <see cref="SpanStack{T}"/>.</returns>
    /// <exception cref="InvalidOperationException">If this <see cref="SpanStack{T}"/> is empty.</exception>
    public readonly T Peek() {
        // assigning these to locals helps the compiler avoid a bounds check
        int index = this.index;
        Span<T> array = this.array;

        if (index >= (uint)array.Length) ThrowStackEmpty();

        return array[index];
    }

    /// <summary>Indicates whether there is an item at the top of the <see cref="SpanStack{T}"/>, and copies it to the 
    /// <paramref name="result"/> parameter if present without removing it from from the <see cref="SpanStack{T}"/>.</summary>
    /// <param name="result">If present, the object at the top of the <see cref="SpanStack{T}"/>; 
    /// otherwise, the default value of <typeparamref name="T"/>.</param>
    /// <returns><see langword="true"/> if there is an object at the top of the <see cref="SpanStack{T}"/>; 
    /// <see langword="false"/> if the <see cref="SpanStack{T}"/> is empty.</returns>
    public readonly bool TryPeek(out T? result) {
        // assigning these to locals helps the compiler avoid a bounds check
        int index = this.index;
        Span<T> array = this.array;

        if (index >= array.Length) {
            result = default;
            return false;
        }

        result = array[index];
        return true;
    }

    /// <summary>Inserts an item onto the top of the <see cref="SpanStack{T}"/> stack.</summary>
    /// <param name="value">The item to be pushed onto the <see cref="SpanStack{T}"/>.</param>
    /// <exception cref="InvalidOperationException">If this <see cref="SpanStack{T}"/> is full.</exception>
    public void Push(T value) {
        // assigning these to locals helps the compiler avoid a bounds check
        int index = this.index - 1;
        Span<T> array = this.array;

        if (index < 0) ThrowStackFull();

        array[index] = value;
        this.index = index;
    }

    /// <summary>Indicates whether there was room on the <see cref="SpanStack{T}"/> for another item, and pushes 
    /// <paramref name="value"/> to the top of the <see cref="SpanStack{T}"/> if there was.</summary>
    /// <param name="value">The value to be pushed to the top of the stack.</param>
    /// <returns><see langword="true"/> if there was room in the <see cref="SpanStack{T}"/> for the item; 
    /// <see langword="false"/> if the <see cref="SpanStack{T}"/> is full.</returns>
    public bool TryPush(T value) {
        // assigning these to locals helps the compiler avoid a bounds check
        int index = this.index - 1;
        Span<T> array = this.array;

        if (index < 0) return false;

        array[index] = value;
        this.index = index;
        return true;
    }

    /// <summary>Removes and returns the item on the top of the <see cref="SpanStack{T}"/>.</summary>
    /// <returns>The item removed from the top of the <see cref="SpanStack{T}"/>.</returns>
    /// <exception cref="InvalidOperationException">If this <see cref="SpanStack{T}"/> is empty.</exception>
    public T Pop() {
        // assigning these to locals helps the compiler avoid a bounds check
        int index = this.index;
        Span<T> array = this.array;

        if (index >= array.Length) ThrowStackEmpty();

        this.index = index + 1;
        return array[index];
    }

    /// <summary>Indicates whether there is an item at the top of the <see cref="SpanStack{T}"/>, and if so 
    /// removes it and copies it to the <paramref name="result"/> parameter.</summary>
    /// <param name="result">If present, the object at the top of the <see cref="SpanStack{T}"/>; 
    /// otherwise, the default value of <typeparamref name="T"/>.</param>
    /// <returns><see langword="true"/> if there is an object at the top of the <see cref="SpanStack{T}"/>; 
    /// <see langword="false"/> if the <see cref="SpanStack{T}"/> is empty.</returns>
    public bool TryPop(out T? value) {
        // assigning these to locals helps the compiler avoid a bounds check
        int index = this.index;
        Span<T> array = this.array;

        if (index >= array.Length) {
            value = default;
            return false;
        }

        value = array[index];
        this.index = index + 1;
        return true;
    }

    /// <summary>Determines whether an element is in this <see cref="SpanStack{T}"/>.</summary>
    /// <remarks>Searches linearly through the <see cref="SpanStack{T}"/> using <see cref="EqualityComparer{T}.Default"/>
    /// for <typeparamref name="T"/>.</remarks>
    /// <param name="item">The item to locate in this <see cref="SpanStack{T}"/>. May be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the object was found in the <see cref="SpanStack{T}"/>; 
    /// <see langword="false"/> otherwise.</returns>
    public readonly bool Contains(T item) => this.Contains(item, EqualityComparer<T>.Default);

    public readonly bool Contains(T item, IEqualityComparer<T> comparer) {
        // assigning these to locals helps the compiler avoid a bounds check
        int index = this.index;
        Span<T> array = this.array;

        // iterate down from the top of the array to the current index.
        for (int loopIndex = array.Length - 1; loopIndex >= index; loopIndex--)
            if (comparer.Equals(item, array[loopIndex])) return true;

        return false;
    }
    #endregion

    #region Span Like Methods
    /// <summary>Returns the current contents of this <see cref="SpanStack{T}"/> as a <see cref="Span{T}"/>.
    /// The <see cref="Span{T}"/> will contain only the current content, not the entire capacity.</summary>
    /// <returns>The current items in this <see cref="SpanStack{T}"/> as a <see cref="Span{T}"/>.</returns>
    public readonly Span<T> AsSpan() => array[index..];

    /// <summary>Copies the content of this <see cref="SpanStack{T}"/> to <paramref name="destination"/>. 
    /// Only the current content will be copied, not the entire capacity.</summary>
    /// <remarks>This method copies all of the stack to destination even they overlap.</remarks>
    /// <param name="destination">The <see cref="Span{T}"/> to be copied to.</param>
    /// <exception cref="ArgumentException">If <paramref name="destination"/> does not have sufficent capcity for
    /// all the current items in this <see cref="SpanStack{T}"/>.</exception>
    public readonly void CopyTo(Span<T> destination) => array[index..].CopyTo(destination);

    /// <summary>Attempts to copy the current content of this <see cref="SpanStack{T}"/> to 
    /// <paramref name="destination"/> and indicates whether the copy operation succeeded.
    /// Only the current content will be copied, not the entire capacity.</summary>
    /// <remarks>This method copies all of the stack to destination even they overlap.</remarks>
    /// <param name="destination">The <see cref="Span{T}"/> to be copied to.</param>
    /// <returns><see langword="true"/> if the copy operation succeeded; <see langword="false"/> otherwise.</returns>
    public readonly void TryCopyTo(Span<T> destination) => array[index..].TryCopyTo(destination);

    /// <summary>Copies the current content of this <see cref="SpanStack{T}"/> to a new array of <typeparamref name="T"/>.</summary>
    /// <remarks>This requires a new heap allocation.</remarks>
    /// <returns>A new array of <typeparamref name="T"/> containing the data in this <see cref="SpanStack{T}"/>.</returns>
    public readonly T[] ToArray() => array[index..].ToArray();

    /// <summary>Returns an enumerator for this <see cref="SpanStack{T}"/>.</summary>
    /// <returns>An enumerator for this <see cref="SpanStack{T}"/>.</returns>
    public readonly Span<T>.Enumerator GetEnumerator() => array[index..].GetEnumerator();
    #endregion

    /// <summary>Returns a <see cref="string"/> with the name of the type and it's capacity.</summary>
    public readonly override string ToString() => $"StackSpan<{typeof(T).Name}>[{this.array.Length}]";

    /// <summary>Converts a <see cref="Span{T}"/> to a <see cref="SpanStack{T}"/>.</summary>
    /// <param name="span">The <see cref="Span{T}"/> to convert to a <see cref="SpanStack{T}"/>.</param>
    public static implicit operator SpanStack<T>(Span<T> span) => new(span);

    /// <summary>Converts a <typeparamref name="T"/> array to a <see cref="SpanStack{T}"/>.</summary>
    /// <param name="array">The <see cref="Span{T}"/> to convert to a <see cref="SpanStack{T}"/>.</param>
    public static implicit operator SpanStack<T>(T[] array) => new(array);

    private static void ThrowStackEmpty() => throw new InvalidOperationException("Stack is empty.");
    private static void ThrowStackFull() => throw new InvalidOperationException("Stack is full.");
}