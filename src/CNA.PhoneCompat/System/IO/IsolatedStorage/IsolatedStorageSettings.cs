using System.Collections;
using System.Runtime.Serialization;
using System.Text;

namespace System.IO.IsolatedStorage;

/// <summary>
/// Windows Phone's application settings: a dictionary kept in the application's isolated store
/// (System.Windows on the phone, cna-cs CSX-132). As there, values are written with the
/// DataContract serializer, the types of the values listed first so they can be read back, to the
/// store's <c>__ApplicationSettings</c> file -- on <see cref="Save"/>, and when the application
/// ends, which is when the phone saved them unasked. The store is .NET's
/// <see cref="IsolatedStorageFile.GetUserStoreForApplication"/>, where a phone game's own files go.
/// </summary>
public sealed class IsolatedStorageSettings :
    IDictionary<string, object>, IDictionary, ICollection<KeyValuePair<string, object>>, ICollection,
    IEnumerable<KeyValuePair<string, object>>, IEnumerable
{
    private const string FileName = "__ApplicationSettings";
    private static readonly Lazy<IsolatedStorageSettings> Application = new(() => new IsolatedStorageSettings());
    private readonly Dictionary<string, object> _settings;

    private IsolatedStorageSettings()
    {
        _settings = Load();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => Save();
    }

    /// <summary>The settings of the running application.</summary>
    public static IsolatedStorageSettings ApplicationSettings => Application.Value;

    public int Count => _settings.Count;

    public ICollection Keys => _settings.Keys;

    public ICollection Values => _settings.Values;

    public object this[string key]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(key);
            return _settings[key];
        }
        set
        {
            ArgumentNullException.ThrowIfNull(key);
            _settings[key] = value;
        }
    }

    public void Add(string key, object value)
    {
        ArgumentNullException.ThrowIfNull(key);
        _settings.Add(key, value);
    }

    public void Clear() => _settings.Clear();

    public bool Contains(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _settings.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _settings.Remove(key);
    }

    /// <summary>A value of the type the caller names: an unboxing cast, so a value of another type
    /// throws <see cref="InvalidCastException"/> as on the phone.</summary>
    public bool TryGetValue<T>(string key, out T value)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (_settings.TryGetValue(key, out object? stored))
        {
            value = (T)stored;
            return true;
        }

        value = default!;
        return false;
    }

    public void Save()
    {
        using IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
        using IsolatedStorageFileStream stream = store.OpenFile(FileName, FileMode.Create, FileAccess.Write);
        Type[] known = _settings.Values.Where(value => value is not null).Select(value => value.GetType()).Distinct().ToArray();
        byte[] header = Encoding.UTF8.GetBytes(string.Join('\0', known.Select(type => type.AssemblyQualifiedName)) + "\n");
        stream.Write(header);
        new DataContractSerializer(typeof(Dictionary<string, object>), known).WriteObject(stream, _settings);
    }

    internal static Dictionary<string, object> Load()
    {
        try
        {
            using IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            if (!store.FileExists(FileName))
            {
                return [];
            }

            using IsolatedStorageFileStream stream = store.OpenFile(FileName, FileMode.Open, FileAccess.Read);
            var header = new List<byte>();
            for (int b = stream.ReadByte(); b is not -1 and not '\n'; b = stream.ReadByte())
            {
                header.Add((byte)b);
            }

            Type[] known = Encoding.UTF8.GetString(header.ToArray())
                .Split('\0', StringSplitOptions.RemoveEmptyEntries)
                .Select(name => Type.GetType(name, throwOnError: false))
                .OfType<Type>()
                .ToArray();
            return (Dictionary<string, object>?)new DataContractSerializer(typeof(Dictionary<string, object>), known)
                .ReadObject(stream) ?? [];
        }
        catch (Exception exception) when (exception is IOException or SerializationException or IsolatedStorageException)
        {
            // A store the settings cannot be read from starts them afresh, as the phone did.
            return [];
        }
    }

    ICollection<string> IDictionary<string, object>.Keys => _settings.Keys;

    ICollection<object> IDictionary<string, object>.Values => _settings.Values;

    bool IDictionary<string, object>.ContainsKey(string key) => Contains(key);

    bool IDictionary<string, object>.TryGetValue(string key, out object value) => TryGetValue(key, out value);

    bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;

    void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) => Add(item.Key, item.Value);

    bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) =>
        ((ICollection<KeyValuePair<string, object>>)_settings).Contains(item);

    void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) =>
        ((ICollection<KeyValuePair<string, object>>)_settings).CopyTo(array, arrayIndex);

    bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) =>
        ((ICollection<KeyValuePair<string, object>>)_settings).Remove(item);

    IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => _settings.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _settings.GetEnumerator();

    bool IDictionary.IsFixedSize => false;

    bool IDictionary.IsReadOnly => false;

    object? IDictionary.this[object key]
    {
        get => key is string name && _settings.TryGetValue(name, out object? value) ? value : null;
        set => this[(string)key] = value!;
    }

    void IDictionary.Add(object key, object? value) => Add((string)key, value!);

    bool IDictionary.Contains(object key) => key is string name && Contains(name);

    IDictionaryEnumerator IDictionary.GetEnumerator() => _settings.GetEnumerator();

    void IDictionary.Remove(object key)
    {
        if (key is string name)
        {
            Remove(name);
        }
    }

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    void ICollection.CopyTo(Array array, int index) => ((ICollection)_settings).CopyTo(array, index);
}
