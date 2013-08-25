using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        public static AppConfig Use(Type type, bool inMemoryOnly = false)
        {
            var attr = type.GetCustomAttributes(typeof(Config), inherit: false).Cast<ConfigFile>().SingleOrDefault();
            var file = attr != null ? attr.File : type.Name;
            var path = System.Environment.CurrentDirectory + "\\ConfigFiles\\" + file + ".config";
            if (!File.Exists(path))
                path = System.Environment.CurrentDirectory + "\\ConfigFiles\\" + type.Name + ".config";
            if (!File.Exists(path))
                path = type.Assembly.Location + ".config";

            if (File.Exists(path) && inMemoryOnly)
            {
                return new MergeAppConfigInMemory(path);
            }
            else if (File.Exists(path))
            {
                return new MergeAppConfigSaveFile(path);
            }
            return new UseExistingAppConfig();
        }

        public abstract void Dispose();

        private class UseExistingAppConfig : AppConfig
        {
            public override void Dispose()
            { }
        }

        private class AppSettingsMerge
        {
            readonly Dictionary<string, string> _remappedVals = new Dictionary<string, string>();
            private readonly AppSettingsSection _target;
            private readonly AppSettingsSection _additional;

            public AppSettingsSection Target { get { return _target; } }
            public AppSettingsSection Additional { get { return _additional; } }

            public AppSettingsMerge(AppSettingsSection target, AppSettingsSection additional)
            {
                _target = target;
                _additional = additional;
            }

            public void Merge()
            {
                foreach (var key in _additional.Settings.AllKeys)
                {
                    var existing = _target.Settings[key];
                    if (existing != null)
                    {
                        _target.Settings.Remove(key);
                        _remappedVals[key] = existing.Value;
                    }
                    _target.Settings.Add(key, _additional.Settings[key].Value);
                }
            }

            public void UnMerge()
            {
                foreach (var key in _additional.Settings.AllKeys)
                {
                    _target.Settings.Remove(key);
                    if (_remappedVals.ContainsKey(key))
                    {
                        _target.Settings.Add(key, _remappedVals[key]);
                    }
                }
            }

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

        private abstract class MergeAppConfigBase : AppConfig
        {
            private readonly string _path;
            protected AppSettingsMerge Merge;

            protected MergeAppConfigBase(string path)
            {
                _path = path;
                ChangeConfiguration();
            }

            protected abstract AppSettingsSection TargetSettionsSection();
            protected abstract void SaveSettings();


            private AppSettingsSection AdditionalSettingsSection()
            {
                var configFileMap =
                    new ExeConfigurationFileMap { ExeConfigFilename = _path };

                var config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
                var appSettings = (AppSettingsSection)config.GetSection("appSettings");
                return appSettings;
            }

            private void ChangeConfiguration()
            {
                Merge = new AppSettingsMerge(TargetSettionsSection(), AdditionalSettingsSection());
                Merge.Merge();
                SaveSettings();
                ValidateSave();
            }

            public override void Dispose()
            {
                if (Merge == null) return;
                Merge.UnMerge();
                SaveSettings();
            }

            private void ValidateSave()
            {
                var settings = (NameValueCollection)ConfigurationManager.GetSection("appSettings");
                if (Merge.Additional.Settings.AllKeys
                    .Any(key => !settings.AllKeys.Any(s => s.Equals(key))))
                {
                    throw new ConfigurationErrorsException("configuration not applied");
                }
            }
        }

        private class MergeAppConfigSaveFile : MergeAppConfigBase
        {
            public MergeAppConfigSaveFile(string path) : base(path)
            {
            }

            private System.Configuration.Configuration _configFile;
            protected override AppSettingsSection TargetSettionsSection()
            {
                _configFile = ConfigurationManager.OpenExeConfiguration(Assembly.GetEntryAssembly().Location);
                return (AppSettingsSection)_configFile.GetSection("appSettings");
            }

            protected override void SaveSettings()
            {
                _configFile.Save();
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private class MergeAppConfigInMemory : MergeAppConfigBase
        {
            public MergeAppConfigInMemory(string path) : base(path)
            {
            }

            private FieldInfo _readonlyProp;
            private FieldInfo _rootProp;
            protected override AppSettingsSection TargetSettionsSection()
            {
                _rootProp = ConfigurationManager.AppSettings.GetType()
               .GetField("_root", BindingFlags.Instance | BindingFlags.NonPublic);

                var section = _rootProp.GetValue(ConfigurationManager.AppSettings) as AppSettingsSection;
                _readonlyProp = typeof(ConfigurationElementCollection).GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
                _readonlyProp.SetValue(section.Settings, false);
                return section;
            }

            protected override void SaveSettings()
            {
                _readonlyProp.SetValue(Merge.Target.Settings, true);
            }

            public override void Dispose()
            {
                _readonlyProp.SetValue(Merge.Target.Settings, false);
                base.Dispose();
            }
            
        }
    }
}