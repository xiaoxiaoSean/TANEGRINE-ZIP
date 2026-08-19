using System.Resources;
using System.Reflection;

public static class LanguageManager
{
    private static readonly ResourceManager rm =
        new ResourceManager(
            "TANGERINE_ZIP.LanguageManager.LanguageResource",
            Assembly.GetExecutingAssembly());

    public static string Get(string key)
    {
        return rm.GetString(key) ?? key;
    }
}