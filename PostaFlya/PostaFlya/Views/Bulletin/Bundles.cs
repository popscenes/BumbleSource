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

        private static readonly string[] 
            DeskJs =  {
                       "Bulletin/Desk/Bulletin.js"
                    };

        private static readonly string[]
            DetailDeskJs =  {
                       "Bulletin/Desk/Detail.js"
                    };


        public static void AddBundles(BundleCollection table)
        {
            AddJsBundles(table);
            AddCssBundles(table);
        }

        private static void AddCssBundles(BundleCollection table)
        {

        }

        private static void AddJsBundles(BundleCollection table)
        {
            //Bulletin
            var scriptBundleDesk = new ScriptBundle("~/Script/BulletinDeskJs.js");
            scriptBundleDesk.AddScriptFiles(CoreJs);
            scriptBundleDesk.AddScriptFiles(DeskJs);
            table.Add(scriptBundleDesk);

            //Detail
            var scriptDetBundleDesk = new ScriptBundle("~/Script/BulletinDetDeskJs.js");
            scriptDetBundleDesk.AddScriptFiles(CoreJs);
            scriptDetBundleDesk.AddScriptFiles(DetailDeskJs);
            table.Add(scriptDetBundleDesk); 
        }
    }
}