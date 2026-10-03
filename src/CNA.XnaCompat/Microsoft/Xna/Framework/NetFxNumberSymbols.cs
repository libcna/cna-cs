using System.Globalization;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Number symbols as the Windows XNA 4.0 games were written on printed them (CSX-128). .NET reads
/// culture data from ICU, which prints infinity as "∞" and, for Swedish, Finnish, Norwegian and
/// others, the minus sign as U+2212; the .NET Framework of Windows XP to 7 printed "Infinity",
/// "-Infinity" and an ASCII hyphen. A default XNA SpriteFont holds characters 32 to 126, so a game
/// drawing a frame rate or a coordinate drew the ASCII forms and now meets a character its font
/// lacks: Project Mercury's test bench prints <c>1 / ElapsedGameTime.TotalSeconds</c>, infinite
/// on a frame of no elapsed time. Times too (CSX-139): ICU's en-US and other cultures put a narrow
/// no-break space (U+202F) before AM/PM, where Windows' patterns had a space, and ExEn's timing test
/// draws <c>DateTime.ToLongTimeString()</c>. Only those symbols change; the culture is otherwise
/// the user's. <c>&lt;CnaNetFxNumberSymbols&gt;false&lt;/CnaNetFxNumberSymbols&gt;</c> keeps ICU's.
/// </summary>
internal static class NetFxNumberSymbols
{
    private const char NarrowNoBreakSpace = '\u202F';

    internal static void Apply()
    {
        if (AppContext.TryGetSwitch("CNA.XnaCompat.NetFxNumberSymbols", out bool enabled) && !enabled)
        {
            return;
        }

        if (WithNetFxSymbols(CultureInfo.CurrentCulture) is CultureInfo current)
        {
            CultureInfo.CurrentCulture = current;
        }

        // Threads the game starts, which take the default rather than this thread's culture.
        CultureInfo threads = CultureInfo.DefaultThreadCurrentCulture ?? CultureInfo.CurrentCulture;
        CultureInfo.DefaultThreadCurrentCulture = WithNetFxSymbols(threads) ?? threads;
    }

    private static CultureInfo? WithNetFxSymbols(CultureInfo culture)
    {
        NumberFormatInfo format = culture.NumberFormat;
        DateTimeFormatInfo times = culture.DateTimeFormat;
        bool icuOnly = format.PositiveInfinitySymbol.Contains('∞') || format.NegativeInfinitySymbol.Contains('∞') ||
                       format.NegativeSign == "−" ||
                       times.LongTimePattern.Contains(NarrowNoBreakSpace) || times.ShortTimePattern.Contains(NarrowNoBreakSpace) ||
                       times.FullDateTimePattern.Contains(NarrowNoBreakSpace);
        if (!icuOnly)
        {
            return null;
        }

        var patched = (CultureInfo)culture.Clone();
        DateTimeFormatInfo patterns = patched.DateTimeFormat;
        patterns.LongTimePattern = patterns.LongTimePattern.Replace(NarrowNoBreakSpace, ' ');
        patterns.ShortTimePattern = patterns.ShortTimePattern.Replace(NarrowNoBreakSpace, ' ');
        patterns.FullDateTimePattern = patterns.FullDateTimePattern.Replace(NarrowNoBreakSpace, ' ');
        NumberFormatInfo symbols = patched.NumberFormat;
        if (symbols.PositiveInfinitySymbol.Contains('∞'))
        {
            symbols.PositiveInfinitySymbol = "Infinity";
        }

        if (symbols.NegativeInfinitySymbol.Contains('∞'))
        {
            symbols.NegativeInfinitySymbol = "-Infinity";
        }

        if (symbols.NegativeSign == "−")
        {
            symbols.NegativeSign = "-";
        }

        return patched;
    }
}
