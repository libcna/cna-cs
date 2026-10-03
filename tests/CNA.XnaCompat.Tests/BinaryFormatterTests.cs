// CSX-141: BinaryFormatter compiles and runs for code built against XNA, as on .NET Framework 4.
// This assembly imports CNA.XnaCompat.targets as a game does; without it SYSLIB0011 is an error and
// Serialize throws NotSupportedException.
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace CNA.XnaCompat.Tests;

public class BinaryFormatterTests
{
    [Serializable]
    private sealed class MapSquare
    {
        public int[] LayerTiles = [1, 2, 3];
        public string CodeValue = "START";
        public bool Passable = true;
    }

    [Fact]
    public void AGameReadsBackWhatItsBinaryFormatterWrote()
    {
        var formatter = new BinaryFormatter();
        using var stream = new MemoryStream();
        formatter.Serialize(stream, new MapSquare());
        stream.Position = 0;
        var square = (MapSquare)formatter.Deserialize(stream);

        Assert.Equal([1, 2, 3], square.LayerTiles);
        Assert.Equal("START", square.CodeValue);
        Assert.True(square.Passable);
    }
}
