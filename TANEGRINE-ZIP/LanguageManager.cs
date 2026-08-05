using System.Resources;
using System.Reflection;

public static class LanguageManager
{
    private static readonly ResourceManager rm =
        new ResourceManager(
            "TANEGRINE_ZIP.LanguageResource",
            Assembly.GetExecutingAssembly());

    public static string Get(string key)
    {
        return rm.GetString(key) ?? key;
    }
}