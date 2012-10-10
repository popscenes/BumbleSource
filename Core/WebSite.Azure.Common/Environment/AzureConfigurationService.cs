using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.ServiceRuntime;
using Website.Infrastructure.Configuration;

namespace Website.Azure.Common.Environment
{
    public class AzureConfigurationService : ConfigurationServiceInterface
    {
        public string GetSetting(string setting)
        {
            return RoleEnvironment.IsAvailable ?
                    RoleEnvironment.GetConfigurationSettingValue(setting)
                    :ConfigurationManager.AppSettings[setting];
        }
    }
}
