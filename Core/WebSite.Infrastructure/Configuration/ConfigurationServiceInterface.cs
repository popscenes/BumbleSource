using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Website.Infrastructure.Types;
using Website.Infrastructure.Util;

namespace Website.Infrastructure.Configuration
{
    public static class ConfigurationServiceInterfaceExtension
    {
        public static SettingType GetSetting<SettingType>(this ConfigurationServiceInterface service, string setting,
                                                        SettingType defaultVal = default(SettingType) )
        {
            var val = service.GetSetting(setting);
            if (string.IsNullOrWhiteSpace(val))
                return defaultVal;

            var ret = SerializeUtil.ConvertVal(val, typeof (SettingType));
            if (ret == null)
                return defaultVal;

            return (SettingType)ret;
        }
    }

    public interface ConfigurationServiceInterface
    {
        string GetSetting(string setting);
        void AddConfiguration(Dictionary<string, string> settings);
        void RemoveConfiguration(Dictionary<string, string> settings);
        void AddConfiguration(AppSettingsSection settings);
        void RemoveConfiguration(AppSettingsSection settings);
        void ClearExtra();
    }

    public abstract class ConfigurationService : ConfigurationServiceInterface
    {
        private readonly Dictionary<string, string> _additionalConfig = new Dictionary<string, string>(); 

        public string GetSetting(string setting)
        {
            string value;
            if (_additionalConfig.TryGetValue(setting, out value) && value != null)
                return value;

            return GetBaseSetting(setting);
        }

        protected abstract string GetBaseSetting(string setting);

        public void AddConfiguration(Dictionary<string, string> settings)
        {
            foreach (var setting in settings)
            {
                _additionalConfig.Add(setting.Key, setting.Value);
            }
        }

        public void RemoveConfiguration(Dictionary<string, string> settings)
        {
            foreach (var setting in settings)
            {
                _additionalConfig.Remove(setting.Key);
            }
        }

        public void AddConfiguration(AppSettingsSection settings)
        {
            foreach (var setting in settings.Settings.AllKeys)
            {
                _additionalConfig.Add(setting, settings.Settings[setting].Value);
            }
        }

        public void RemoveConfiguration(AppSettingsSection settings)
        {
            foreach (var setting in settings.Settings.AllKeys)
            {
                _additionalConfig.Remove(setting);
            }
        }

        public void ClearExtra()
        {
            _additionalConfig.Clear();
        }
    }

    public class DefaultConfigurationService : ConfigurationService
    {
        protected override string GetBaseSetting(string setting)
        {
            try
            {
                return ConfigurationManager.AppSettings[setting];
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public static class Config
    {
        public static ConfigurationServiceInterface Instance { get; set; }
    }


    
}
