namespace Ume_Chat_External_General;

// Autism is my superpower
public static class Grammar
{
    public static string GetPlurality(int count, string singular, string plural)
    {
        return count == 1 ? singular : plural;
    }
}
