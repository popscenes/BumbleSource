using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using Website.Common.Extension;

namespace PostaFlya.Views.Account
{
    public class Bundles
    {
        private static readonly string[]
            CoreJs = {
                        "openid/openid-jquery.js",
                         "openid/openid-en.js",
                         "Account/Account.js"
                   };

        private static readonly string[]
            DeskJs =  {
                       "Account/Desk/Account.js"
                    };

        //Structure Css
        private static readonly string[]
            CoreStructureCss = {
                        
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
            table.AddStructureCssCollection("Account/DeskCss.css", CoreStructureCss, DeskStructureCss);

            //themes
            table.AddThemedCssCollection("Account/DeskCss.css", PostaFlya.Views.Shared.Bundles.Themes,
                CoreThemeCss, DeskThemeCss);
        }

        private static void AddJsBundles(BundleCollection table)
        {

            var scriptBundleDesk = new ScriptBundle("~/Script/AccountDeskJs.js");
            scriptBundleDesk.AddScriptFiles(CoreJs);
            scriptBundleDesk.AddScriptFiles(DeskJs);
            table.Add(scriptBundleDesk);
        }
    }
}