using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using Website.Common.Extension;

namespace PostaFlya.Views.Flier
{
    public class Bundles
    {
        //js
        private static readonly string[]
            CoreJs = {
                        "Bulletin/BulletinLayoutProperties.js",
                        "Flier/Flier.js",
                        "Base/CreateEditFlier.js"
                   };

        private static readonly string[] 
            MobileJs = {
                        "Flier/Mobile/Flier.js"
                    };

        private static readonly string[] 
            DeskJs =  {
                       "Flier/Desk/Flier.js"
                    };

        //structure
        private static readonly string[]
            CoreStructureCss = {
                        "Fliers/Fliers.css"
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
            table.AddStructureCssCollection("Flier/MobileCss.css", CoreStructureCss, MobileStructureCss);
            table.AddStructureCssCollection("Flier/DeskCss.css", CoreStructureCss, DeskStructureCss);

            //themes
            table.AddThemedCssCollection("Flier/MobileCss.css", Shared.Bundles.Themes,
                CoreThemeCss, MobileThemeCss);
            table.AddThemedCssCollection("Flier/DeskCss.css", Shared.Bundles.Themes,
                CoreThemeCss, DeskThemeCss);

        }

        private static void AddJsBundles(BundleCollection table)
        {
            var scriptBundleMobile = new ScriptBundle("~/Script/FlierMobileJs.js");
            scriptBundleMobile.AddScriptFiles(CoreJs);
            scriptBundleMobile.AddScriptFiles(MobileJs);
            table.Add(scriptBundleMobile);

            var scriptBundleDesk = new ScriptBundle("~/Script/FlierDeskJs.js");
            scriptBundleDesk.AddScriptFiles(CoreJs);
            scriptBundleDesk.AddScriptFiles(DeskJs);
            table.Add(scriptBundleDesk);            
        }
    }
}