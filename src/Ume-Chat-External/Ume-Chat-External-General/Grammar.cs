namespace Ume_Chat_External_General;

/// <summary>
///     Class to manage grammar inside of logs...
/// </summary>
public static class Grammar
{
    /// <summary>
    ///     Autism is my superpower
    /// </summary>
    public static string GetPlurality(int count, string singular, string plural)
    {
        return count == 1 ? singular : plural;
    }
}
