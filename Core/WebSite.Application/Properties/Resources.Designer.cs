﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Website.Application.Properties {
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
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Website.Application.Properties.Resources", typeof(Resources).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Description Required.
        /// </summary>
        public static string FlierCreateModel_Description_Description_Required {
            get {
                return ResourceManager.GetString("FlierCreateModel_Description_Description_Required", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Give Your Flier A Location!.
        /// </summary>
        public static string FlierCreateModel_Location_Give_Your_Flier_A_Location_ {
            get {
                return ResourceManager.GetString("FlierCreateModel_Location_Give_Your_Flier_A_Location_", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Title Required.
        /// </summary>
        public static string FlierCreateModel_Title_Title_Required {
            get {
                return ResourceManager.GetString("FlierCreateModel_Title_Title_Required", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please Select An Image.
        /// </summary>
        public static string FlierImageIdRequired {
            get {
                return ResourceManager.GetString("FlierImageIdRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} can contain only letters, digits and underscores.
        /// </summary>
        public static string InvalidCharacters {
            get {
                return ResourceManager.GetString("InvalidCharacters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} has an invalid number of elements.
        /// </summary>
        public static string InvalidCount {
            get {
                return ResourceManager.GetString("InvalidCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is not a valid email address..
        /// </summary>
        public static string InvalidEmailAddress {
            get {
                return ResourceManager.GetString("InvalidEmailAddress", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is an invalid id.
        /// </summary>
        public static string InvalidGuid {
            get {
                return ResourceManager.GetString("InvalidGuid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is too long..
        /// </summary>
        public static string StringTooLarge {
            get {
                return ResourceManager.GetString("StringTooLarge", resourceCulture);
            }
        }
    }
}
