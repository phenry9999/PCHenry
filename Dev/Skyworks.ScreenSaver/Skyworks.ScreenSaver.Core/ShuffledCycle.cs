using System.Diagnostics.CodeAnalysis;

namespace Skyworks.ScreenSaver.Core;

public sealed class ShuffledCycle<T>
{
    private readonly Random random;
    private readonly IEqualityComparer<T> comparer;
    private T[] items = [];
    private T[] order = [];
    private int position;

    public ShuffledCycle(Random? random = null, IEqualityComparer<T>? comparer = null)
    {
        this.random = random ?? Random.Shared;
        this.comparer = comparer ?? EqualityComparer<T>.Default;
    }

    public int Count => items.Length;
    public IReadOnlyList<T> Items => Array.AsReadOnly(items);

    public void Replace(IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        items = source.Distinct(comparer).ToArray();
        Shuffle();
    }

    public bool TryNext([MaybeNullWhen(false)] out T item)
    {
        if (items.Length == 0) { item = default; return false; }
        if (position == order.Length) Shuffle();
        item = order[position++];
        return true;
    }

    public void Remove(T item)
    {
        items = items.Where(value => !comparer.Equals(value, item)).ToArray();
        // Preserve the unplayed part of this cycle; a broken image must not cause early repeats.
        order = order.Skip(position).Where(value => !comparer.Equals(value, item)).ToArray();
        position = 0;
    }

    private void Shuffle()
    {
        order = (T[])items.Clone();
        for (int i = order.Length - 1; i > 0; i--)
        {
            int other = random.Next(i + 1);
            (order[i], order[other]) = (order[other], order[i]);
        }
        position = 0;
    }
}
