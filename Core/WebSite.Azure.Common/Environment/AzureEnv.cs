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
            if (IsRunningInCloud())
                return RoleEnvironment.CurrentRoleInstance.Id;

            return "devinstance";
        }
    }
}
