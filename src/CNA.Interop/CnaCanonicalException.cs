using System.Text;

namespace CNA.Interop;

/// <summary>
/// The canonical exception behind the calling thread's last native failure (ABI 0.37.0,
/// CBIND-132): CNA names the .NET type the canonical call threw, and an argument exception's
/// parameter name, so a facade can throw what XNA throws rather than a category's nearest guess.
/// The <c>System</c> types are built here, where every facade can reach them; a facade adds its own
/// (XNA's, the phone's) on top.
/// </summary>
internal static unsafe class CnaCanonicalException
{
    /// <summary>The canonical type name, or empty when the C layer raised the failure itself.</summary>
    internal static string LastType() =>
        Read(Native.cna_error_get_last_exception_type_size_ext, CopyType);

    internal static string LastParamName() =>
        Read(Native.cna_error_get_last_exception_param_name_size_ext, CopyParamName);

    /// <summary>The <c>System</c> exception <paramref name="type"/> names, or null for any other.</summary>
    internal static Exception? CreateSystem(string type, string paramName, string message)
    {
        string? param = paramName.Length == 0 ? null : paramName;
        return type switch
        {
            "System.ArgumentNullException" => new ArgumentNullException(param, message),
            "System.ArgumentOutOfRangeException" => new ArgumentOutOfRangeException(param, message),
            "System.ArgumentException" => new ArgumentException(message, param),
            "System.InvalidOperationException" => new InvalidOperationException(message),
            "System.ObjectDisposedException" => new ObjectDisposedException(null, message),
            "System.NotSupportedException" => new NotSupportedException(message),
            "System.NotImplementedException" => new NotImplementedException(message),
            "System.InvalidCastException" => new InvalidCastException(message),
            "System.FormatException" => new FormatException(message),
            "System.IndexOutOfRangeException" => new IndexOutOfRangeException(message),
            "System.Collections.Generic.KeyNotFoundException" => new KeyNotFoundException(message),
            "System.OverflowException" => new OverflowException(message),
            "System.DivideByZeroException" => new DivideByZeroException(message),
            "System.IO.IOException" => new IOException(message),
            "System.IO.EndOfStreamException" => new EndOfStreamException(message),
            "System.IO.FileNotFoundException" => new FileNotFoundException(message),
            "System.IO.DirectoryNotFoundException" => new DirectoryNotFoundException(message),
            "System.UnauthorizedAccessException" => new UnauthorizedAccessException(message),
            _ => null,
        };
    }

    private delegate CnaResult SizeQuery(out ulong bytes);

    private unsafe delegate CnaResult CopyQuery(byte* destination, ulong capacity, out ulong bytes);

    /// <summary>Best effort, never throwing: this runs while a failure is already being reported.</summary>
    private static unsafe string Read(SizeQuery size, CopyQuery copy)
    {
        if (size(out ulong length).IsFailure() || length == 0)
        {
            return string.Empty;
        }

        byte[] buffer = new byte[length];
        fixed (byte* pointer = buffer)
        {
            return copy(pointer, length, out ulong written).IsFailure()
                ? string.Empty
                : Encoding.UTF8.GetString(buffer, 0, (int)written);
        }
    }

    private static unsafe CnaResult CopyType(byte* destination, ulong capacity, out ulong bytes) =>
        Native.cna_error_copy_last_exception_type_ext(destination, capacity, out bytes);

    private static unsafe CnaResult CopyParamName(byte* destination, ulong capacity, out ulong bytes) =>
        Native.cna_error_copy_last_exception_param_name_ext(destination, capacity, out bytes);
}
