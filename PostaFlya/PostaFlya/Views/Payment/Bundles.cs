﻿using System.Web.Optimization;
using Website.Common.Extension;

namespace PostaFlya.Views.Payment
{
    public class Bundles
    {
        private static readonly string[]
            CoreJs = {
                        
                        "Payment/Payment.js"
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
            table.AddStructureCssCollection("Payment/DeskCss", CoreStructureCss);

            //themes
            table.AddThemedCssCollection("Payment/DeskCss", PostaFlya.Views.Shared.Bundles.Themes,
                CoreThemeCss);
        }

        private static void AddJsBundles(BundleCollection table)
        {
            var scriptBundleMobile = new ScriptBundle("~/Script/PaymentMobileJs");
            scriptBundleMobile.AddScriptFiles(CoreJs);
            table.Add(scriptBundleMobile);

            var scriptBundleDesk = new ScriptBundle("~/Script/PaymentDeskJs");
            scriptBundleDesk.AddScriptFiles(CoreJs);
            table.Add(scriptBundleDesk);
        }
    }
}