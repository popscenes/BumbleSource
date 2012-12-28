using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using Website.Common.Extension;

namespace PostaFlya.Views.Shared
{
    public static class Bundles
    {
        public static readonly string[] Themes = {"taskflya"};
        //javascript      
        private static readonly string[] CoreJsFiles =
        {
            "jquery-1.8.1.js",
            "jquery.unobtrusive-ajax.js",
            "jquery.validate.js",
            "jquery.validate.unobtrusive.js",
            "knockout-2.2.0.js",
//            "knockout-2.1.0.debug.js",          
            "knockout.mapping-latest.js",
//            "knockout.mapping-latest.debug.js",
            "modernizr-2.6.2.js",
            "jquery.isotope.js",
            "jquery.isotope.centered.js",
            "jquery.imagesloaded.js", //this overrides the implementation in isotope
            "External/ExternalScriptInit.js",
            "jquery.endless-scroll.js",
            "maps/jquery.ui.map.js",
            "maps/jquery.ui.map.extensions.js",            
            "maps/jquery.ui.map.microdata.js",            
            "maps/jquery.ui.map.microformat.js",            
            "maps/jquery.ui.map.overlays.js",  
            "maps/jquery.ui.map.rdfa.js",            
            "maps/jquery.ui.map.services.js",
            "plupload/plupload.js", 
            "plupload/plupload.html4.js", 
            "plupload/plupload.html5.js", 
            "plupload/jquery.plupload.queue/jquery.plupload.queue.js", 
            "jquery.bxSlider.js", 
            "sammy.js",
            "URI.js",
 
            //ours
            "Extension/sprintf-0.7-beta1.js",
            "Extension/showdown.js",
            "Extension/knockout.custombindings.js",
            "Base/CurrentBrowser.js",
            "Base/LocationModel.js",
            "Base/LocationBase.js",           
            "Base/LocationService.js",
            "Base/LocationSelector.js",
            "Base/ImageSelector.js",
            "Base/ScrollToTop.js",
            "Base/DistanceSelector.js",
            "Base/BrowserViewModel.js",
            "Base/CommentsViewModel.js",
            "Base/TagsSelector.js",
            "Base/ClaimsViewModel.js",
            "Base/TileLayoutViewModel.js",
            "Base/ErrorHandling.js",
            "Base/CreateEditFlier.js",
            "Base/CreateFlierInstance.js",
        };

        private static readonly string[] DeskJsFiles =
        {
            "jquery-ui-1.9-RC1.js",
        };

        private static readonly string[] MobileJsFiles =
        {
            "jquery.mobile-1.1.0.js",
        };

        //css
        //STRUCTURE FILES
        private static readonly string[] CoreStructureCss =
        {
            //isotope
            "jquery.isotope.css",
            //custom
            "CommonSite.css",
            
        };

        private static readonly string[] DeskStructureCss =
        {
            //jquery ui
            "jquery.ui.core.css",
            "jquery.ui.resizable.css",
            "jquery.ui.selectable.css",
            "jquery.ui.accordion.css",
            "jquery.ui.autocomplete.css",
            "jquery.ui.button.css",
            "jquery.ui.dialog.css",
            //"jquery.ui.slider.css",
            "jquery.ui.tabs.css",
            "jquery.ui.datepicker.css",
            "jquery.ui.progressbar.css",
            //custom
            "DeskSite.css",
            "jquery.plupload.queue.css"
        };

        private static readonly string[] MobileStructureCss =
        {
            //jquery mobile
            "jquery.mobile.structure-1.1.0.css",
            //custom
            "MobileSite.css"
        };

        //THEME FILES
        private static readonly string[] CoreThemeCss =
        {
            //custom
            "CommonSite.css",
        };

        private static readonly string[] DeskThemeCss =
        {
            //jquery ui
            "jquery.ui.theme.css",
            //custom
            "DeskSite.css",

        };

        private static readonly string[] MobileThemeCss =
        {
            //jquery mobile
            "jquery.mobile.theme-1.1.0.css",
            //custom
            "MobileSite.css"
        };


        public static void AddBundles(BundleCollection table)
        {
            AddJsBundles(table);
            AddCssBundles(table);
        }

        private static void AddCssBundles(BundleCollection table)
        {
            //Structure bundles
            BundleTable.Bundles.AddStructureCssCollection("DeskCss",
                CoreStructureCss, DeskStructureCss);

            BundleTable.Bundles.AddStructureCssCollection("MobileCss",
                CoreStructureCss, MobileStructureCss);

            //Themes bundles
            BundleTable.Bundles.AddThemedCssCollection("DeskCss", Themes,
                CoreThemeCss, DeskThemeCss);

            BundleTable.Bundles.AddThemedCssCollection("MobileCss", Themes,
                CoreThemeCss, MobileThemeCss);
        }

        private static void AddJsBundles(BundleCollection table)
        {
            var deskJsBundle = new ScriptBundle("~/Script/DeskJs");
            deskJsBundle.AddScriptFiles(CoreJsFiles);
            deskJsBundle.AddScriptFiles(DeskJsFiles);
            BundleTable.Bundles.Add(deskJsBundle);

            var mobJsBundle = new ScriptBundle("~/Script/MobileJs");
            mobJsBundle.AddScriptFiles(CoreJsFiles);
            mobJsBundle.AddScriptFiles(MobileJsFiles);
            BundleTable.Bundles.Add(mobJsBundle);
        }
    }
}