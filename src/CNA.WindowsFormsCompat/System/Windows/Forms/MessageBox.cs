namespace System.Windows.Forms;

/// <summary>
/// Windows Forms' message box. XNA games show one when they stop on an error. There is no
/// Windows Forms here to draw it, so the caption and text go to standard error, where the error can
/// still be read, and the box answers with its first button, as Enter would.
/// </summary>
public static class MessageBox
{
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
        Console.Error.WriteLine(string.IsNullOrEmpty(caption) ? text : caption + ": " + text);
        return buttons switch
        {
            MessageBoxButtons.OK or MessageBoxButtons.OKCancel => DialogResult.OK,
            MessageBoxButtons.AbortRetryIgnore => DialogResult.Abort,
            MessageBoxButtons.YesNoCancel or MessageBoxButtons.YesNo => DialogResult.Yes,
            MessageBoxButtons.RetryCancel => DialogResult.Retry,
            _ => throw new ComponentModel.InvalidEnumArgumentException(nameof(buttons), (int)buttons, typeof(MessageBoxButtons)),
        };
    }
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
