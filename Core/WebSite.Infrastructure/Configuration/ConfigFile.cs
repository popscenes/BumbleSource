using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Website.Infrastructure.Configuration
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ConfigFile : Attribute
    {
        public string File { get; set; }

        public ConfigFile(string file = "App")
        {
            File = file;
        }
    }

    public abstract class AppConfig : IDisposable
    {
        public static AppConfig Use(Type type)
        {
            var attr = type.GetCustomAttributes(typeof(Config), inherit: false).Cast<ConfigFile>().SingleOrDefault();
            var file = attr != null ? attr.File : type.Name;
            var path = System.Environment.CurrentDirectory + "\\configs\\" + file + ".config";
            if (!File.Exists(path))
                path = type.Assembly.Location + ".config";

            return File.Exists(path)
                ? new ChangeAppConfig(path) as AppConfig
                : new UseExistingAppConfig() as AppConfig;
        }

        public abstract void Dispose();

        private class UseExistingAppConfig : AppConfig
        {
            public override void Dispose()
            { }
        }

        private class ChangeAppConfig : AppConfig
        {
            private readonly string _oldConfig = AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString();

            private bool _disposedValue;

            public ChangeAppConfig(string path)
            {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", path);
                ResetConfigMechanism();
            }

            public override void Dispose()
            {
                if (!_disposedValue)
                {
                    AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", _oldConfig);
                    ResetConfigMechanism();


                    _disposedValue = true;
                }
                GC.SuppressFinalize(this);
            }

            private static void ResetConfigMechanism()
            {
                var init = typeof(ConfigurationManager).GetField("s_initState", BindingFlags.NonPublic | BindingFlags.Static);
                if (init != null)
                    init.SetValue(null, 0);

                var config = typeof(ConfigurationManager).GetField("s_configSystem", BindingFlags.NonPublic | BindingFlags.Static);
                if (config != null)
                    config.SetValue(null, null);

                var @assembly = typeof(ConfigurationManager).Assembly.GetTypes().FirstOrDefault(x => x.FullName == "System.Configuration.ClientConfigPaths");
                if (@assembly == null) return;
                var current = @assembly.GetField("s_current", BindingFlags.NonPublic | BindingFlags.Static);
                if (current != null)
                    current.SetValue(null, null);
            }
        }
    }
}