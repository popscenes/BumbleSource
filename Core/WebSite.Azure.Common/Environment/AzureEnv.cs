using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.ServiceRuntime;

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

                _usingRealTableStorage = IsRunningInProdFabric();
                return _usingRealTableStorage.Value;
            }
            set { _usingRealTableStorage = value; }
        }

        //not using anywhere yet but might as well know how to do it!
        public static bool IsRunningInDevFabric()
        {
            if (!IsRunningInCloud())
                return false;
            // easiest check: try translate deployment ID into guid
            Guid guidId;
            return !Guid.TryParse(RoleEnvironment.DeploymentId, out guidId);
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
             return RoleEnvironment.CurrentRoleInstance.Id;
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
