﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PostaFlya.Domain.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PostaFlya.Domain.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to It will cost 500 credits to retrieve contact details for people that wish for you to contact them.
        /// </summary>
        internal static string LeadGenerationFeatureChargeBehaviour_CurrentStateMessage_Flier_LeadGenerationStatus {
            get {
                return ResourceManager.GetString("LeadGenerationFeatureChargeBehaviour_CurrentStateMessage_Flier_LeadGenerationStat" +
                        "us", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to People can send their contact details to you.
        /// </summary>
        internal static string LeadGenerationFeatureChargeBehaviour_Description {
            get {
                return ResourceManager.GetString("LeadGenerationFeatureChargeBehaviour_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Lead Contact Information is unavailable until paid for.
        /// </summary>
        internal static string LeadGenerationFeatureChargeBehaviour_LeadContactUnavailableUntilPaidFor {
            get {
                return ResourceManager.GetString("LeadGenerationFeatureChargeBehaviour_LeadContactUnavailableUntilPaidFor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your flier won&apos;t appear until it is paid for.
        /// </summary>
        internal static string PostRadiusFeatureChargeBehaviour_CurrentStateMessage_Unpaid {
            get {
                return ResourceManager.GetString("PostRadiusFeatureChargeBehaviour_CurrentStateMessage_Unpaid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Posting a flier to an area.
        /// </summary>
        internal static string PostRadiusFeatureChargeBehaviour_GetPostRadiusFeatureCharge_Description {
            get {
                return ResourceManager.GetString("PostRadiusFeatureChargeBehaviour_GetPostRadiusFeatureCharge_Description", resourceCulture);
            }
        }
    }
}
