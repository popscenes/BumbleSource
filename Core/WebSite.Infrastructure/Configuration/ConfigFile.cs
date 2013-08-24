using System;
using System.Collections.Generic;
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
            var path = System.Environment.CurrentDirectory + "\\ConfigFiles\\" + file + ".config";
            if (!File.Exists(path))
                path = System.Environment.CurrentDirectory + "\\ConfigFiles\\" + type.Name + ".config";
            if (!File.Exists(path))
                path = type.Assembly.Location + ".config";

            return File.Exists(path)
                ? new MergeAppConfig(path) as AppConfig
                : new UseExistingAppConfig() as AppConfig;
        }

        public abstract void Dispose();

        private class UseExistingAppConfig : AppConfig
        {
            public override void Dispose()
            { }
        }

        private class SetLoadedConfigWritable : IDisposable
        {
            private readonly FieldInfo _readonlyProp;
            private readonly FieldInfo _rootProp;
            public SetLoadedConfigWritable()
            {
                _rootProp = ConfigurationManager.AppSettings.GetType()
                               .GetField("_root", BindingFlags.Instance | BindingFlags.NonPublic);

                var section = _rootProp.GetValue(ConfigurationManager.AppSettings) as AppSettingsSection;
                _readonlyProp = typeof(ConfigurationElementCollection).GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
                _readonlyProp.SetValue(section.Settings, false);
            }

            public void Dispose()
            {
                var section = _rootProp.GetValue(ConfigurationManager.AppSettings) as AppSettingsSection;
                _readonlyProp.SetValue(section.Settings, false);
                
            }
        }

        private class MergeAppConfig : AppConfig
        {

            private readonly string _path;
            readonly Dictionary<string, string> _remappedVals = new Dictionary<string, string>();

            public MergeAppConfig(string path)
            {
                _path = path;
                ChangeConfiguration();
            }

            private void ChangeConfiguration()
            {
                var appSettings = AppSettingsSection();
                var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetEntryAssembly().Location);
                var currAppSettings = (AppSettingsSection)config.GetSection("appSettings");
                
                foreach (var key in appSettings.Settings.AllKeys)
                {
                    var existing = currAppSettings.Settings[key];
                    if (existing != null)
                    {
                        currAppSettings.Settings.Remove(key);
                        _remappedVals[key] = existing.Value;
                    }
                    currAppSettings.Settings.Add(key, appSettings.Settings[key].Value);
                }

                config.Save();
                ConfigurationManager.RefreshSection("appSettings");
                ConfigurationManager.GetSection("appSettings");
                
            }

            public override void Dispose()
            {
                var appSettings = AppSettingsSection();
                var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetEntryAssembly().Location);
                var currAppSettings = (AppSettingsSection)config.GetSection("appSettings");

                using (new SetLoadedConfigWritable())
                {
                    foreach (var key in appSettings.Settings.AllKeys)
                    {
                        currAppSettings.Settings.Remove(key);
                        if (_remappedVals.ContainsKey(key))
                        {
                            currAppSettings.Settings.Add(key, _remappedVals[key]);
                        }
                    }
                }
                config.Save();
                
                ConfigurationManager.RefreshSection("appSettings");
                ConfigurationManager.GetSection("appSettings");                
            }


            private AppSettingsSection AppSettingsSection()
            {
                var configFileMap =
                    new ExeConfigurationFileMap { ExeConfigFilename = _path };

                var config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
                var appSettings = (AppSettingsSection)config.GetSection("appSettings");
                return appSettings;
            }
        }
    }
}