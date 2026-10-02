namespace CNA.Content;

/// <summary>
/// The two path rules that decide what an XNA external reference names, transcribed from XNA 4.0's
/// own <c>ContentReader.GetPathToReference</c> and <c>TitleContainer.GetCleanPath</c>.
///
/// <b>Why this is not <see cref="Path"/>.</b> An external reference resolves to a *content asset
/// name*, not to a file-system path: it is the string handed back to
/// <c>ContentManager.Load&lt;T&gt;</c>, which is also the manager's cache key. Running it through
/// <see cref="Path.Combine(string, string)"/> makes the answer depend on the host, in two ways that
/// both change identity rather than only spelling:
///
/// <list type="bullet">
/// <item>the separator inserted between the two halves is the host's, so the same asset is
/// <c>Models/x</c> on Linux and <c>Models\x</c> on Windows;</item>
/// <item><see cref="Path.IsPathRooted(string)"/> answers differently -- a reference beginning
/// <c>\</c> is rooted on Windows (so XNA drops the directory and keeps the reference alone) and
/// not rooted on Linux (so the directory is kept). One reference, two different assets.</item>
/// </list>
///
/// XNA ran on Windows, so Windows' rules are the specification here and are spelled out rather
/// than delegated. <see cref="Resolve"/> then hands the result to
/// <see cref="TitleContainer.GetCleanPath"/>, which is where <c>.</c> and <c>..</c> collapse.
///
/// Shared by <c>CNA.Content.Xnb</c>'s reader and <c>CNA.XnaCompat</c>'s <c>ContentReader</c>
/// deliberately: two normalisations of one rule is how the two content paths would come to disagree
/// about which asset a model's effect reference names, and the disagreement would show up as a
/// missing file rather than as a difference.
/// </summary>
internal static class XnaContentPath
{
    /// <summary>
    /// Resolves the external reference <paramref name="reference"/> read while loading
    /// <paramref name="assetName"/> into the asset name it denotes.
    ///
    /// An empty or absent reference answers <see langword="null"/>: XNA reads the reference string
    /// first and returns <c>default(T)</c> for an empty one without consulting the content manager
    /// at all, so "no reference" and "a reference to nothing" are the same thing and neither is an
    /// error.
    /// </summary>
    internal static string? Resolve(string assetName, string? reference)
    {
        ArgumentNullException.ThrowIfNull(assetName);

        if (string.IsNullOrEmpty(reference))
        {
            return null;
        }

        return TitleContainer.GetCleanPath(GetPathToReference(assetName, reference));
    }

    /// <summary>XNA's own <c>ContentReader.GetPathToReference</c>: the reference is relative to the
    /// directory of the asset that names it, where "directory" is everything before the last
    /// separator of the *asset name* -- not of any file-system path it resolved to.</summary>
    internal static string GetPathToReference(string assetName, string reference)
    {
        int separator = assetName.LastIndexOfAny(['\\', '/', Path.DirectorySeparatorChar]);
        string directory = separator < 0 ? string.Empty : assetName[..separator];
        return WindowsCombine(directory, reference);
    }

    /// <summary>
    /// .NET Framework's <c>Path.Combine</c> under Windows rules, which is what XNA's own
    /// <c>GetPathToReference</c> called. Written out because the same call on this host answers
    /// differently -- see this type's own doc comment.
    /// </summary>
    private static string WindowsCombine(string first, string second)
    {
        if (second.Length == 0)
        {
            return first;
        }

        if (first.Length == 0 || IsWindowsRooted(second))
        {
            return second;
        }

        char last = first[^1];
        return last is '\\' or '/' or ':' ? first + second : first + "\\" + second;
    }

    /// <summary>Windows' own <c>Path.IsPathRooted</c>: a leading separator, or a drive letter's
    /// colon in second position.</summary>
    private static bool IsWindowsRooted(string path) =>
        (path.Length >= 1 && path[0] is '\\' or '/') ||
        (path.Length >= 2 && path[1] == ':');

    /// <summary>
    /// The file this asset name and extension denote under <paramref name="rootDirectory"/>.
    ///
    /// This is the one boundary where a content asset name stops being an identity and becomes a
    /// path, and the separator changes here and nowhere else. An XNA asset name is spelled with
    /// backslashes -- <c>Textures\rock_diff</c> -- and on this host a backslash is an ordinary
    /// filename character, so combining without translating asks for a single file literally named
    /// <c>Textures\rock_diff.xnb</c>. Nothing loaded through it, and the error named a path that
    /// looked almost right, which is the worst kind.
    ///
    /// It stayed hidden while nothing produced such a name: a game normally writes its asset names
    /// with whatever separator it likes and this host accepts <c>/</c>. External references are what
    /// made it reachable, because they are *generated* -- <see cref="Resolve"/> answers in XNA's
    /// spelling by construction, which is correct for the identity and unusable as a path.
    ///
    /// The name is not rewritten, only the lookup: <c>ContentManager</c> still caches under the
    /// name XNA would use, and native still receives it as written (CNA's own loader normalises
    /// separators on both the root and the asset name, so it is indifferent).
    /// </summary>
    internal static string ToFilePath(string rootDirectory, string assetName, string extension)
    {
        ArgumentNullException.ThrowIfNull(rootDirectory);
        ArgumentNullException.ThrowIfNull(assetName);

        string relative = assetName
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);

        string exact = Path.Combine(rootDirectory, relative + extension);

        // Probed where the file is, not where the process happens to be: XNA's
        // ContentManager.OpenStream reads a relative content path through TitleContainer, so a
        // relative root is the title's (XNA IL). The answer keeps the caller's relative form, which
        // is what TitleContainer.OpenStream takes.
        //
        // XNA games are written against a case-insensitive filesystem and rely on it, in the file
        // name (CSSAMPLE-022 Pathfinding asks for "map1" and ships Map1.xnb) and in the directories
        // (the Racing Game Kit asks for "models\Cube" where XNA's build wrote Models/Cube.xnb).
        // CNA's native content manager walks every component ignoring case, so the managed side
        // does the same; the exact path is tried first, so a correctly-cased game never scans.
        return File.Exists(ToTitlePath(exact)) ? exact : ToHostPath(exact, AppContext.BaseDirectory);
    }

    /// <summary>
    /// <see cref="ToFilePath"/> as a path the filesystem can open: a content path under a relative
    /// root is resolved against the title's directory, as XNA resolves it, never the working
    /// directory -- a game started from anywhere else would otherwise find none of its content.
    /// </summary>
    internal static string ToTitleFilePath(string rootDirectory, string assetName, string extension) =>
        ToTitlePath(ToFilePath(rootDirectory, assetName, extension));

    /// <summary>A path relative to the title, as an absolute one; an absolute path unchanged.</summary>
    internal static string ToTitlePath(string path) =>
        Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);

    /// <summary>
    /// A file path a Windows-authored game wrote, as this host can open it (cna-cs CSX-096): a
    /// <c>\</c> becomes the host separator, and when the path does not exist as written each
    /// segment is matched ignoring case, as on the filesystem the game was written for.
    /// RolePlayingGame hands <c>AudioEngine</c> <c>Content\Audio\RpgAudio.xgs</c>, ShipGame
    /// <c>content/sounds/sounds.xgs</c> for <c>Content/Sounds</c>; both work on Windows.
    ///
    /// A relative path stays relative and is looked up under <paramref name="baseDirectory"/>
    /// (the working directory when null: what <c>Path.GetFullPath</c>, and so XNA's audio
    /// classes, resolve it against). A path that matches nothing comes back separator-normalized
    /// and otherwise unchanged, so the caller's own not-found error names it. Ties between
    /// entries differing only in case go to the ordinal-first.
    /// </summary>
    internal static string ToHostPath(string path, string? baseDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(path);

        string normalized = Path.DirectorySeparatorChar == '\\' ? path : path.Replace('\\', Path.DirectorySeparatorChar);
        bool rooted = Path.IsPathRooted(normalized);
        string start = rooted ? Path.GetPathRoot(normalized)! : baseDirectory ?? Directory.GetCurrentDirectory();
        string probe = rooted ? normalized : Path.Combine(start, normalized);
        if (File.Exists(probe) || Directory.Exists(probe))
        {
            return normalized;
        }

        string[] segments = (rooted ? normalized[start.Length..] : normalized)
            .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        string current = start;
        var resolved = new List<string>(segments.Length);
        foreach (string segment in segments)
        {
            string candidate = Path.Combine(current, segment);
            if (segment is "." or ".." || File.Exists(candidate) || Directory.Exists(candidate))
            {
                current = candidate;
                resolved.Add(segment);
                continue;
            }

            if (!Directory.Exists(current))
            {
                return normalized;
            }

            string? match = null;
            foreach (string entry in Directory.EnumerateFileSystemEntries(current))
            {
                string name = Path.GetFileName(entry);
                if (string.Equals(name, segment, StringComparison.OrdinalIgnoreCase) &&
                    (match is null || string.CompareOrdinal(name, match) < 0))
                {
                    match = name;
                }
            }

            if (match is null)
            {
                return normalized;
            }

            current = Path.Combine(current, match);
            resolved.Add(match);
        }

        string joined = string.Join(Path.DirectorySeparatorChar, resolved);
        return rooted ? Path.Combine(start, joined) : joined;
    }
}
