using System.Collections;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>XNA's read-only gamer list, with its allocation-free struct enumerator.</summary>
public class GamerCollection<T> : ReadOnlyCollection<T>, IEnumerable<Gamer>
    where T : Gamer
{
    internal GamerCollection(IList<T> list)
        : base(list)
    {
    }

    public new GamerCollectionEnumerator GetEnumerator() => new(this);

    IEnumerator<Gamer> IEnumerable<Gamer>.GetEnumerator() => GetEnumerator();

    public struct GamerCollectionEnumerator : IEnumerator<T>
    {
        private readonly GamerCollection<T> _collection;
        private int _index;

        internal GamerCollectionEnumerator(GamerCollection<T> collection)
        {
            _collection = collection;
            _index = -1;
        }

        public T Current => _collection[_index];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext() => ++_index < _collection.Count;

        void IEnumerator.Reset() => _index = -1;
    }
}
