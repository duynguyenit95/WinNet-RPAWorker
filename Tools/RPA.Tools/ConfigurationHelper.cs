using System.Text.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;

namespace RPA.Tools
{
    public class ConfigurationHelper
    {
        public static string GetAppSettingsValue(string key)
        {
            var appSettings = ConfigurationManager.AppSettings;
            return appSettings.Get(key);
        }
        public static void SetAppSettingValue(string key,string value)
        {
            System.Configuration.Configuration config =
              ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings.Add(key, value);
            config.Save();
            ConfigurationManager.RefreshSection("appSettings");
        }
    }

}
