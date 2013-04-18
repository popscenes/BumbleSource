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
            "googleanalytics.js",
            "jquery-1.9.1.js",
            "jquery.unobtrusive-ajax.js",
            "jquery.validate.js",
            "jquery.validate.unobtrusive.js",
            "knockout-2.2.1.js",    
            "knockout.mapping-latest.js",
            "modernizr-2.6.2.js",
            "jquery.isotope.js",
            "jquery.isotope.centered.js",
            "jquery.imagesloaded.js", //this overrides the implementation in isotope
            "External/ExternalScriptInit.js",
            "jquery.endless-scroll.js",
            "plupload/plupload.js", 
            "plupload/plupload.html4.js", 
            "plupload/plupload.html5.js", 
            "plupload/jquery.plupload.queue/jquery.plupload.queue.js", 
            "jquery.bxSlider.js",
            "jquery.galleriffic.js",
            "sammy.js",
            "URI.js",
            "jquery.cookie.js", 
            "Extension/knockout-jquery-ui-widget.js",
 
            //ours
            "Extension/sprintf-0.7-beta1.js",
            "Extension/showdown.js",
            "Extension/knockout.custombindings.js",
            "Base/CurrentBrowser.js",
            "Base/LocationModel.js",
            "Base/ContactDetailsModel.js",
            "Base/VenuInformationModel.js",
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
            "Base/jquery.helptips.js",
            "Base/HelpTips.js",
            "Base/PageInit.js"
        };

        private static readonly string[] DeskJsFiles =
        {
            "jquery-ui-1.10.0.js",
            "jquery.ui.touch-punch.min.js",
            "jquery-ui-timepicker-addon.js"
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
            ////"jquery.ui.slider.css",
            //"jquery.ui.tabs.css",
            
            "jquery.ui.progressbar.css",
            //custom
            "DeskSite.css",
            "jquery.plupload.queue.css"
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
            //"jquery.ui.theme.css",
            //custom
            "jquery.ui.datepicker.css",
            "DeskSite.css",

        };


        public static void AddBundles(BundleCollection table)
        {
            AddJsBundles(table);
            AddCssBundles(table);
        }

        private static void AddCssBundles(BundleCollection table)
        {
            //Structure bundles
            BundleTable.Bundles.AddStructureCssCollection("DeskCss.css",
                CoreStructureCss, DeskStructureCss);


            //Themes bundles
            BundleTable.Bundles.AddThemedCssCollection("DeskCss.css", Themes,
                CoreThemeCss, DeskThemeCss);

        }

        private static void AddJsBundles(BundleCollection table)
        {
            var deskJsBundle = new ScriptBundle("~/Script/DeskJs.js");
            deskJsBundle.AddScriptFiles(CoreJsFiles);
            deskJsBundle.AddScriptFiles(DeskJsFiles);
            BundleTable.Bundles.Add(deskJsBundle);

        }
    }
}