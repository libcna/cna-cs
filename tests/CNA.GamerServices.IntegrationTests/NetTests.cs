using CNA.Integration.Tests;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;
using Xunit;
using Xunit.Abstractions;

namespace CnaCs.GamerServices.IntegrationTests;

[Collection(GamerServicesCollection.Name)]
public class NetTests(GamerServicesGameFixture fixture, ITestOutputHelper output)
{
    [NativeFact]
    public void LocalSession_HasOneManagedObjectPerGamerAcrossEveryView()
    {
        fixture.InsideAFrame(_ =>
        {
            using NetworkSession session = NetworkSession.Create(NetworkSessionType.Local, 1, 4);
            Assert.Equal(NetworkSessionType.Local, session.SessionType);
            Assert.Equal(NetworkSessionState.Lobby, session.SessionState);
            Assert.True(session.IsHost);
            Assert.Equal(4, session.MaxGamers);

            LocalNetworkGamer local = Assert.Single<LocalNetworkGamer>(session.LocalGamers);
            Assert.Same(local, Assert.Single<NetworkGamer>(session.AllGamers));
            Assert.Empty((IEnumerable<NetworkGamer>)session.RemoteGamers);
            Assert.Same(local, session.Host);
            Assert.Same(local, session.FindGamerById(local.Id));
            Assert.Same(session, local.Session);
            Assert.True(local.IsLocal);
            Assert.True(local.IsHost);
            Assert.False(local.HasLeftSession);
            Assert.Equal("CnaTester", local.Gamertag);
            Assert.Same(Gamer.SignedInGamers[PlayerIndex.One], local.SignedInGamer);

            NetworkMachine machine = local.Machine;
            Assert.Same(machine, local.Machine);
            Assert.Same(local, Assert.Single<NetworkGamer>(machine.Gamers));

            // XNA replays GamerJoined to a new handler for every gamer already there.
            var joined = new List<NetworkGamer>();
            session.GamerJoined += (sender, e) => joined.Add(e.Gamer);
            Assert.Same(local, Assert.Single(joined));
        });
    }

    [NativeFact]
    public void LocalSession_StateMachineRaisesItsEventsFromUpdate()
    {
        fixture.InsideAFrame(_ =>
        {
            using NetworkSession session = NetworkSession.Create(NetworkSessionType.Local, 1, 4);
            var events = new List<string>();
            session.GameStarted += (_, _) => events.Add("started");
            session.GameEnded += (_, _) => events.Add("ended");

            LocalNetworkGamer local = session.LocalGamers[0];
            local.IsReady = true;
            Assert.True(local.IsReady);
            Assert.True(session.IsEveryoneReady);

            session.StartGame();
            session.Update();
            Assert.Equal(NetworkSessionState.Playing, session.SessionState);
            session.EndGame();
            session.Update();
            Assert.Equal(NetworkSessionState.Lobby, session.SessionState);
            Assert.Equal(["started", "ended"], events);

            session.ResetReady();
            Assert.False(local.IsReady);
        });
    }

    [NativeFact]
    public void HandlerException_IsRethrownFromUpdate()
    {
        fixture.InsideAFrame(_ =>
        {
            using NetworkSession session = NetworkSession.Create(NetworkSessionType.Local, 1, 4);
            session.GameStarted += (_, _) => throw new FormatException("from the handler");
            session.StartGame();
            FormatException thrown = Assert.Throws<FormatException>(session.Update);
            Assert.Equal("from the handler", thrown.Message);
            Assert.Equal(NetworkSessionState.Playing, session.SessionState);
        });
    }

    /// <summary>
    /// XNA's samples send to every gamer and skip packets whose sender <c>IsLocal</c>: a SystemLink
    /// session loops a gamer's own packets back. That round trip carries a packet across native's
    /// transport and back into the managed reader at its exact length.
    /// </summary>
    [NativeFact]
    public void SystemLinkSession_LoopsAPacketBackToItsLocalGamer()
    {
        fixture.InsideAFrame(_ =>
        {
            using NetworkSession session = NetworkSession.Create(NetworkSessionType.SystemLink, 1, 2);
            LocalNetworkGamer local = session.LocalGamers[0];
            var writer = new PacketWriter();
            writer.Write(new Vector3(1, 2, 3));
            writer.Write(Color.CornflowerBlue);
            writer.Write("hello");
            int length = writer.Length;
            local.SendData(writer, SendDataOptions.ReliableInOrder);
            Assert.Equal(0, writer.Length);

            for (int attempt = 0; attempt < 50 && !local.IsDataAvailable; attempt++)
            {
                session.Update();
                if (!local.IsDataAvailable)
                {
                    Thread.Sleep(10);
                }
            }

            var reader = new PacketReader();
            int received = local.ReceiveData(reader, out NetworkGamer sender);
            output.WriteLine($"received {received} of {length} bytes");
            Assert.Equal(length, received);
            Assert.Equal(length, reader.Length);
            Assert.Equal(0, reader.Position);
            Assert.Same(local, sender);
            Assert.True(sender.IsLocal);
            Assert.Equal(new Vector3(1, 2, 3), reader.ReadVector3());
            Assert.Equal(Color.CornflowerBlue, reader.ReadColor());
            Assert.Equal("hello", reader.ReadString());

            // An empty queue empties the reader and names no sender, as XNA's does.
            Assert.Equal(0, local.ReceiveData(reader, out NetworkGamer none));
            Assert.Null(none);
            Assert.Equal(0, reader.Length);
        });
    }

    /// <summary>
    /// CNA's Local session does not loop packets back (its NET.md: the pump drops a Local packet
    /// event). Whether XNA's did is not visible in its managed IL -- the packet goes to its native
    /// kernel -- so this pins CNA's behaviour and that nothing is half-delivered.
    /// </summary>
    [NativeFact]
    public void LocalSession_DropsPacketsWithoutHalfDeliveringThem()
    {
        fixture.InsideAFrame(_ =>
        {
            using NetworkSession session = NetworkSession.Create(NetworkSessionType.Local, 1, 4);
            LocalNetworkGamer local = session.LocalGamers[0];
            local.SendData(new byte[] { 1, 2, 3 }, SendDataOptions.Reliable);
            session.Update();
            Assert.False(local.IsDataAvailable);
            var reader = new PacketReader();
            Assert.Equal(0, local.ReceiveData(reader, out NetworkGamer sender));
            Assert.Null(sender);
            Assert.Equal(0, reader.Length);
        });
    }

    [NativeFact]
    public void SessionProperties_AreTheSessionsAndRefuseWhatXnaRefuses()
    {
        fixture.InsideAFrame(_ =>
        {
            var search = new NetworkSessionProperties { [0] = 7 };
            using NetworkSession session = NetworkSession.Create(NetworkSessionType.Local, 1, 4, 0, search);
            Assert.Equal(7, session.SessionProperties[0]);
            Assert.Null(session.SessionProperties[1]);
            session.SessionProperties[1] = -3;
            Assert.Equal(-3, session.SessionProperties[1]);
            Assert.Equal(8, session.SessionProperties.Count);

            Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => search[8]).ParamName);
            IList<int?> list = search;
            Assert.False(list.IsReadOnly);
            Assert.Throws<NotSupportedException>(() => list.Add(1));
            Assert.Throws<NotSupportedException>(() => list.RemoveAt(0));
            Assert.Throws<NotSupportedException>(list.Clear);
        });
    }

    [NativeFact]
    public void Create_ValidatesItsArgumentsInXnasOrder()
    {
        Assert.Equal("maxLocalGamers", Assert.Throws<ArgumentOutOfRangeException>(
            () => NetworkSession.Create(NetworkSessionType.Local, 0, 4)).ParamName);
        Assert.Equal("maxGamers", Assert.Throws<ArgumentOutOfRangeException>(
            () => NetworkSession.Create(NetworkSessionType.SystemLink, 1, 1)).ParamName);
        Assert.Equal("maxGamers", Assert.Throws<ArgumentOutOfRangeException>(
            () => NetworkSession.Create(NetworkSessionType.Local, 1, 32)).ParamName);
        Assert.Equal("privateGamerSlots", Assert.Throws<ArgumentOutOfRangeException>(
            () => NetworkSession.Create(NetworkSessionType.Local, 1, 4, 4, null!)).ParamName);
        Assert.Equal("sessionType", Assert.Throws<ArgumentException>(
            () => NetworkSession.Find(NetworkSessionType.Local, 1, null!)).ParamName);
        Assert.Equal("localGamers", Assert.Throws<ArgumentNullException>(
            () => NetworkSession.Create(NetworkSessionType.Local, null!, 4, 0, null!)).ParamName);
        Assert.Equal("availableSession", Assert.Throws<ArgumentNullException>(
            () => NetworkSession.Join(null!)).ParamName);
    }

    [NativeFact]
    public void Dispose_ReleasesTheSessionAndAllowsTheNextOne()
    {
        fixture.InsideAFrame(_ =>
        {
            NetworkSession first = NetworkSession.Create(NetworkSessionType.Local, 1, 4);
            LocalNetworkGamer gamer = first.LocalGamers[0];
            first.Dispose();
            first.Dispose();
            Assert.True(first.IsDisposed);
            Assert.Throws<ObjectDisposedException>(first.Update);

            // Only one session exists at a time, so this proves the first one is really gone.
            using NetworkSession second = NetworkSession.Create(
                NetworkSessionType.Local, [Gamer.SignedInGamers[PlayerIndex.One]], 4, 0, null!);
            Assert.NotSame(gamer, second.LocalGamers[0]);
            Assert.Same(second, second.LocalGamers[0].Session);
        });
    }

    [NativeFact]
    public void BeginCreate_CompletesSynchronouslyAndEndsOnce()
    {
        fixture.InsideAFrame(_ =>
        {
            int callbacks = 0;
            IAsyncResult result = NetworkSession.BeginCreate(NetworkSessionType.Local, 1, 4, _ => callbacks++, "state");
            Assert.True(result.CompletedSynchronously);
            Assert.Equal(1, callbacks);
            using NetworkSession session = NetworkSession.EndCreate(result);
            Assert.Equal("state", result.AsyncState);
            Assert.Throws<InvalidOperationException>(() => NetworkSession.EndCreate(result));
            Assert.Throws<ArgumentException>(() => NetworkSession.EndJoin(result));
        });
    }

    [NativeFact]
    public void PacketWriterAndReader_UseXnasLayout()
    {
        var writer = new PacketWriter();
        writer.Write(new Vector2(1, 2));
        writer.Write(Matrix.CreateTranslation(4, 5, 6));
        writer.Write(new Quaternion(1, 2, 3, 4));
        writer.Write(new Color(10, 20, 30, 40));
        writer.Write(BitConverter.UInt32BitsToSingle(0x7FC01234));
        writer.Write(2.5);
        Assert.Equal(8 + 64 + 16 + 4 + 4 + 8, writer.Length);

        byte[] bytes = new byte[writer.Length];
        Array.Copy(writer.ByteArrayForTest(), bytes, bytes.Length);
        Assert.Equal(new byte[] { 10, 20, 30, 40 }, bytes[88..92]);

        var reader = new PacketReader();
        reader.SetPacketForTest(bytes);
        Assert.Equal(new Vector2(1, 2), reader.ReadVector2());
        Assert.Equal(Matrix.CreateTranslation(4, 5, 6), reader.ReadMatrix());
        Assert.Equal(new Quaternion(1, 2, 3, 4), reader.ReadQuaternion());
        Assert.Equal(new Color(10, 20, 30, 40), reader.ReadColor());
        Assert.Equal(0x7FC01234u, BitConverter.SingleToUInt32Bits(reader.ReadSingle()));
        Assert.Equal(2.5, reader.ReadDouble());
        Assert.Equal(reader.Length, reader.Position);
    }
}

internal static class PacketTestAccess
{
    internal static byte[] ByteArrayForTest(this PacketWriter writer) =>
        ((MemoryStream)writer.BaseStream).GetBuffer();

    internal static void SetPacketForTest(this PacketReader reader, byte[] bytes)
    {
        var stream = (MemoryStream)reader.BaseStream;
        stream.SetLength(0);
        stream.Write(bytes, 0, bytes.Length);
        stream.Position = 0;
    }
}
