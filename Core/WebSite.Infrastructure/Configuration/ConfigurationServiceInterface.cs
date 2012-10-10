using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website.Infrastructure.Configuration
{
    public interface ConfigurationServiceInterface
    {
        string GetSetting(string setting);
    }

    public static class Config
    {
        public static ConfigurationServiceInterface Instance { get; set; }
    }
}
