using System.Globalization;

namespace Ewallet.Utils;

public static class CurrencyFormatter
{
    private static readonly NumberFormatInfo Format = new()
    {
        NumberGroupSeparator = ".",
        NumberDecimalDigits = 0
    };

    public static string Rupiah(long amount) => "Rp" + amount.ToString("N0", Format);
}