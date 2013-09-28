using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.ServiceRuntime;
using Website.Infrastructure.Configuration;

namespace Website.Azure.Common.Environment
{
    public class AzureConfigurationService : DefaultConfigurationService
    {
        protected override string GetBaseSetting(string setting)
        {
            string ret = null;

            try
            {
                if(RoleEnvironment.IsAvailable)
                    ret = RoleEnvironment.GetConfigurationSettingValue(setting);
            }
            catch (Exception){ }

            return ret ?? base.GetBaseSetting(setting);
        }
    }
}
