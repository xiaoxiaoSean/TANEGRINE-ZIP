using System.Globalization;

namespace TANEGRINE_ZIP
{
    public static class LanguageManager
    {
        public static void SetLanguage(string language)
        {
            CultureInfo.CurrentUICulture =
                new CultureInfo(language);
        }


        public static string Get(string key)
        {
            return LanguageResource.ResourceManager
                .GetString(key);
        }
    }
}