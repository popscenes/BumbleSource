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

            if (File.Exists(path))
            {
                return new MergeAppConfig(path);
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
            private readonly AppSettingsSection _additional;
            private readonly ConfigurationServiceInterface _configurationService;


            public AppSettingsMerge(AppSettingsSection additional, ConfigurationServiceInterface configurationService)
            {
                _additional = additional;
                _configurationService = configurationService;
            }

            public void Merge()
            {
                _configurationService.AddConfiguration(_additional);
            }

            public void UnMerge()
            {
                _configurationService.RemoveConfiguration(_additional);
            }

        }
//        private class SetLoadedConfigWritable : IDisposable
//        {
//            private readonly FieldInfo _readonlyProp;
//            private readonly FieldInfo _rootProp;
//            public SetLoadedConfigWritable()
//            {
//                _rootProp = ConfigurationManager.AppSettings.GetType()
//                               .GetField("_root", BindingFlags.Instance | BindingFlags.NonPublic);
//
//                var section = _rootProp.GetValue(ConfigurationManager.AppSettings) as AppSettingsSection;
//                _readonlyProp = typeof(ConfigurationElementCollection).GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
//                _readonlyProp.SetValue(section.Settings, false);
//            }
//
//            public void Dispose()
//            {
//                var section = _rootProp.GetValue(ConfigurationManager.AppSettings) as AppSettingsSection;
//                _readonlyProp.SetValue(section.Settings, false);
//                
//            }
//        }

        private class MergeAppConfig : AppConfig
        {
            private readonly string _path;
            private AppSettingsMerge _merge;

            public MergeAppConfig(string path)
            {
                _path = path;
                ChangeConfiguration();
            }

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
                _merge = new AppSettingsMerge(AdditionalSettingsSection(), Config.Instance);
                _merge.Merge();
            }

            public override void Dispose()
            {
                if (_merge == null) return;
                _merge.UnMerge();
            }
        }

//        private class MergeAppConfigSaveFile : MergeAppConfigBase
//        {
//            public MergeAppConfigSaveFile(string path) : base(path)
//            {
//            }
//
//            private System.Configuration.Configuration _configFile;
//            protected override AppSettingsSection TargetSettionsSection()
//            {
//                _configFile = ConfigurationManager.OpenExeConfiguration(Assembly.GetEntryAssembly().Location);
//                return (AppSettingsSection)_configFile.GetSection("appSettings");
//            }
//
//            protected override void SaveSettings()
//            {
//                _configFile.Save();
//                ConfigurationManager.RefreshSection("appSettings");
//            }
//        }

//        private class MergeAppConfigInMemory : MergeAppConfigBase
//        {
//            public MergeAppConfigInMemory(string path) : base(path)
//            {
//            }
//
//            private FieldInfo _readonlyProp;
//            private FieldInfo _rootProp;
//            protected override AppSettingsSection TargetSettionsSection()
//            {
//                _rootProp = ConfigurationManager.AppSettings.GetType()
//               .GetField("_root", BindingFlags.Instance | BindingFlags.NonPublic);
//
//                var section = _rootProp.GetValue(ConfigurationManager.AppSettings) as AppSettingsSection;
//                _readonlyProp = typeof(ConfigurationElementCollection).GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
//                _readonlyProp.SetValue(section.Settings, false);
//                return section;
//            }
//
//            protected override void SaveSettings()
//            {
//                _readonlyProp.SetValue(Merge.Target.Settings, true);
//            }
//
//            public override void Dispose()
//            {
//                _readonlyProp.SetValue(Merge.Target.Settings, false);
//                base.Dispose();
//            }
//            
//        }
    }
}