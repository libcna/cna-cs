using System.Runtime.InteropServices;
using Xunit;

namespace CNA.XnaCompat.Tests;

/// <summary>
/// CSX-134: a struct laid out explicitly for XNA's 32-bit processes, an object reference at a
/// 4-byte offset after its other fields (BEPUphysics' EntityStateChange keeps its Entity at 20).
/// This assembly imports CNA.XnaCompat.targets as a game does; without the repair this type does not
/// load in a 64-bit process.
/// </summary>
public class ThirtyTwoBitLayoutTests
{
    [StructLayout(LayoutKind.Explicit)]
    private struct EntityStateChange
    {
        [FieldOffset(0)]
        public double X;
        [FieldOffset(0)]
        public long Bits;
        [FieldOffset(16)]
        public int Field;
        [FieldOffset(20)]
        public object Target;
        [FieldOffset(24)]
        public string Name;
    }

    [Fact]
    public void AStructLaidOutForThirtyTwoBits_LoadsAndKeepsItsFields()
    {
        var change = new EntityStateChange { X = 1.5, Field = 7, Target = "entity", Name = "name" };
        EntityStateChange[] buffer = [change];

        Assert.Equal(1.5, buffer[0].X);
        Assert.Equal(System.BitConverter.DoubleToInt64Bits(1.5), buffer[0].Bits);
        Assert.Equal(7, buffer[0].Field);
        Assert.Equal("entity", buffer[0].Target);
        Assert.Equal("name", buffer[0].Name);
    }
}
