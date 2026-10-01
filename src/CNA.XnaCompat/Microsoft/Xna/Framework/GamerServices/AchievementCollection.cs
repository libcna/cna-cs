using System.Collections;
using CNA;
using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>A gamer's achievements as XNA hands them out: read-only, indexed by position and key.</summary>
public sealed class AchievementCollection : IList<Achievement>, IDisposable
{
    private readonly NativeResourceHandle _handle;
    private readonly List<Achievement> _items;

    internal AchievementCollection(CnaHandle handle)
    {
        _handle = new NativeResourceHandle(
            handle.AsNint, value => Native.cna_achievement_collection_destroy(new CnaHandle(value)).IsSuccess());
        GamerServicesInterop.Check(Native.cna_achievement_collection_get_count(handle, out int count), nameof(AchievementCollection));
        _items = new List<Achievement>(count);
        for (int index = 0; index < count; index++)
        {
            GamerServicesInterop.Check(
                Native.cna_achievement_collection_get_at(handle, index, out CnaHandle achievement), nameof(AchievementCollection));
            _items.Add(new Achievement(achievement));
        }
    }

    public bool IsDisposed => _handle.IsClosed;

    public Achievement this[int index] => _items[index];

    /// <summary>The achievement with that key; <see cref="KeyNotFoundException"/> when there is none.</summary>
    public Achievement this[string achievementKey]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(achievementKey);
            foreach (Achievement achievement in _items)
            {
                if (achievement.Key == achievementKey)
                {
                    return achievement;
                }
            }

            throw new KeyNotFoundException(achievementKey);
        }
    }

    Achievement IList<Achievement>.this[int index]
    {
        get => _items[index];
        set => throw ReadOnly();
    }

    public int Count => _items.Count;

    bool ICollection<Achievement>.IsReadOnly => true;

    public IEnumerator<Achievement> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    int IList<Achievement>.IndexOf(Achievement item) => _items.IndexOf(item);

    bool ICollection<Achievement>.Contains(Achievement item) => _items.Contains(item);

    void ICollection<Achievement>.CopyTo(Achievement[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    void IList<Achievement>.Insert(int index, Achievement item) => throw ReadOnly();

    void IList<Achievement>.RemoveAt(int index) => throw ReadOnly();

    void ICollection<Achievement>.Add(Achievement item) => throw ReadOnly();

    void ICollection<Achievement>.Clear() => throw ReadOnly();

    bool ICollection<Achievement>.Remove(Achievement item) => throw ReadOnly();

    public void Dispose()
    {
        _handle.Dispose();
    }

    private static NotSupportedException ReadOnly() => new("The achievement collection is read-only.");
}
