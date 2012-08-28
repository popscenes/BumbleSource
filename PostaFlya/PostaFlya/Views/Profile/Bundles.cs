using System.Web.Optimization;
using Website.Common.Extension;

namespace PostaFlya.Views.Profile
{
    public class Bundles
    {
        private static readonly string[]
            CoreJs = {
                        "Behaviour/Default/DefaultBehaviourViewModel.js",
                        "Behaviour/BehaviourViewModelFactory.js",
                        "Bulletin/SelectedFlierViewModel.js",
                        "Bulletin/BulletinLayoutProperties.js",
                        "Profile/Profile.js",
                   };

        //Get
        private static readonly string[] 
            MobileJs = {
                        //"Profile/Mobile/Profile.js"
                    };

        private static readonly string[] 
            DeskJs =  {
                       "Profile/Desk/Profile.js"
                    };

        //Structure Css
        private static readonly string[]
            CoreStructureCss = {
                        //"Bulletin/Bulletin.css"
                   };

        private static readonly string[]
            MobileStructureCss = {
                        //"Bulletin/BulletinMobile.css"
                    };

        private static readonly string[]
            DeskStructureCss =  {
                       //"Bulletin/BulletinDesk.css"
                    };

        //theme Css
        private static readonly string[]
            CoreThemeCss = {
                        //"Bulletin/Bulletin.css"
                   };

        private static readonly string[]
            MobileThemeCss = {
                        //"Bulletin/BulletinMobile.css"
                    };

        private static readonly string[]
            DeskThemeCss =  {
                       //"Bulletin/BulletinDesk.css"
                    };

        public static void AddBundles(BundleCollection table)
        {
            AddJsBundles(table);
            AddCssBundles(table);
        }

        private static void AddCssBundles(BundleCollection table)
        {
            //structure
            table.AddStructureCssCollection("Profile/MobileCss", CoreStructureCss, MobileStructureCss);
            table.AddStructureCssCollection("Profile/DeskCss", CoreStructureCss, DeskStructureCss);

            //themes
            table.AddThemedCssCollection("Profile/MobileCss", PostaFlya.Views.Shared.Bundles.Themes,
                CoreThemeCss, MobileThemeCss);
            table.AddThemedCssCollection("Profile/DeskCss", PostaFlya.Views.Shared.Bundles.Themes,
                CoreThemeCss, DeskThemeCss);
        }

        private static void AddJsBundles(BundleCollection table)
        {
            //Bulletin
            var scriptBundleMobile = new ScriptBundle("~/Script/ProfileMobileJs");
            scriptBundleMobile.AddScriptFiles(CoreJs);
            scriptBundleMobile.AddScriptFiles(MobileJs);
            table.Add(scriptBundleMobile);

            var scriptBundleDesk = new ScriptBundle("~/Script/ProfileDeskJs");
            scriptBundleDesk.AddScriptFiles(CoreJs);
            scriptBundleDesk.AddScriptFiles(DeskJs);
            table.Add(scriptBundleDesk); 
        }
    }
}