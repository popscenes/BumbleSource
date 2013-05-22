﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using Microsoft.Web.Helpers;
using Website.Common.Extension;

namespace PostaFlya.Views.Board
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
            "jquery.isotope.js",
            "jquery.isotope.centered.js",
            "jquery.imagesloaded.js", //this overrides the implementation in isotope
            "Extension/knockout-jquery-ui-widget.js",
            "jquery.endless-scroll.js",
 
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
            "Base/TagsSelector.js",
            "Base/ClaimsViewModel.js",
            "Base/TileLayoutViewModel.js",
            "Base/dateFilter.js"
        };

        private static readonly string[]
            BulletinJs = {
                        "Base/facebook.js",
                        "Bulletin/BoardWidgetPage.js",
                        "Behaviour/Default/DefaultBehaviourViewModel.js",
                        "Behaviour/BehaviourViewModelFactory.js",
                        "Bulletin/SelectedFlierViewModel.js",
                        "Bulletin/BulletinLayoutProperties.js",
                   };

        private static readonly string[] Css =
        {
            "jquery.isotope.css",
            "CommonSite.css",
            "DeskSite.css",          
        };

        private static readonly string[]ThemeCss =
        {
            "CommonSite.css",
            "DeskSite.css",

        };

        public static void AddBundles(BundleCollection table)
        {
            AddJsBundles(table);
            AddCssBundles(table);
        }

        private static void AddCssBundles(BundleCollection table)
        {

            var cssBundle = new StyleBundle("~/Content/BoardWidget.css");
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