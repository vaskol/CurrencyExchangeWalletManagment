using System.Globalization;
namespace Core.Utilities;
public static class NumberFormattingHelper
{
    private static readonly CultureInfo EuroCulture = new CultureInfo("de-DE");

    public static string ToPrettyAmount(decimal value)
    {
        return value.ToString("#,##0.##", EuroCulture);
    }
}
