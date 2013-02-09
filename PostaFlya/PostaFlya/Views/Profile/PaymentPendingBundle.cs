using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using Website.Common.Extension;

namespace PostaFlya.Views.Profile
{
    public class PaymentPendingBundles
    {
        private static readonly string[]
            CoreJs = {
                        
                        "Profile/PaymentPending.js"
                   };
        //Structure Css
        private static readonly string[]
            CoreStructureCss = {
                        
                   };

        //theme Css
        private static readonly string[]
            CoreThemeCss = {
                   };


        public static void AddBundles(BundleCollection table)
        {
            AddJsBundles(table);
            AddCssBundles(table);
        }

        private static void AddCssBundles(BundleCollection table)
        {
            //structure
            table.AddStructureCssCollection("PaymentPending/DeskCss.css", CoreStructureCss);

            //themes
            table.AddThemedCssCollection("PaymentPending/DeskCss.css", PostaFlya.Views.Shared.Bundles.Themes,
                CoreThemeCss);
        }

        private static void AddJsBundles(BundleCollection table)
        {
            var scriptBundleMobile = new ScriptBundle("~/Script/PaymentPendingMobileJs.js");
            scriptBundleMobile.AddScriptFiles(CoreJs);
            table.Add(scriptBundleMobile);

            var scriptBundleDesk = new ScriptBundle("~/Script/PaymentPendingDeskJs.js");
            scriptBundleDesk.AddScriptFiles(CoreJs);
            table.Add(scriptBundleDesk);
        }
    }
}
