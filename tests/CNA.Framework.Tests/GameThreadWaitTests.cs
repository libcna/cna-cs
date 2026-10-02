using System.Threading;
using Xunit;

namespace CNA.Framework.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class GameThreadCollection
{
    public const string Name = "GameThread";
}

/// <summary>
/// A game thread that waits for a worker runs the calls that worker queued for it: XNA's loading
/// screens join the thread that draws their animation from Update (Microsoft's Network Game State
/// Management sample), and a worker's call that waited for the next Update deadlocked them.
/// </summary>
[Collection(GameThreadCollection.Name)]
public class GameThreadWaitTests
{
    [Fact]
    public void AJoinOnTheGameThread_RunsTheCallsTheWorkerQueued()
    {
        int nativeRuns = 0;
        GameThread.Enter(() => Interlocked.Increment(ref nativeRuns));
        try
        {
            int gameThread = Environment.CurrentManagedThreadId;
            int ranOn = 0;
            var worker = new Thread(() =>
            {
                try
                {
                    ranOn = GameThread.Invoke(() => Environment.CurrentManagedThreadId);
                }
                catch (InvalidOperationException)
                {
                    // The game ended before its thread ran the call: what a wait that does not run
                    // the queue ends in, once the assertion below has given up on it.
                }
            });
            worker.Start();

            Assert.True(worker.Join(TimeSpan.FromSeconds(10)), "the game thread's wait did not run the worker's call");
            Assert.Equal(gameThread, ranOn);
            Assert.True(nativeRuns > 0, "the wait did not run native CNA's queue");
        }
        finally
        {
            GameThread.Exit();
        }
    }

    [Fact]
    public void TheWaitStillTimesOut_AndAWaitThatEndsAtOnceReturnsAtOnce()
    {
        GameThread.Enter(() => { });
        try
        {
            using var never = new ManualResetEvent(false);
            var clock = System.Diagnostics.Stopwatch.StartNew();
            Assert.False(never.WaitOne(50));
            Assert.InRange(clock.ElapsedMilliseconds, 40, 2000);

            using var already = new ManualResetEvent(true);
            Assert.True(already.WaitOne(Timeout.Infinite));
        }
        finally
        {
            GameThread.Exit();
        }
    }

    [Fact]
    public void TheContextItReplaced_PostsAndComesBackWhenTheGameEnds()
    {
        SynchronizationContext? original = SynchronizationContext.Current;
        var previous = new RecordingContext();
        SynchronizationContext.SetSynchronizationContext(previous);
        try
        {
            GameThread.Enter(() => { });
            SynchronizationContext? during = SynchronizationContext.Current;
            Assert.NotSame(previous, during);
            during!.Post(_ => { }, null);
            Assert.Equal(1, previous.Posts);

            GameThread.Exit();
            Assert.Same(previous, SynchronizationContext.Current);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(original);
        }
    }

    private sealed class RecordingContext : SynchronizationContext
    {
        public int Posts;

        public override void Post(SendOrPostCallback d, object? state) => Posts++;
    }
}
