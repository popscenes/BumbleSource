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
            DeskJs =  {
                       "Profile/Desk/Profile.js"
                    };

        //Structure Css
        private static readonly string[]
            CoreStructureCss = {
                        //"Bulletin/Bulletin.css"
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
            table.AddStructureCssCollection("Profile/DeskCss.css", CoreStructureCss, DeskStructureCss);

            //themes
            table.AddThemedCssCollection("Profile/DeskCss.css", PostaFlya.Views.Shared.Bundles.Themes,
                CoreThemeCss, DeskThemeCss);
        }

        private static void AddJsBundles(BundleCollection table)
        {
            //Bulletin

            var scriptBundleDesk = new ScriptBundle("~/Script/ProfileDeskJs.js");
            scriptBundleDesk.AddScriptFiles(CoreJs);
            scriptBundleDesk.AddScriptFiles(DeskJs);
            table.Add(scriptBundleDesk); 
        }
    }
}