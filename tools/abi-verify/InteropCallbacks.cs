// SPDX-License-Identifier: MIT
using System.Reflection;
using System.Runtime.InteropServices;

/// <summary>
/// Proves that the callbacks this binding hands to CNA have the shape CNA declares.
///
/// The prototype probe checks the *routes*, and for a callback parameter the managed side declares
/// as <c>nint</c> it can only check that a pointer is passed. What crosses at run time is a managed
/// function, and nothing compared its signature with the C typedef until this existed. Two real
/// defects were sitting behind that gap: an event callback took the sender as <c>nint</c> where CNA
/// passes a <c>CNA_Handle</c> (identical on a 64-bit target, four bytes short on a 32-bit one), and
/// the content-reader destroy callback returned a result code where CNA declares <c>void</c>.
///
/// Each check declares a C function with the signature derived from the *managed* member -- a
/// method carrying <c>UnmanagedCallersOnly</c>, or a function-pointer field of an interop struct --
/// and assigns it to the C typedef. Nothing here restates the header; the assignment is what fails.
/// </summary>
static class InteropCallbacks
{
    /// <summary>
    /// Which C typedef each managed callback implements.
    ///
    /// The pairing cannot be read from metadata -- a function pointer is handed to a route at a call
    /// site, not declared as implementing anything -- so it is listed, and each entry is a statement
    /// that the call site was read. Two event shapes, the three content-reader callbacks, the CNB
    /// loader, and the GamerServices and Net callbacks CNA.XnaCompat hands to native directly
    /// account for every callback this binding provides. The Net session callbacks are why the
    /// XnaCompat entries exist: they take the session first, and a two-parameter version read the
    /// event description as the context and crashed on the first event.
    /// </summary>
    public static readonly (string Owner, string Member, string Typedef)[] Pairings =
    [
        ("CNA.NativeEventBridge", "OnNativeEvent", "CNA_GameEventCallback"),
        ("CNA.NativeEventBridge", "OnNativeEventWithSender", "CNA_GraphicsResourceDisposingCallback"),
        ("CnaContentTypeReaderCallbacks", "Create", "CNA_ContentTypeReaderCreateCallback"),
        ("CnaContentTypeReaderCallbacks", "Read", "CNA_ContentTypeReaderReadCallback"),
        ("CnaContentTypeReaderCallbacks", "Destroy", "CNA_ContentTypeReaderDestroyCallback"),
        ("CNA.Content.Cnb.CnbLoaderRegistration", "OnLoad", "CNA_CnbLoaderCallback"),
        ("Microsoft.Xna.Framework.GamerServices.GamerServicesAsyncResult", "OnNativeCompletion", "CNA_GamerAsyncCallback"),
        ("Microsoft.Xna.Framework.GamerServices.GamerServicesDispatcher", "OnInstallingTitleUpdate", "CNA_GamerAsyncCallback"),
        ("Microsoft.Xna.Framework.GamerServices.AvatarDescription", "OnNativeChanged", "CNA_GamerAsyncCallback"),
        ("Microsoft.Xna.Framework.GamerServices.SignedInGamer", "OnSignedIn", "CNA_SignedInGamerEventCallback"),
        ("Microsoft.Xna.Framework.GamerServices.SignedInGamer", "OnSignedOut", "CNA_SignedInGamerEventCallback"),
        ("Microsoft.Xna.Framework.Net.NetworkSession", "OnGameStarted", "CNA_GameStartedCallback"),
        ("Microsoft.Xna.Framework.Net.NetworkSession", "OnGameEnded", "CNA_GameEndedCallback"),
        ("Microsoft.Xna.Framework.Net.NetworkSession", "OnGamerJoined", "CNA_GamerJoinedCallback"),
        ("Microsoft.Xna.Framework.Net.NetworkSession", "OnGamerLeft", "CNA_GamerLeftCallback"),
        ("Microsoft.Xna.Framework.Net.NetworkSession", "OnHostChanged", "CNA_HostChangedCallback"),
        ("Microsoft.Xna.Framework.Net.NetworkSession", "OnSessionEnded", "CNA_NetworkSessionEndedCallback"),
        ("Microsoft.Xna.Framework.Net.NetworkSession", "OnWriteArbitrated", "CNA_WriteLeaderboardsCallback"),
        ("Microsoft.Xna.Framework.Net.NetworkSession", "OnWriteUnarbitrated", "CNA_WriteLeaderboardsCallback"),
        ("Microsoft.Xna.Framework.Net.NetworkSession", "OnWriteTrueSkill", "CNA_WriteLeaderboardsCallback"),
        ("Microsoft.Xna.Framework.Net.NetworkSession", "OnInviteAccepted", "CNA_InviteAcceptedCallback"),
    ];

    /// <summary>
    /// Callback parameters whose C spelling differs from the managed one only by <c>const</c>, which
    /// C# cannot put on a pointer: an event description CNA lends for the duration of the call. Keyed
    /// <c>Member#index</c>; anything else about the parameter is still compared.
    /// </summary>
    private static readonly Dictionary<string, string> ConstParameters = new(StringComparer.Ordinal)
    {
        ["OnSignedIn#1"] = "const CNA_SignedInGamerEventInfo*",
        ["OnSignedOut#1"] = "const CNA_SignedInGamerEventInfo*",
        ["OnGameStarted#1"] = "const CNA_GameStartedEventInfo*",
        ["OnGameEnded#1"] = "const CNA_GameEndedEventInfo*",
        ["OnGamerJoined#1"] = "const CNA_GamerJoinedEventInfo*",
        ["OnGamerLeft#1"] = "const CNA_GamerLeftEventInfo*",
        ["OnHostChanged#1"] = "const CNA_HostChangedEventInfo*",
        ["OnSessionEnded#1"] = "const CNA_NetworkSessionEndedEventInfo*",
        ["OnWriteArbitrated#1"] = "const CNA_WriteLeaderboardsEventInfo*",
        ["OnWriteUnarbitrated#1"] = "const CNA_WriteLeaderboardsEventInfo*",
        ["OnWriteTrueSkill#1"] = "const CNA_WriteLeaderboardsEventInfo*",
        ["OnInviteAccepted#0"] = "const CNA_InviteAcceptedEventInfo*",
    };

    /// <summary>The managed signature of one pairing, as C, or null when it cannot be found.</summary>
    private static (string Return, string[] Parameters)? Signature(string owner, string member)
    {
        foreach (Assembly assembly in new[]
                 {
                     typeof(CNA.Interop.CnaHandle).Assembly, typeof(CNA.Game).Assembly,
                     typeof(Microsoft.Xna.Framework.Net.NetworkSession).Assembly,
                 })
        {
            Type? type = assembly.GetTypes().FirstOrDefault(
                t => t.FullName == owner || t.Name == owner);
            if (type is null)
            {
                continue;
            }

            MethodInfo? method = type.GetMethod(
                member, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method is not null)
            {
                string? returned = InteropPrototypes.CType(method.ReturnType);
                string?[] parameters = [.. method.GetParameters().Select(p => InteropPrototypes.CType(p.ParameterType))];
                return returned is null || parameters.Any(p => p is null)
                    ? null
                    : (returned, parameters.Select(p => p!).ToArray());
            }

            FieldInfo? field = type.GetField(
                member, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field is not null && field.FieldType.IsFunctionPointer)
            {
                string? returned = InteropPrototypes.CType(field.FieldType.GetFunctionPointerReturnType());
                string?[] parameters =
                    [.. field.FieldType.GetFunctionPointerParameterTypes().Select(InteropPrototypes.CType)];
                return returned is null || parameters.Any(p => p is null)
                    ? null
                    : (returned, parameters.Select(p => p!).ToArray());
            }
        }

        return null;
    }

    public static string Generate(out List<string> unresolved)
    {
        unresolved = [];

        var text = new System.Text.StringBuilder();
        text.AppendLine("// SPDX-License-Identifier: MIT");
        text.AppendLine("// Generated by CNA.AbiVerify from the callbacks this binding provides. Do not edit.");
        text.AppendLine();
        text.AppendLine("#include <stddef.h>");
        text.AppendLine("#include <stdint.h>");
        text.AppendLine();
        text.AppendLine("#include \"CNA/C/cna.h\"");
        text.AppendLine();

        foreach ((string owner, string member, string typedef) in Pairings)
        {
            if (Signature(owner, member) is not { } signature)
            {
                unresolved.Add($"{owner}.{member}");
                continue;
            }

            // C11 requires a definition's parameters to be named, and -Wextra requires them to be
            // used, so each one is named and discarded. The body is irrelevant: what is under test
            // is the type the compiler gives this function, and whether it may be assigned to the
            // typedef CNA declares.
            string[] spelled = [.. signature.Parameters.Select((p, i) =>
                ConstParameters.TryGetValue($"{member}#{i}", out string? constant) && constant == "const " + p ? constant : p)];
            string parameters = spelled.Length == 0
                ? "void"
                : string.Join(", ", spelled.Select((p, i) => $"{p} p{i}"));

            string function = $"mcb_{owner.Replace('.', '_')}_{member}";
            text.AppendLine($"// {owner}.{member}");
            text.AppendLine($"static {signature.Return} {function}({parameters})");
            text.AppendLine("{");
            for (int index = 0; index < signature.Parameters.Length; index++)
            {
                text.AppendLine($"    (void)p{index};");
            }

            if (signature.Return != "void")
            {
                text.AppendLine($"    return ({signature.Return})0;");
            }

            text.AppendLine("}");
            text.AppendLine($"{typedef} const chk_{function} = &{function};");
            text.AppendLine();
        }

        return text.ToString();
    }
}
