using Xunit;

namespace CNA.XnaCompat.Tests;

/// <summary>
/// List&lt;T&gt;.ForEach as the .NET Framework 4.0 that XNA 4.0 games target ran it (CSX-110): this
/// assembly imports CNA.XnaCompat.targets as a game does, so its calls are intercepted. On .NET
/// they throw InvalidOperationException as soon as the action changes the list.
/// </summary>
public class NetFx40ListForEachTests
{
    [Fact]
    public void ForEach_VisitsWhatTheActionAppends()
    {
        // escape-from-enceladus: an active event's update activates the next one.
        var list = new List<int> { 1, 2 };
        var seen = new List<int>();

        list.ForEach(item =>
        {
            seen.Add(item);
            if (item < 3)
            {
                list.Add(item + 2);
            }
        });

        Assert.Equal([1, 2, 3, 4], seen);
    }

    [Fact]
    public void ForEach_WalksByIndex_WhenTheActionRemoves()
    {
        // The 4.0 loop indexes the live list, so removing the current element skips the next.
        var list = new List<string> { "a", "b", "c", "d" };
        var seen = new List<string>();

        list.ForEach(item =>
        {
            seen.Add(item);
            list.Remove(item);
        });

        Assert.Equal(["a", "c"], seen);
        Assert.Equal(["b", "d"], list);
    }

    [Fact]
    public void ForEach_NullAction_NamesTheParameterAsTheFramework()
    {
        var list = new List<int> { 1 };

        var thrown = Assert.Throws<ArgumentNullException>(() => list.ForEach(null!));
        Assert.Equal("match", thrown.ParamName);
    }

    [Fact]
    public void ForEach_InAGenericMethod_IsInterceptedToo()
    {
        Assert.Equal(3, AppendWhileWalking(new List<object> { "x" }));
    }

    private static int AppendWhileWalking<T>(List<T> list)
    {
        int visits = 0;
        list.ForEach(item =>
        {
            if (++visits < 3)
            {
                list.Add(item);
            }
        });
        return visits;
    }
}
