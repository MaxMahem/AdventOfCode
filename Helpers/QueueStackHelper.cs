namespace AdventOfCode.Helpers;

public static class QueueStackHelper {
    public static void Add<T>(this Queue<T> queue, T item) => queue.Enqueue(item);
    public static void Add<T>(this Stack<T> stack, T item) => stack.Push(item);
}
