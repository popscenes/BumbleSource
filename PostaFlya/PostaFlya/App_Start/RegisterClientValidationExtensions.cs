using System.Diagnostics;
using DataAnnotationsExtensions.ClientValidation;
using Website.Azure.Common.Environment;

[assembly: WebActivator.PreApplicationStartMethod(typeof(PostaFlya.App_Start.RegisterClientValidationExtensions), "Start")]
 
namespace PostaFlya.App_Start {
    public static class RegisterClientValidationExtensions {
        public static void Start() {
            
            Trace.TraceInformation("RegisterClientValidationExtensions Start " + AzureEnv.GetIdForInstance());

            DataAnnotationsModelValidatorProviderExtensions.RegisterValidationExtensions();            
        }
    }
}