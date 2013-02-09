using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using Website.Common.Extension;

namespace PostaFlya.Views.Bulletin
{
    public class Bundles
    {
        private static readonly string[]
            CoreJs = {
                        "Bulletin/Bulletin.js",
                        "Behaviour/Default/DefaultBehaviourViewModel.js",
                        "Behaviour/BehaviourViewModelFactory.js",
                        "Behaviour/BehaviourViewModel.js",
                        "Bulletin/SelectedFlierViewModel.js",
                        "Bulletin/BulletinLayoutProperties.js"
                   };

        //Get
        private static readonly string[] 
            MobileJs = {
                        "Bulletin/Mobile/Bulletin.js"
                    };

        private static readonly string[] 
            DeskJs =  {
                       "Bulletin/Desk/Bulletin.js"
                    };

        //Detail Get
        private static readonly string[]
            DetailMobileJs = {
                        "Bulletin/Mobile/Detail.js"
                    };

        private static readonly string[]
            DetailDeskJs =  {
                       "Bulletin/Desk/Detail.js"
                    };

        //Structure Css
        private static readonly string[]
            CoreStructureCss = {
                        "Bulletin/Bulletin.css"
                   };

        private static readonly string[]
            MobileStructureCss = {
                        "Bulletin/BulletinMobile.css"
                    };

        private static readonly string[]
            DeskStructureCss =  {
                       "Bulletin/BulletinDesk.css"
                    };

        //theme Css
        private static readonly string[]
            CoreThemeCss = {
                        "Bulletin/Bulletin.css"
                   };

        private static readonly string[]
            MobileThemeCss = {
                        "Bulletin/BulletinMobile.css"
                    };

        private static readonly string[]
            DeskThemeCss =  {
                       "Bulletin/BulletinDesk.css"
                    };

        public static void AddBundles(BundleCollection table)
        {
            AddJsBundles(table);
            AddCssBundles(table);
        }

        private static void AddCssBundles(BundleCollection table)
        {
            //structure
            table.AddStructureCssCollection("Bulletin/MobileCss.css", CoreStructureCss, MobileStructureCss);
            table.AddStructureCssCollection("Bulletin/DeskCss.css", CoreStructureCss, DeskStructureCss);

            //themes
            table.AddThemedCssCollection("Bulletin/MobileCss.css", PostaFlya.Views.Shared.Bundles.Themes,
                CoreThemeCss, MobileThemeCss);
            table.AddThemedCssCollection("Bulletin/DeskCss.css", PostaFlya.Views.Shared.Bundles.Themes,
                CoreThemeCss, DeskThemeCss);
        }

        private static void AddJsBundles(BundleCollection table)
        {
            //Bulletin
            var scriptBundleMobile = new ScriptBundle("~/Script/BulletinMobileJs.js");
            scriptBundleMobile.AddScriptFiles(CoreJs);
            scriptBundleMobile.AddScriptFiles(MobileJs);
            table.Add(scriptBundleMobile);

            var scriptBundleDesk = new ScriptBundle("~/Script/BulletinDeskJs.js");
            scriptBundleDesk.AddScriptFiles(CoreJs);
            scriptBundleDesk.AddScriptFiles(DeskJs);
            table.Add(scriptBundleDesk);

            //Detail
            var scriptDetBundleMobile = new ScriptBundle("~/Script/BulletinDetMobileJs.js");
            scriptDetBundleMobile.AddScriptFiles(CoreJs);
            scriptDetBundleMobile.AddScriptFiles(DetailMobileJs);
            table.Add(scriptDetBundleMobile);

            var scriptDetBundleDesk = new ScriptBundle("~/Script/BulletinDetDeskJs.js");
            scriptDetBundleDesk.AddScriptFiles(CoreJs);
            scriptDetBundleDesk.AddScriptFiles(DetailDeskJs);
            table.Add(scriptDetBundleDesk); 
        }
    }
}