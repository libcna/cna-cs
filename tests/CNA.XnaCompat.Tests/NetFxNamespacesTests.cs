// CSX-131: a using of a namespace .NET Framework 4 had and .NET does not still compiles, as it did
// for the game's author; Visual Studio added this one on its own (WilliamKluge/SuperSmashPolls).
// This assembly imports CNA.XnaCompat.targets as a game does; without the repair it would not build.
using System.Runtime.Remoting.Messaging;
using System.Management;
using Xunit;

namespace CNA.XnaCompat.Tests;

public class NetFxNamespacesTests
{
    [Fact]
    public void AUsingOfANamespaceDotNetDropped_StillCompiles_AndAddsNoPublicType()
    {
        // The placeholder that lets the directive bind is internal to the assembly that names it.
        Type? placeholder = typeof(NetFxNamespacesTests).Assembly.GetType("System.Runtime.Remoting.Messaging.CnaNetFx40Namespace");
        Assert.NotNull(placeholder);
        Assert.False(placeholder!.IsPublic);
        Assert.NotNull(typeof(NetFxNamespacesTests).Assembly.GetType("System.Management.CnaNetFx40Namespace"));
    }
}
