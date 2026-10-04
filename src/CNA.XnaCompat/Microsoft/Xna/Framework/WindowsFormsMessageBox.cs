namespace Microsoft.Xna.Framework;

/// <summary>The internal bridge from CNA.WindowsFormsCompat to CNA's native dialog service.</summary>
internal static class WindowsFormsMessageBox
{
    internal static bool TryShow(
        uint type,
        string title,
        string message,
        string[] buttonLabels,
        out int chosen) =>
        CNA.NativeMessageBox.TryShow(type, title, message, buttonLabels, out chosen);
}
