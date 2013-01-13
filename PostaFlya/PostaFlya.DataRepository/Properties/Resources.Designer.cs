﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PostaFlya.DataRepository.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PostaFlya.DataRepository.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to ;WITH sorted AS 
        ///( 
        ///    SELECT  Id, FriendlyId, CreateDate, ROW_NUMBER() OVER
        ///                        (ORDER BY {0}) AS RN 
        ///    FROM    FlierSearchRecord 
        ///) 
        ///SELECT top (@take) * 
        ///FROM sorted 
        ///WHERE RN &gt; (@skip).
        /// </summary>
        internal static string SqlAllOrderedBy {
            get {
                return ResourceManager.GetString("SqlAllOrderedBy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ;WITH sorted AS 
        ///( 
        ///	SELECT  bfr.*, 
        ///			ROW_NUMBER() OVER
        ///					 (ORDER BY {0}) AS RN 
        ///	FROM    BoardFlierSearchRecord bfr			
        ///	WHERE bfr.BoardId = @board AND bfr.BoardStatus = 2
        ///	{1}  
        ///) 
        ///SELECT  {2} *
        ///FROM sorted 
        ///WHERE RN &gt; (@skip).
        /// </summary>
        internal static string SqlSeachFliersByBoard {
            get {
                return ResourceManager.GetString("SqlSeachFliersByBoard", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ;WITH sorted AS 
        ///( 
        ///	SELECT  Location.STDistance(@loc) as Metres, fr.*, 
        ///			ROW_NUMBER() OVER
        ///					 (ORDER BY {0}) AS RN 
        ///	FROM    FlierSearchRecord fr
        ///	INNER JOIN BoardFlierLocationSearchRecord bfr
        ///	ON fr.Id = bfr.FlierId				
        ///	WHERE bfr.BoardId = @board AND bfr.BoardStatus = 2 AND
        ///	Location.STDistance(@loc) &lt;= @distance*1000 
        ///	{1}  
        ///) 
        ///SELECT  {2} * 
        ///FROM sorted 
        ///WHERE RN &gt; (@skip).
        /// </summary>
        internal static string SqlSearchFliersByBoardLocationTags {
            get {
                return ResourceManager.GetString("SqlSearchFliersByBoardLocationTags", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ;WITH sorted AS 
        ///            ( 
        ///                SELECT  Location.STDistance(@loc) as Metres, *, 
        ///                        ROW_NUMBER() OVER
        ///                                 (ORDER BY {0}) AS RN 
        ///                FROM    FlierSearchRecord 
        ///                WHERE Location.STDistance(@loc) &lt;= @distance*1000 
        ///            	        {1}  
        ///            ) 
        ///            SELECT  {2} * 
        ///            FROM sorted 
        ///            WHERE RN &gt; (@skip).
        /// </summary>
        internal static string SqlSearchFliersByLocationTags {
            get {
                return ResourceManager.GetString("SqlSearchFliersByLocationTags", resourceCulture);
            }
        }
    }
}
