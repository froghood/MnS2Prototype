namespace Touhou;

public class FixedQueue<T> : Queue<T> {

    public int Capacity { get; }

    public FixedQueue(int capacity) {
        Capacity = capacity;
    }

    public new void Enqueue(T item) {
        base.Enqueue(item);

        while (base.Count > Capacity) {
            base.TryDequeue(out var _);
        }
    }
}