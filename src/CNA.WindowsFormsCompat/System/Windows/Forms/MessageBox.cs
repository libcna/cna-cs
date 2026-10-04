namespace System.Windows.Forms;

/// <summary>
/// Windows Forms' message box. XNA games commonly show one when they stop on an error. CNA uses the
/// platform's native dialog service while a game is alive. If the platform has no dialog service,
/// or the game failed before its native runtime existed, the message is written to standard error
/// and the first button is returned so the original failure remains visible.
/// </summary>
public static class MessageBox
{
    internal delegate bool NativeShow(
        uint type, string title, string message, string[] buttonLabels, out int chosen);

    /// <summary>Shows <paramref name="text"/> and returns the default button's result.</summary>
    public static DialogResult Show(string text) => Show(text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None);

    /// <summary>Shows <paramref name="text"/> under <paramref name="caption"/> and returns the default button's result.</summary>
    public static DialogResult Show(string text, string caption) =>
        Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);

    /// <summary>Shows a message with the given buttons and returns the default button's result.</summary>
    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons) =>
        Show(text, caption, buttons, MessageBoxIcon.None);

    /// <summary>Shows a message with the given buttons and icon and returns the default button's result.</summary>
    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        return ShowCore(
            text ?? string.Empty,
            caption ?? string.Empty,
            buttons,
            icon,
            Microsoft.Xna.Framework.WindowsFormsMessageBox.TryShow,
            Console.Error.WriteLine);
    }

    internal static DialogResult ShowCore(
        string text,
        string caption,
        MessageBoxButtons buttons,
        MessageBoxIcon icon,
        NativeShow nativeShow,
        Action<string> fallback)
    {
        (string[] labels, DialogResult[] results) = Buttons(buttons);
        uint type = Severity(icon);

        if (nativeShow(type, caption, text, labels, out int chosen))
        {
            if (chosen >= 0 && chosen < results.Length)
            {
                return results[chosen];
            }

            int cancel = Array.IndexOf(results, DialogResult.Cancel);
            return cancel >= 0 ? DialogResult.Cancel : results[0];
        }

        fallback(string.IsNullOrEmpty(caption) ? text : caption + ": " + text);
        return results[0];
    }

    private static (string[] Labels, DialogResult[] Results) Buttons(MessageBoxButtons buttons) =>
        buttons switch
        {
            MessageBoxButtons.OK => (["OK"], [DialogResult.OK]),
            MessageBoxButtons.OKCancel => (["OK", "Cancel"], [DialogResult.OK, DialogResult.Cancel]),
            MessageBoxButtons.AbortRetryIgnore =>
                (["Abort", "Retry", "Ignore"], [DialogResult.Abort, DialogResult.Retry, DialogResult.Ignore]),
            MessageBoxButtons.YesNoCancel =>
                (["Yes", "No", "Cancel"], [DialogResult.Yes, DialogResult.No, DialogResult.Cancel]),
            MessageBoxButtons.YesNo => (["Yes", "No"], [DialogResult.Yes, DialogResult.No]),
            MessageBoxButtons.RetryCancel =>
                (["Retry", "Cancel"], [DialogResult.Retry, DialogResult.Cancel]),
            _ => throw new ComponentModel.InvalidEnumArgumentException(nameof(buttons), (int)buttons, typeof(MessageBoxButtons)),
        };

    private static uint Severity(MessageBoxIcon icon) => (int)icon switch
    {
        0 or 32 or 64 => 2,
        16 => 0,
        48 => 1,
        _ => throw new ComponentModel.InvalidEnumArgumentException(nameof(icon), (int)icon, typeof(MessageBoxIcon)),
    };
}

/// <summary>A message box's buttons, with Windows Forms' values.</summary>
public enum MessageBoxButtons
{
    OK = 0,
    OKCancel = 1,
    AbortRetryIgnore = 2,
    YesNoCancel = 3,
    YesNo = 4,
    RetryCancel = 5,
}

/// <summary>A message box's icon, with Windows Forms' values.</summary>
public enum MessageBoxIcon
{
    None = 0,
    Hand = 16,
    Stop = 16,
    Error = 16,
    Question = 32,
    Exclamation = 48,
    Warning = 48,
    Asterisk = 64,
    Information = 64,
}

/// <summary>A dialog's result, with Windows Forms' values.</summary>
public enum DialogResult
{
    None = 0,
    OK = 1,
    Cancel = 2,
    Abort = 3,
    Retry = 4,
    Ignore = 5,
    Yes = 6,
    No = 7,
}
