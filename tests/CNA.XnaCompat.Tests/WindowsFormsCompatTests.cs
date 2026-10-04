using System.Windows.Forms;
using Xunit;

namespace CNA.XnaCompat.Tests;

/// <summary>
/// CNA.WindowsFormsCompat, the opt-in System.Windows.Forms subset an XNA game uses on its own window.
/// Native dialogs are isolated behind a delegate here; the form behind a live game window is an
/// integration test.
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
    public void MessageBox_FallbackWritesTheErrorAndAnswersWithItsFirstButton(
        MessageBoxButtons buttons, DialogResult expected)
    {
        string? written = null;
        Assert.Equal(expected, MessageBox.ShowCore(
            "text", "caption", buttons, MessageBoxIcon.Error,
            NoNativeDialog,
            value => written = value));
        Assert.Equal("caption: text", written);
    }

    [Theory]
    [InlineData(MessageBoxButtons.OKCancel, 1, DialogResult.Cancel)]
    [InlineData(MessageBoxButtons.AbortRetryIgnore, 2, DialogResult.Ignore)]
    [InlineData(MessageBoxButtons.YesNoCancel, 1, DialogResult.No)]
    [InlineData(MessageBoxButtons.RetryCancel, 0, DialogResult.Retry)]
    public void MessageBox_ReturnsTheButtonChosenInTheNativeDialog(
        MessageBoxButtons buttons, int chosen, DialogResult expected)
    {
        uint seenType = uint.MaxValue;
        string[]? seenLabels = null;
        bool TryShow(uint type, string title, string message, string[] labels, out int answer)
        {
            seenType = type;
            seenLabels = labels;
            answer = chosen;
            return true;
        }

        Assert.Equal(expected, MessageBox.ShowCore(
            "body", "title", buttons, MessageBoxIcon.Warning, TryShow,
            _ => throw new Xunit.Sdk.XunitException("The fallback must not run.")));
        Assert.Equal(1u, seenType);
        Assert.NotNull(seenLabels);
        Assert.Equal(buttons switch
        {
            MessageBoxButtons.OKCancel => ["OK", "Cancel"],
            MessageBoxButtons.AbortRetryIgnore => ["Abort", "Retry", "Ignore"],
            MessageBoxButtons.YesNoCancel => ["Yes", "No", "Cancel"],
            MessageBoxButtons.RetryCancel => ["Retry", "Cancel"],
            _ => throw new Xunit.Sdk.XunitException("Unexpected theory value."),
        }, seenLabels);
    }

    [Fact]
    public void MessageBox_ClosingANativeCancelableDialogMeansCancel()
    {
        bool Close(uint type, string title, string message, string[] labels, out int answer)
        {
            answer = -1;
            return true;
        }

        Assert.Equal(DialogResult.Cancel, MessageBox.ShowCore(
            "body", "title", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, Close, _ => { }));
    }

    [Fact]
    public void MessageBox_RejectsUndefinedButtonsBeforeTryingThePlatform()
    {
        bool Unexpected(uint type, string title, string message, string[] labels, out int answer)
        {
            answer = 0;
            return true;
        }

        Assert.Throws<System.ComponentModel.InvalidEnumArgumentException>(() => MessageBox.ShowCore(
            "body", "title", (MessageBoxButtons)99, MessageBoxIcon.None,
            Unexpected, _ => { }));
    }

    [Fact]
    public void FromHandle_IsNullForAWindowNoGameOwns() =>
        Assert.Null(Control.FromHandle(new IntPtr(0x1234)));

    private static bool NoNativeDialog(
        uint type, string title, string message, string[] labels, out int chosen)
    {
        chosen = -1;
        return false;
    }
}
