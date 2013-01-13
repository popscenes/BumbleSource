using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using Website.Application.Util;

namespace Website.Application.Azure.Util
{
    public class AzureTempFileStorage : TempFileStorageInterface
    {
        public string GetTempPath()
        {            
            var fileUploadFolder = RoleEnvironment.GetLocalResource("TempFileStorage");
            if(fileUploadFolder == null)
                throw new ConfigurationErrorsException("Please define a Local resource for TempFileStorage"); 
            return fileUploadFolder.RootPath;
        }
    }
}
