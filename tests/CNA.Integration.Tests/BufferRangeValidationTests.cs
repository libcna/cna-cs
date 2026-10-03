using CNA.Graphics;
using Xunit;
using Xunit.Abstractions;

namespace CNA.Integration.Tests;

/// <summary>
/// A vertex or index buffer's SetData/GetData range is checked as XNA's
/// <c>Helpers.ValidateCopyParameters</c> checks it (XNA IL, CSX-143): a start outside the array is
/// <see cref="ArgumentOutOfRangeException"/> for <c>dataIndex</c>, a count past its end or not
/// positive one for <c>elementCount</c>, with XNA's message. Project Mercury's QuadRenderer passes
/// more vertices than its array holds once an emitter outgrows it; CNA.NET threw a plain
/// <see cref="ArgumentException"/> with a message of its own.
/// </summary>
[Collection(NativeGameCollection.Name)]
public class BufferRangeValidationTests(ITestOutputHelper output, NativeGameFixture fixture)
{
    private const string MustBeValidIndex = "This parameter must be a valid index within the array.";

    private static void AssertOutOfRange(string parameter, Action call)
    {
        ArgumentOutOfRangeException thrown = Assert.Throws<ArgumentOutOfRangeException>(call);
        Assert.Equal(parameter, thrown.ParamName);
        Assert.StartsWith(MustBeValidIndex, thrown.Message);
    }

    [Native3DFact]
    public void SetDataAndGetData_RefuseARangeAsXnaDoes()
    {
        fixture.InsideAFrameWithDevice(device =>
        {
            if (!CnaNativeProbe.HasCapabilityOrRefuses(
                    device,
                    GraphicsCapability.ThreeD,
                    "creating a VertexBuffer",
                    () => new VertexBuffer(
                        device, VertexPositionColor.VertexDeclaration, 4, BufferUsage.None).Dispose(),
                    output))
            {
                return;
            }

            using var vertices = new VertexBuffer(device, VertexPositionColor.VertexDeclaration, 8, BufferUsage.None);
            using var indices = new IndexBuffer(device, IndexElementSize.SixteenBits, 8, BufferUsage.None);
            var vertexData = new VertexPositionColor[4];
            var indexData = new short[4];
            int stride = VertexPositionColor.VertexDeclaration.VertexStride;

            AssertOutOfRange("elementCount", () => vertices.SetData(0, vertexData, 0, 5, stride));
            AssertOutOfRange("elementCount", () => vertices.SetData(0, vertexData, 2, 3, stride));
            AssertOutOfRange("elementCount", () => vertices.SetData(0, vertexData, 0, 0, stride));
            AssertOutOfRange("dataIndex", () => vertices.SetData(0, vertexData, 5, 1, stride));
            AssertOutOfRange("dataIndex", () => vertices.SetData(0, vertexData, -1, 1, stride));
            AssertOutOfRange("elementCount", () => vertices.GetData(0, vertexData, 0, 5, stride));
            AssertOutOfRange("dataIndex", () => vertices.GetData(0, vertexData, -1, 1, stride));

            AssertOutOfRange("elementCount", () => indices.SetData(0, indexData, 0, 5));
            AssertOutOfRange("dataIndex", () => indices.SetData(0, indexData, 5, 1));
            AssertOutOfRange("elementCount", () => indices.GetData(0, indexData, 1, 4));

            // An empty array is still the null-or-empty refusal, before any range check.
            Assert.Throws<ArgumentNullException>(() => vertices.SetData(0, Array.Empty<VertexPositionColor>(), 0, 1, stride));

            // A range that fits is accepted.
            vertices.SetData(0, vertexData, 1, 3, stride);
            indices.SetData(0, indexData, 0, 4);
        });
    }
}
