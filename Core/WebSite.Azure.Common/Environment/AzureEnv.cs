using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.ServiceRuntime;
using Website.Infrastructure.Configuration;

namespace Website.Azure.Common.Environment
{
    public static class AzureEnv
    {
        private static bool? _usingRealTableStorage;
        public static bool UseRealStorage
        {
            get 
            {
                if (_usingRealTableStorage.HasValue)
                    return _usingRealTableStorage.Value;

                _usingRealTableStorage = Config.Instance != null && Config.Instance.GetSetting<bool>("UseProductionStorage");
                return _usingRealTableStorage.Value;
            }
            set { _usingRealTableStorage = value; }
        }

        public static bool IsRunningInDevFabric()
        {
            return IsRunningInCloud() && RoleEnvironment.IsEmulated;
        }

        public static bool IsRunningInCloud()
        {
            return RoleEnvironment.IsAvailable;
        }

        public static bool IsRunningInProdFabric()
        {
            return IsRunningInCloud() && !IsRunningInDevFabric();
        }

        public static string GetIdForInstance()
        {
             return RoleEnvironment.CurrentRoleInstance != null ? RoleEnvironment.CurrentRoleInstance.Id : ".0";
        }

        public static int GetInstanceIndex()
        {
            if (!IsRunningInCloud())
                return 0;

            var instanceId = RoleEnvironment.CurrentRoleInstance.Id;
            var instanceIndex = 0;
            if (int.TryParse(instanceId.Substring(instanceId.LastIndexOf(".", System.StringComparison.Ordinal) + 1), out instanceIndex)) // On cloud.
            {
                int.TryParse(instanceId.Substring(instanceId.LastIndexOf("_", System.StringComparison.Ordinal) + 1), out instanceIndex); // On compute emulator.
            }
            return instanceIndex;
        }
    }
}
