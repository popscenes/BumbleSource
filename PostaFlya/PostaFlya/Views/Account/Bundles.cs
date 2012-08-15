using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using WebSite.Common.Extension;

namespace PostaFlya.Views.Account
{
    public class Bundles
    {
        private static readonly string[]
            CoreJs = {
                        "openid/openid-jquery.js",
                         "openid/openid-en.js"
                   };

        private static readonly string[]
            MobileJs = {
                        
                    };

        private static readonly string[]
            DeskJs =  {
                       
                    };

        //Structure Css
        private static readonly string[]
            CoreStructureCss = {
                        
                   };

        private static readonly string[]
            MobileStructureCss = {
                        
                    };

        private static readonly string[]
            DeskStructureCss =  {
                       
                    };

        //theme Css
        private static readonly string[]
            CoreThemeCss = {
                        "openid.css",
                        "openid-shadow.css"
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
            table.AddStructureCssCollection("Account/MobileCss", CoreStructureCss, MobileStructureCss);
            table.AddStructureCssCollection("Account/DeskCss", CoreStructureCss, DeskStructureCss);

            //themes
            table.AddThemedCssCollection("Account/MobileCss", PostaFlya.Views.Shared.Bundles.Themes,
                CoreThemeCss, MobileThemeCss);
            table.AddThemedCssCollection("Account/DeskCss", PostaFlya.Views.Shared.Bundles.Themes,
                CoreThemeCss, DeskThemeCss);
        }

        private static void AddJsBundles(BundleCollection table)
        {
            var scriptBundleMobile = new ScriptBundle("~/Script/AccountMobileJs");
            scriptBundleMobile.AddScriptFiles(CoreJs);
            scriptBundleMobile.AddScriptFiles(MobileJs);
            table.Add(scriptBundleMobile);

            var scriptBundleDesk = new ScriptBundle("~/Script/AccountDeskJs");
            scriptBundleDesk.AddScriptFiles(CoreJs);
            scriptBundleDesk.AddScriptFiles(DeskJs);
            table.Add(scriptBundleDesk);
        }
    }
}