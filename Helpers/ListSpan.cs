namespace AdventOfCode.Helpers;

using System.Diagnostics.CodeAnalysis;

/// <summary>A fixed size list-like collection on top of a provided <see cref="Span{T}"/>.</summary>
/// <remarks>Because this list ensures each item is initialized before it is read, this object can safetly be
/// used with uninitialized Spans or arrays for better performance.</remarks>
/// <typeparam name="T">The type of values in the list.</typeparam>
/// <param name="span">The <see cref="Span{T}"/> to wrap.</param>
public ref struct ListSpan<T>(Span<T> span)
{
    /// <summary>Underlying array. Items will be "added" here, starting with the first at the bottom and working up.</summary>
    readonly Span<T> array = span;

    /// <summary>Index of the current end of the list.</summary>
    int endIndex = 0;

    /// <summary>Gets the current number of items in this <see cref="ListSpan{T}"/>.</summary>
    public readonly int Count => endIndex;

    /// <summary>Gets the capacity of the <see cref="ListSpan{T}"/>.</summary>
    public readonly int Capacity => array.Length;

    /// <summary>Indicates if the <see cref="ListSpan{T}"/> is empty.</summary>
    public readonly bool IsEmpty => endIndex == 0;

    #region List like methods

    /// <summary>Removes all items from the <see cref="ListSpan{T}"/>.</summary>
    public void Clear() => endIndex = 0;

    /// <summary>Inserts an item onto the end of the list.</summary>
    /// <param name="value">The item to be added.</param>
    /// <exception cref="InvalidOperationException">If the list is at capacity.</exception>
    public void Add(T value) {
        if (this.endIndex > array.Length) ThrowListFull();

        this.array[this.endIndex++] = value;
    }

    /// <summary>Indicates if there is room on the list for the item, and adds it to the end of the list if so.</summary>
    /// <param name="value">The item to be added.</param>
    /// <returns><see langword="true"/> if there was room in the list for the item; 
    /// <see langword="false"/> if the list is full.</returns>
    public bool TryAdd(T value) {
        if (this.endIndex > array.Length) return false;

        this.array[this.endIndex++] = value;
        return true;
    }

    /// <summary>Removes and returns the item at the end of the list.</summary>
    /// <returns>The item removed from the end of the list.</returns>
    /// <exception cref="InvalidOperationException">If the list is empty.</exception>
    public T Remove() {
        if (this.endIndex <= 0) ThrowListEmpty();

        return this.array[--this.endIndex];
    }

    /// <summary>Indicates if there is an item at the end of the list, and if so removes it and copies it to the 
    /// <paramref name="result"/> parameter.</summary>
    /// <param name="result">If present, the object at the end of the list;
    /// otherwise, the default value of <typeparamref name="T"/>.</param>
    /// <returns><see langword="true"/> if there is an object at the end of the list;
    /// <see langword="false"/> if the list is empty.</returns>
    public bool TryRemove(out T? value) {
        if (this.endIndex <= 0) {
            value = default;
            return false;
        }

        value = this.array[--this.endIndex];
        return true;
    }

    /// <summary>Determines whether an element is in this <see cref="StackSpan{T}"/>.</summary>
    /// <remarks>Searches linearly through the <see cref="StackSpan{T}"/> using <see cref="EqualityComparer{T}.Default"/>
    /// for <typeparamref name="T"/>.</remarks>
    /// <param name="item">The item to locate in this <see cref="StackSpan{T}"/>. May be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the object was found in the <see cref="StackSpan{T}"/>; 
    /// <see langword="false"/> otherwise.</returns>
    public readonly bool Contains(T item) => this.Contains(item, EqualityComparer<T>.Default);

    public readonly bool Contains(T item, IEqualityComparer<T> comparer) {
        for (int loopIndex = 0; loopIndex < this.endIndex; loopIndex++)
            if (comparer.Equals(item, array[loopIndex])) return true;

        return false;
    }
    #endregion

    #region List Like Methods
    /// <summary>Returns a reference to specified element of the list.</summary>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when index less than 0 or index greater than or equal to Length
    /// </exception>
    public readonly ref T this[int index] {
        get {
            if (index >= this.endIndex) ThrowOutOfRange();
            return ref this.array[index];
        }
    }

    /// <summary>Returns the current contents of this list as a <see cref="Span{T}"/>. 
    /// The <see cref="Span{T}"/> will contain only the current content, not the entire capacity.</summary>
    /// <returns>The current items in this list as a <see cref="Span{T}"/>.</returns>
    public readonly Span<T> AsSpan() => array[..endIndex];

    /// <summary>Copies the content of this list to <paramref name="destination"/>. 
    /// Only the current content will be copied, not the entire capacity.</summary>
    /// <remarks>This method copies all of the stack to destination even they overlap.</remarks>
    /// <param name="destination">The <see cref="Span{T}"/> to be copied to.</param>
    /// <exception cref="ArgumentException">If <paramref name="destination"/> does not have sufficent capcity for
    /// all the current items in this list.</exception>
    public readonly void CopyTo(Span<T> destination) => array[..endIndex].CopyTo(destination);

    /// <summary>Attempts to copy the current content of this list to 
    /// <paramref name="destination"/> and indicates whether the copy operation succeeded.
    /// Only the current content will be copied, not the entire capacity.</summary>
    /// <remarks>This method copies all of the stack to destination even they overlap.</remarks>
    /// <param name="destination">The <see cref="Span{T}"/> to be copied to.</param>
    /// <returns><see langword="true"/> if the copy operation succeeded; <see langword="false"/> otherwise.</returns>
    public readonly void TryCopyTo(Span<T> destination) => array[..endIndex].TryCopyTo(destination);

    /// <summary>Copies the current content of this list to a new array of <typeparamref name="T"/>.</summary>
    /// <remarks>This requires a new heap allocation.</remarks>
    /// <returns>A new array of <typeparamref name="T"/> containing the data in this list.</returns>
    public readonly T[] ToArray() => array[..endIndex].ToArray();

    /// <summary>Returns an enumerator for this list.</summary>
    /// <returns>An enumerator for this list.</returns>
    public readonly Span<T>.Enumerator GetEnumerator() => array[endIndex..].GetEnumerator();
    #endregion

    /// <summary>Returns a <see cref="string"/> with the name of the type and it's capacity.</summary>
    public readonly override string ToString() => $"ListSpan<{typeof(T).Name}>[{this.array.Length}]";

    /// <summary>Converts a <see cref="Span{T}"/> to a <see cref="ListSpan{T}"/>.</summary>
    /// <param name="span">The <see cref="Span{T}"/> to convert to a <see cref="ListSpan{T}"/>.</param>
    public static implicit operator ListSpan<T>(Span<T> span) => new(span);

    /// <summary>Converts a <typeparamref name="T"/> array to a <see cref="ListSpan{T}"/>.</summary>
    /// <param name="array">The <see cref="Span{T}"/> to convert to a <see cref="ListSpan{T}"/>.</param>
    public static implicit operator ListSpan<T>(T[] array) => new(array);

    [DoesNotReturn]
    private static void ThrowListEmpty() => throw new InvalidOperationException("List is empty.");
    [DoesNotReturn]
    private static void ThrowListFull() => throw new InvalidOperationException("List is full.");
    [DoesNotReturn]
    private static void ThrowOutOfRange() => throw new IndexOutOfRangeException("Index is out of range.");
}