using System.Web.Optimization;
using WebSite.Common.Extension;

namespace PostaFlya.Views.HeatMap
{
    public class Bundles
    {
        //js
        private static readonly string[]
            CoreJs = {
                         "HeatMap/heatmap-lib.js",
                         "HeatMap/heatmap-gmaps.js",
                         "HeatMap/HeatMap.js",           
                   };

        private static readonly string[]
            MobileJs = {
                        "HeatMap/Mobile/HeatMap.js"
                    };

        private static readonly string[]
            DeskJs =  {
                       "HeatMap/Desk/HeatMap.js"
                    };

        //structure
        private static readonly string[]
            CoreStructureCss = {
                        
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
            table.AddStructureCssCollection("HeatMap/MobileCss",
                CoreStructureCss, MobileStructureCss);
            table.AddStructureCssCollection("HeatMap/DeskCss",
                CoreStructureCss, DeskStructureCss);

            //themes
            table.AddThemedCssCollection("HeatMap/MobileCss", Shared.Bundles.Themes,
                CoreThemeCss, MobileThemeCss);
            table.AddThemedCssCollection("HeatMap/DeskCss", Shared.Bundles.Themes,
                CoreThemeCss, DeskThemeCss);

        }

        private static void AddJsBundles(BundleCollection table)
        {
            var scriptBundleMobile = new ScriptBundle("~/Script/HeatMap/MobileJs");
            scriptBundleMobile.AddScriptFiles(CoreJs);
            scriptBundleMobile.AddScriptFiles(MobileJs);
            table.Add(scriptBundleMobile);

            var scriptBundleDesk = new ScriptBundle("~/Script/HeatMap/DeskJs");
            scriptBundleDesk.AddScriptFiles(CoreJs);
            scriptBundleDesk.AddScriptFiles(DeskJs);
            table.Add(scriptBundleDesk);
        }
    }
}