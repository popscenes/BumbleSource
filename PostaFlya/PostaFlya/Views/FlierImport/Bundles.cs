using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using Website.Common.Extension;

namespace PostaFlya.Views.FlierImport
{
    public class Bundles
    {
        //js
        private static readonly string[]
            CoreJs = {
                        "Bulletin/BulletinLayoutProperties.js",
                        "FlierImport/FlierImport.js",
                        //"Base/CreateEditFlier.js",
                        //"Base/CreateFlierInstance.js"
                   };

        private static readonly string[]
            MobileJs = {
                        "FlierImport/Mobile/FlierImport.js"
                    };

        private static readonly string[]
            DeskJs =  {
                       "FlierImport/Desk/FlierImport.js"
                    };

        //structure
        private static readonly string[]
            CoreStructureCss = {
                        "FlierImport/FlierImport.css"
                   };

        private static readonly string[]
            MobileStructureCss = {
                        
                    };

        private static readonly string[]
            DeskStructureCss =  {
                       
                    };

        //theme
        private static readonly string[]
            CoreThemeCss = {
                        
                   };

        private static readonly string[]
            MobileThemeCss = {
                        
                    };

        private static readonly string[]
            DeskThemeCss =  {
                       
                    };


        public static void AddBundles(BundleCollection table)
        {
            AddJsBundles(table);
            AddCssBundles(table);
        }

        private static void AddCssBundles(BundleCollection table)
        {
            //structure
            table.AddStructureCssCollection("FlierImport/MobileCss.css", CoreStructureCss, MobileStructureCss);
            table.AddStructureCssCollection("FlierImport/DeskCss.css", CoreStructureCss, DeskStructureCss);

            //themes
            table.AddThemedCssCollection("FlierImport/MobileCss.css", Shared.Bundles.Themes,
                CoreThemeCss, MobileThemeCss);
            table.AddThemedCssCollection("FlierImport/DeskCss.css", Shared.Bundles.Themes,
                CoreThemeCss, DeskThemeCss);

        }

        private static void AddJsBundles(BundleCollection table)
        {
            var scriptBundleMobile = new ScriptBundle("~/Script/FlierImportMobileJs.js");
            scriptBundleMobile.AddScriptFiles(CoreJs);
            scriptBundleMobile.AddScriptFiles(MobileJs);
            table.Add(scriptBundleMobile);

            var scriptBundleDesk = new ScriptBundle("~/Script/FlierImportDeskJs.js");
            scriptBundleDesk.AddScriptFiles(CoreJs);
            scriptBundleDesk.AddScriptFiles(DeskJs);
            table.Add(scriptBundleDesk);
        }
    }
}