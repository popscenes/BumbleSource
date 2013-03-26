using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }

    public static class Config
    {
        public static ConfigurationServiceInterface Instance { get; set; }
    }
}
