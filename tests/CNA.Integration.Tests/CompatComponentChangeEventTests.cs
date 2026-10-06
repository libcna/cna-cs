using Microsoft.Xna.Framework;
using Xunit;
using XnaGame = Microsoft.Xna.Framework.Game;

// NOT under CNA, for the reason CompatLayerIntegrationTests records.
namespace CnaDotnet.Integration.Tests.Compat;

/// <summary>
/// CSX-138: a component signals a change of <c>Visible</c>, <c>DrawOrder</c>, <c>Enabled</c> or
/// <c>UpdateOrder</c> only when the value changes, as XNA's setters compare first (IL). ExEn's
/// Marblets sets its already visible title screen visible in its game's constructor; its handler
/// starts the title music, and CNA.NET ran it before the game had loaded any.
/// </summary>
[Collection(global::CNA.Integration.Tests.OwnGameCollection.Name)]
public class CompatComponentChangeEventTests
{
    private sealed class Counting(XnaGame game) : DrawableGameComponent(game)
    {
        public int VisibleChanges { get; private set; }
        public int DrawOrderChanges { get; private set; }
        public int EnabledChanges { get; private set; }
        public int UpdateOrderChanges { get; private set; }

        protected override void OnVisibleChanged(object sender, EventArgs args) => VisibleChanges++;
        protected override void OnDrawOrderChanged(object sender, EventArgs args) => DrawOrderChanges++;
        protected override void OnEnabledChanged(object sender, EventArgs args) => EnabledChanges++;
        protected override void OnUpdateOrderChanged(object sender, EventArgs args) => UpdateOrderChanges++;
    }

    [global::CNA.Integration.Tests.NativeFact]
    public void SettingAComponentsCurrentValues_SignalsNothing()
    {
        using var game = new XnaGame();
        using var component = new Counting(game);

        component.Visible = true;
        component.DrawOrder = 0;
        component.Enabled = true;
        component.UpdateOrder = 0;

        Assert.Equal(0, component.VisibleChanges);
        Assert.Equal(0, component.DrawOrderChanges);
        Assert.Equal(0, component.EnabledChanges);
        Assert.Equal(0, component.UpdateOrderChanges);
    }

    [global::CNA.Integration.Tests.NativeFact]
    public void EachChange_IsSignalledOnce()
    {
        using var game = new XnaGame();
        using var component = new Counting(game);

        component.Visible = false;
        component.Visible = false;
        component.DrawOrder = 5;
        component.DrawOrder = 5;
        component.Enabled = false;
        component.Enabled = false;
        component.UpdateOrder = 3;
        component.UpdateOrder = 3;

        Assert.Equal(1, component.VisibleChanges);
        Assert.Equal(1, component.DrawOrderChanges);
        Assert.Equal(1, component.EnabledChanges);
        Assert.Equal(1, component.UpdateOrderChanges);
        Assert.False(component.Visible);
        Assert.Equal(5, component.DrawOrder);
    }
}
