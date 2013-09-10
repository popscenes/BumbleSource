using System.Web.Optimization;
using Website.Common.Extension;

namespace PostaFlya.Views.Board.Widget
{
    public static class WidgetBundles
    {

        public static readonly string[] Themes = { "taskflya" };

        private static readonly string[] JsFiles =
        {
            "jquery-{version}.js",
            "knockout-{version}.js",    
            "knockout.mapping-latest.js",
            "modernizr-{version}.js",
            "jquery.imagesloaded.js", //this overrides the implementation in isotope
            "Extension/knockout-jquery-ui-widget.js",
            "jquery.endless-scroll.js",
            "jquery.cookie.js", 
            "jquery-ui-{version}.js",
            "jquery.ui.touch-punch.min.js",
            "jquery-ui-timepicker-addon.js",
            "sammy.js",
            "linq.js",
 
            //ours
            "Extension/knockout.custombindings.js",
            "Base/LocationModel.js",
            "Base/ContactDetailsModel.js",
            "Base/VenueInformationModel.js",
            "Base/LocationBase.js",           
            "Base/LocationService.js",
            "Base/LocationSelector.js",
            "Base/ScrollToTop.js",
            "Base/DistanceSelector.js",
            "Base/BrowserViewModel.js",
            "Base/CommentsViewModel.js",
            "Base/ClaimsViewModel.js",
            "Base/dateFilter.js",
            "Base/ErrorHandling.js",
            "Base/PageDefaultAction.js",
            "Base/Canvas.js"
        };

        private static readonly string[]
            BulletinJs = {
                        "Base/facebook.js",
                        "Base/GigGuideMixin.js",
                        "Bulletin/BoardWidgetPage.js",
                        "Behaviour/Default/DefaultBehaviourViewModel.js",
                        "Behaviour/BehaviourViewModelFactory.js",
                        "Bulletin/SelectedFlierViewModel.js",
                        "Bulletin/BulletinLayoutProperties.js",
                   };

        private static readonly string[] Css =
        {
        };

        private static readonly string[]ThemeCss =
        {
            "jquery.ui.datepicker.css",
            "Widgets.css",
        };

        public static void AddBundles(BundleCollection table)
        {
            AddJsBundles(table);
            AddCssBundles(table);
        }

        private static void AddCssBundles(BundleCollection table)
        {

            var cssBundle = new StyleBundle("~/Content/themes/taskflya/BoardWidget.css");
            cssBundle.AddStructureCssFiles(Css);
            cssBundle.AddThemedCssFiles(Themes[0], ThemeCss);
            BundleTable.Bundles.Add(cssBundle);

        }

        private static void AddJsBundles(BundleCollection table)
        {
            var deskJsBundle = new ScriptBundle("~/Script/BoardWidget.js");
            deskJsBundle.AddScriptFiles(JsFiles);
            deskJsBundle.AddScriptFiles(BulletinJs);
            BundleTable.Bundles.Add(deskJsBundle);

        }
    }
}