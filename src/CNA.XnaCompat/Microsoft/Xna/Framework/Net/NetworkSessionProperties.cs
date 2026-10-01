using System.Collections;
using CNA.Interop;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net;

/// <summary>
/// XNA's eight optional session properties. A standalone list is plain managed state, as XNA's is.
/// A session's own list reads the session's current values and writes through the session -- the
/// host check and the propagation to the other machines are native's -- and a discovered session's
/// list refuses writes, as XNA's read-only copy does.
/// </summary>
public class NetworkSessionProperties : IList<int?>
{
    private const int PropertyCount = 8;

    private readonly int?[] _data = new int?[PropertyCount];
    private Action<int, int?>? _propertyChanging;
    private Func<int?[]>? _liveValues;

    public NetworkSessionProperties()
    {
    }

    public int? this[int index]
    {
        get
        {
            ThrowIfOutOfRange(index);
            Refresh();
            return _data[index];
        }
        set
        {
            ThrowIfOutOfRange(index);
            _propertyChanging?.Invoke(index, value);
            _data[index] = value;
        }
    }

    public int Count => PropertyCount;

    public IEnumerator<int?> GetEnumerator()
    {
        Refresh();
        return ((IList<int?>)_data).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        Refresh();
        return _data.GetEnumerator();
    }

    int IList<int?>.IndexOf(int? item)
    {
        Refresh();
        return ((IList<int?>)_data).IndexOf(item);
    }

    bool ICollection<int?>.Contains(int? item)
    {
        Refresh();
        return ((IList<int?>)_data).Contains(item);
    }

    void ICollection<int?>.CopyTo(int?[] array, int arrayIndex)
    {
        Refresh();
        _data.CopyTo(array, arrayIndex);
    }

    bool ICollection<int?>.IsReadOnly => false;

    void ICollection<int?>.Add(int? item) => throw new NotSupportedException();

    void IList<int?>.Insert(int index, int? item) => throw new NotSupportedException();

    bool ICollection<int?>.Remove(int? item) => throw new NotSupportedException();

    void IList<int?>.RemoveAt(int index) => throw new NotSupportedException();

    void ICollection<int?>.Clear() => throw new NotSupportedException();

    /// <summary>A snapshot that refuses writes: a discovered session's properties.</summary>
    internal static NetworkSessionProperties CreateReadOnly(int?[] values)
    {
        var properties = new NetworkSessionProperties();
        values.CopyTo(properties._data, 0);
        properties._propertyChanging = static (_, _) =>
            throw new NotSupportedException("The properties of a discovered session are read-only.");
        return properties;
    }

    /// <summary>The live list of a session: reads come from it, writes go through it.</summary>
    internal static NetworkSessionProperties CreateLive(Func<int?[]> read, Action<int, int?> write) =>
        new() { _liveValues = read, _propertyChanging = write };

    internal static unsafe int?[] ReadNative(CnaHandle properties)
    {
        var values = new int?[PropertyCount];
        for (int index = 0; index < PropertyCount; index++)
        {
            CnaOptionalInt32 value = default;
            GamerServicesInterop.Check(
                Native.cna_network_session_properties_get_item(properties, index, ref value), nameof(NetworkSessionProperties));
            values[index] = value.HasValue != 0 ? value.Value : null;
        }

        return values;
    }

    internal static CnaOptionalInt32 ToNative(int? value) =>
        new() { HasValue = GamerServicesInterop.Bool(value.HasValue), Value = value.GetValueOrDefault() };

    /// <summary>A new native list holding these values; the caller destroys it.</summary>
    internal CnaHandle CreateNative()
    {
        Refresh();
        GamerServicesInterop.Check(
            Native.cna_network_session_properties_create(out CnaHandle properties), nameof(NetworkSessionProperties));
        try
        {
            for (int index = 0; index < PropertyCount; index++)
            {
                GamerServicesInterop.Check(
                    Native.cna_network_session_properties_set_item(properties, index, ToNative(_data[index])),
                    nameof(NetworkSessionProperties));
            }
        }
        catch
        {
            _ = Native.cna_network_session_properties_destroy(properties);
            throw;
        }

        return properties;
    }

    private void Refresh()
    {
        if (_liveValues is { } read)
        {
            read().CopyTo(_data, 0);
        }
    }

    private static void ThrowIfOutOfRange(int index)
    {
        if (index is < 0 or >= PropertyCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
