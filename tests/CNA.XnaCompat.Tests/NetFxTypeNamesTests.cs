using Xunit;

namespace CNA.XnaCompat.Tests.NetFxTypeNames
{
    // A game's own types with names .NET Framework 4.0 never had in System (CSX-126): Project
    // Mercury's ProjectMercury.Range, a game's Index, a two-parameter PriorityQueue.
    public struct Range
    {
        public Range(float minimum, float maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public float Minimum;
        public float Maximum;
    }

    public sealed class Index
    {
    }

    public sealed class PriorityQueue<TElement, TPriority>
    {
    }

    namespace Emitters
    {
        // As Project Mercury writes it: a using inside the namespace is searched before the outer
        // namespace, so on .NET these names would bind System's types.
        using System;
        using System.Collections.Generic;

        public static class Usage
        {
            public static Range ReleaseSpeed => new Range(1f, 3f);

            public static Type RangeType => typeof(Range);

            public static Type IndexType => typeof(Index);

            public static Type PriorityQueueType => typeof(PriorityQueue<,>);
        }
    }

    public class NetFxTypeNamesTests
    {
        [Fact]
        public void AGamesRange_IsTheOneItsSourceMeans()
        {
            Assert.Equal(typeof(Range), Emitters.Usage.RangeType);
            Assert.Equal(3f, Emitters.Usage.ReleaseSpeed.Maximum);
        }

        [Fact]
        public void AGamesIndex_IsTheOneItsSourceMeans() =>
            Assert.Equal(typeof(Index), Emitters.Usage.IndexType);

        [Fact]
        public void AGamesPriorityQueue_IsTheOneItsSourceMeans() =>
            Assert.Equal(typeof(PriorityQueue<,>), Emitters.Usage.PriorityQueueType);

        [Theory]
        [InlineData("System.Range")]
        [InlineData("System.Index")]
        [InlineData("System.Collections.Generic.PriorityQueue`2, System.Collections")]
        public void TheRuntimeTypes_StayPublic(string name)
        {
            // Only the compile sees the narrowed copies; the program runs on the real assemblies.
            Type? type = Type.GetType(name);
            Assert.NotNull(type);
            Assert.True(type!.IsPublic);
        }
    }
}
