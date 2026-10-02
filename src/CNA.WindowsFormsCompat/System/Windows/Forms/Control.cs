namespace System.Windows.Forms;

/// <summary>
/// The Windows Forms control model, as far as an XNA game reaches into it: finding the form behind
/// its own window.
/// </summary>
public class Control
{
    private protected Control()
    {
    }

    /// <summary>
    /// The control whose window handle is <paramref name="handle"/>: for an XNA game's
    /// <c>Window.Handle</c> the <see cref="Form"/> it is drawn on, and null for any other handle,
    /// as Windows Forms answers for a window it did not create.
    /// </summary>
    public static Control? FromHandle(IntPtr handle) =>
        Microsoft.Xna.Framework.GameWindow.FromHandle(handle) is { } window ? Form.For(window) : null;
}
