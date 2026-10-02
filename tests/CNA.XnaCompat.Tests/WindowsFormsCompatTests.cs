using System.Windows.Forms;
using Xunit;

namespace CNA.XnaCompat.Tests;

/// <summary>
/// CNA.WindowsFormsCompat, the opt-in System.Windows.Forms subset an XNA game uses on its own window.
/// A game's message box has no Windows Forms to draw it here: it is written to standard error and
/// answers with its first button. The form behind a live game window is an integration test.
/// </summary>
public class WindowsFormsCompatTests
{
    [Theory]
    [InlineData(MessageBoxButtons.OK, DialogResult.OK)]
    [InlineData(MessageBoxButtons.OKCancel, DialogResult.OK)]
    [InlineData(MessageBoxButtons.AbortRetryIgnore, DialogResult.Abort)]
    [InlineData(MessageBoxButtons.YesNoCancel, DialogResult.Yes)]
    [InlineData(MessageBoxButtons.YesNo, DialogResult.Yes)]
    [InlineData(MessageBoxButtons.RetryCancel, DialogResult.Retry)]
    public void MessageBox_AnswersWithItsFirstButton(MessageBoxButtons buttons, DialogResult expected)
    {
        TextWriter original = Console.Error;
        using var captured = new StringWriter();
        Console.SetError(captured);
        try
        {
            Assert.Equal(expected, MessageBox.Show("text", "caption", buttons, MessageBoxIcon.Error));
        }
        finally
        {
            Console.SetError(original);
        }
        Assert.Equal("caption: text" + Environment.NewLine, captured.ToString());
    }

    [Fact]
    public void FromHandle_IsNullForAWindowNoGameOwns() =>
        Assert.Null(Control.FromHandle(new IntPtr(0x1234)));
}
