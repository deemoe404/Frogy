using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Frogy.Resources.Language
{
    public class LanguageHelper
    {
        static private Dictionary<string, string> supportedLanguage = new Dictionary<string, string>()
        {
            { "en-US", "English" },
            { "zh-CN", "简体中文" }
        };

        static public Dictionary<string, string> SupportedLanguage
        {
            get { return supportedLanguage; }
        }

        static public string InquireFriendlyName(string codeName)
        {
            if (SupportedLanguage.ContainsKey(codeName))
                return SupportedLanguage[codeName];

            return null;
        }

        static public string InquireCodeName(string friendlyName)
        {
            if (SupportedLanguage.ContainsValue(friendlyName))
            {
                foreach (KeyValuePair<string, string> pair in SupportedLanguage)
                {
                    if (pair.Value == friendlyName) return pair.Key;
                }
            }

            return null;
        }

        static public string SystemLanguage
        {
            get
            {
                return System.Globalization.CultureInfo.CurrentUICulture.Name;
            }
        }

        static public string PreferenceLanguage
        {
            get
            {
                return SupportedLanguage.ContainsKey(Properties.Settings.Default.Language) ?
                    Properties.Settings.Default.Language : SystemLanguage;
            }
        }

        static public string InquireLocalizedWord(string inquireKey)
        {
            ResourceDictionary dict = new ResourceDictionary();
            if (SupportedLanguage.ContainsKey(PreferenceLanguage))
                dict.Source = new Uri(@"Resources\Language\"+ PreferenceLanguage + ".xaml", UriKind.Relative);
            else
                dict.Source = new Uri(@"Resources\Language\en-US.xaml", UriKind.Relative);

            if (dict.Contains(inquireKey))
                return dict[inquireKey].ToString();
            else 
                return null;
        }
    }
}
