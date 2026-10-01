using System.Collections;
using CNA;
using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>
/// XNA's typed property bag (leaderboard columns), over a native dictionary whose slots carry their
/// own type. The object indexer boxes whatever type the slot holds and stores by the runtime type
/// of the value it is given, which is what XNA's does.
/// </summary>
public sealed class PropertyDictionary : IDictionary<string, object>
{
    private const uint KindUnknown = 0;
    private const uint KindDateTime = 1;
    private const uint KindDouble = 2;
    private const uint KindInt32 = 3;
    private const uint KindInt64 = 4;
    private const uint KindOutcome = 5;
    private const uint KindSingle = 6;
    private const uint KindStream = 7;
    private const uint KindString = 8;
    private const uint KindTimeSpan = 9;

    private readonly NativeResourceHandle _handle;

    internal PropertyDictionary(CnaHandle handle)
    {
        _handle = new NativeResourceHandle(
            handle.Value, value => Native.cna_property_dictionary_destroy(new CnaHandle(value)).IsSuccess());
    }

    private CnaHandle Handle => new(_handle.DangerousGetHandle());

    public int GetValueInt32(string key) => Get(key, (h, k) => (Native.cna_property_dictionary_get_int32(h, k, out int v), v));

    public long GetValueInt64(string key) => Get(key, (h, k) => (Native.cna_property_dictionary_get_int64(h, k, out long v), v));

    public float GetValueSingle(string key) => Get(key, (h, k) => (Native.cna_property_dictionary_get_single(h, k, out float v), v));

    public double GetValueDouble(string key) => Get(key, (h, k) => (Native.cna_property_dictionary_get_double(h, k, out double v), v));

    public unsafe string GetValueString(string key)
    {
        ulong size = Get(key, (h, k) => (Native.cna_property_dictionary_get_string_size(h, k, out ulong v), v));
        byte[] buffer = new byte[size];
        ulong written = Get(key, (h, k) =>
        {
            fixed (byte* pointer = buffer)
            {
                return (Native.cna_property_dictionary_copy_string(h, k, pointer, size, out ulong v), v);
            }
        });
        return System.Text.Encoding.UTF8.GetString(buffer, 0, (int)written);
    }

    public LeaderboardOutcome GetValueOutcome(string key) =>
        (LeaderboardOutcome)Get(key, (h, k) => (Native.cna_property_dictionary_get_outcome(h, k, out uint v), v));

    public DateTime GetValueDateTime(string key) =>
        new(Get(key, (h, k) => (Native.cna_property_dictionary_get_date_time_ticks(h, k, out long v), v)));

    public TimeSpan GetValueTimeSpan(string key) =>
        new(Get(key, (h, k) => (Native.cna_property_dictionary_get_time_span_ticks(h, k, out long v), v)));

    /// <summary>
    /// Null for an empty stream slot. A stream's bytes do not cross CNA's C ABI -- only whether it is
    /// there and how long it is -- so one that holds data cannot be returned yet.
    /// </summary>
    public Stream GetValueStream(string key)
    {
        (byte has, ulong _) = Get(key, (h, k) => (Native.cna_property_dictionary_get_stream_size_ext(h, k, out byte has, out ulong bytes), (has, bytes)));
        if (has == 0)
        {
            return null!;
        }

        throw new NotSupportedException("CNA's C ABI does not expose the contents of a stream property.");
    }

    public void SetValue(string key, int value) => Set(key, (h, k) => Native.cna_property_dictionary_set_int32(h, k, value));

    public void SetValue(string key, long value) => Set(key, (h, k) => Native.cna_property_dictionary_set_int64(h, k, value));

    public void SetValue(string key, float value) => Set(key, (h, k) => Native.cna_property_dictionary_set_single(h, k, value));

    public void SetValue(string key, double value) => Set(key, (h, k) => Native.cna_property_dictionary_set_double(h, k, value));

    public void SetValue(string key, string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Set(key, (h, k) => GamerServicesInterop.WithString(value, v => Native.cna_property_dictionary_set_string(h, k, v)));
    }

    public void SetValue(string key, LeaderboardOutcome value) =>
        Set(key, (h, k) => Native.cna_property_dictionary_set_outcome(h, k, (uint)value));

    public void SetValue(string key, DateTime value) =>
        Set(key, (h, k) => Native.cna_property_dictionary_set_date_time_ticks(h, k, value.Ticks));

    public void SetValue(string key, TimeSpan value) =>
        Set(key, (h, k) => Native.cna_property_dictionary_set_time_span_ticks(h, k, value.Ticks));

    public int Count
    {
        get
        {
            GamerServicesInterop.Check(Native.cna_property_dictionary_get_count(Handle, out int count), nameof(Count));
            return count;
        }
    }

    public bool ContainsKey(string key) =>
        Get(key, (h, k) => (Native.cna_property_dictionary_contains_key(h, k, out byte v), v)) != 0;

    public object this[string key]
    {
        get => TryGetValue(key, out object value) ? value : throw new KeyNotFoundException(key);
        set
        {
            switch (value)
            {
                case int v: SetValue(key, v); break;
                case long v: SetValue(key, v); break;
                case float v: SetValue(key, v); break;
                case double v: SetValue(key, v); break;
                case string v: SetValue(key, v); break;
                case LeaderboardOutcome v: SetValue(key, v); break;
                case DateTime v: SetValue(key, v); break;
                case TimeSpan v: SetValue(key, v); break;
                default:
                    throw new ArgumentException(
                        $"A property value of type {value?.GetType().Name ?? "null"} cannot be stored.", nameof(value));
            }
        }
    }

    public bool TryGetValue(string key, out object value)
    {
        (byte found, uint kind) = Get(key, (h, k) =>
            (Native.cna_property_dictionary_try_get_value_kind_ext(h, k, out byte found, out uint kind), (found, kind)));
        if (found == 0)
        {
            value = null!;
            return false;
        }

        value = kind switch
        {
            KindDateTime => GetValueDateTime(key),
            KindDouble => GetValueDouble(key),
            KindInt32 => GetValueInt32(key),
            KindInt64 => GetValueInt64(key),
            KindOutcome => GetValueOutcome(key),
            KindSingle => GetValueSingle(key),
            KindStream => GetValueStream(key),
            KindString => GetValueString(key),
            KindTimeSpan => GetValueTimeSpan(key),
            KindUnknown or _ => null!,
        };
        return true;
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        foreach (string key in KeysSnapshot())
        {
            yield return new KeyValuePair<string, object>(key, this[key]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    ICollection<string> IDictionary<string, object>.Keys => KeysSnapshot();

    ICollection<object> IDictionary<string, object>.Values => KeysSnapshot().Select(key => this[key]).ToList();

    bool ICollection<KeyValuePair<string, object>>.IsReadOnly
    {
        get
        {
            GamerServicesInterop.Check(Native.cna_property_dictionary_get_is_read_only(Handle, out byte value), "IsReadOnly");
            return value != 0;
        }
    }

    void IDictionary<string, object>.Add(string key, object value)
    {
        if (ContainsKey(key))
        {
            throw new ArgumentException("An element with the same key already exists.", nameof(key));
        }

        this[key] = value;
    }

    bool IDictionary<string, object>.Remove(string key) =>
        Get(key, (h, k) => (Native.cna_property_dictionary_remove(h, k, out byte removed), removed)) != 0;

    void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) =>
        ((IDictionary<string, object>)this).Add(item.Key, item.Value);

    void ICollection<KeyValuePair<string, object>>.Clear() =>
        GamerServicesInterop.Check(Native.cna_property_dictionary_clear(Handle), "Clear");

    bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) =>
        TryGetValue(item.Key, out object value) && Equals(value, item.Value);

    void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) =>
        this.ToList().CopyTo(array, arrayIndex);

    bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) =>
        ((ICollection<KeyValuePair<string, object>>)this).Contains(item) && ((IDictionary<string, object>)this).Remove(item.Key);

    private unsafe List<string> KeysSnapshot()
    {
        int count = Count;
        var keys = new List<string>(count);
        for (int index = 0; index < count; index++)
        {
            GamerServicesInterop.Check(Native.cna_property_dictionary_get_key_size_at(Handle, index, out ulong size), "Keys");
            byte[] buffer = new byte[size];
            fixed (byte* pointer = buffer)
            {
                GamerServicesInterop.Check(
                    Native.cna_property_dictionary_copy_key_at(Handle, index, pointer, size, out ulong written), "Keys");
                keys.Add(System.Text.Encoding.UTF8.GetString(buffer, 0, (int)written));
            }
        }

        return keys;
    }

    private T Get<T>(string key, Func<CnaHandle, CnaStringView, (CnaResult Result, T Value)> call)
    {
        ArgumentNullException.ThrowIfNull(key);
        (CnaResult Result, T Value) outcome = default;
        GamerServicesInterop.Check(
            GamerServicesInterop.WithString(key, view =>
            {
                outcome = call(Handle, view);
                return outcome.Result;
            }),
            key);
        return outcome.Value;
    }

    private void Set(string key, Func<CnaHandle, CnaStringView, CnaResult> call)
    {
        ArgumentNullException.ThrowIfNull(key);
        GamerServicesInterop.Check(GamerServicesInterop.WithString(key, view => call(Handle, view)), key);
    }
}
