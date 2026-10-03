namespace CNA;

/// <summary>
/// The range check of <c>VertexBuffer</c>/<c>IndexBuffer</c>'s <c>SetData</c> and <c>GetData</c>, as
/// XNA's <c>Helpers.ValidateCopyParameters</c> makes it (XNA IL, CSX-143): a start outside the array
/// is <see cref="ArgumentOutOfRangeException"/> for <c>dataIndex</c>, a count running past its end or
/// not positive one for <c>elementCount</c>, each with XNA's message. Checked as
/// <c>elementCount &gt; length - startIndex</c> once the start is in range, so it cannot overflow.
/// </summary>
internal static class BufferRangeValidation
{
    private const string MustBeValidIndex = "This parameter must be a valid index within the array.";

    public static void ValidateRange(int length, int startIndex, int elementCount)
    {
        if (startIndex < 0 || startIndex > length)
        {
            throw new ArgumentOutOfRangeException("dataIndex", MustBeValidIndex);
        }

        if (elementCount > length - startIndex || elementCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(elementCount), MustBeValidIndex);
        }
    }
}
