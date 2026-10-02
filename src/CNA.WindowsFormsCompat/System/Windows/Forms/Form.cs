using System.Runtime.CompilerServices;

namespace System.Windows.Forms;

/// <summary>
/// The form an XNA game is drawn on, reached through <see cref="Control.FromHandle"/>.
///
/// Its appearance properties are kept and read back but not applied: CNA's platform layer offers no
/// window opacity, and Windows Forms itself leaves <see cref="Opacity"/> without effect where layered
/// windows are unavailable. A game that hides its form while it loads (the Racing Game Kit) therefore
/// shows its loading screen instead.
/// </summary>
public class Form : Control
{
    private static readonly ConditionalWeakTable<Microsoft.Xna.Framework.GameWindow, Form> Forms = new();

    private double _opacity = 1.0;

    private Form()
    {
    }

    internal static Form For(Microsoft.Xna.Framework.GameWindow window) =>
        Forms.GetValue(window, static _ => new Form());

    /// <summary>The form's opacity, from 0.0 (transparent) to 1.0; out-of-range values are clamped.</summary>
    public double Opacity
    {
        get => _opacity;
        set => _opacity = value < 0.0 ? 0.0 : value > 1.0 ? 1.0 : value;
    }

    /// <summary>The form's border style.</summary>
    public FormBorderStyle FormBorderStyle { get; set; } = FormBorderStyle.Sizable;
}
