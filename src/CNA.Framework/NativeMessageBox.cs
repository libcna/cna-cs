using System.Runtime.InteropServices;
using System.Text;
using CNA.Interop;

namespace CNA;

/// <summary>The native dialog bridge used by the opt-in XNA Windows Forms compatibility assembly.</summary>
internal static unsafe class NativeMessageBox
{
    internal static void SetTestBackendForTests(bool installed, int chosenButton)
    {
        CnaException.ThrowIfFailed(
            Native.cna_message_box_set_test_backend_ext(
                CnaAmbientGame.Current, installed ? (byte)1 : (byte)0, chosenButton),
            nameof(SetTestBackendForTests));
    }

    internal static bool TryShow(
        uint type,
        string title,
        string message,
        string[] buttonLabels,
        out int chosen)
    {
        chosen = -1;
        CnaHandle game = CnaAmbientGame.Current;
        if (game == CnaHandle.Zero ||
            Native.cna_message_box_get_is_supported_ext(game, out byte supported).IsFailure() ||
            supported == 0)
        {
            return false;
        }

        int selected = -1;
        CnaResult result = CnaStringMarshal.WithStringView(title, titleView =>
            CnaStringMarshal.WithStringView(message, messageView =>
                WithButtonLabels(buttonLabels, labels =>
                    Show(game, type, titleView, messageView, labels, out selected))));
        chosen = selected;
        return result.IsSuccess();
    }

    private static CnaResult Show(
        CnaHandle game,
        uint type,
        CnaStringView title,
        CnaStringView message,
        CnaStringView[] labels,
        out int chosen)
    {
        return Native.cna_message_box_show_ext(
            game, type, title, message, in labels[0], (ulong)labels.Length, out chosen);
    }

    private static CnaResult WithButtonLabels(string[] labels, Func<CnaStringView[], CnaResult> call)
    {
        var handles = new GCHandle[labels.Length];
        var views = new CnaStringView[labels.Length];
        try
        {
            for (int index = 0; index < labels.Length; index++)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(labels[index]);
                handles[index] = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                views[index] = new CnaStringView(
                    bytes.Length == 0 ? null : (byte*)handles[index].AddrOfPinnedObject(),
                    (ulong)bytes.Length);
            }

            return call(views);
        }
        finally
        {
            foreach (GCHandle handle in handles)
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }
    }
}
