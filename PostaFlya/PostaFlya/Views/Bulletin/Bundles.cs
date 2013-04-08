using System.Web.Optimization;
using Website.Common.Extension;

namespace PostaFlya.Views.Bulletin
{
    public class Bundles
    {
        private static readonly string[]
            CoreJs = {
                        "Base/facebook.js",
                         "Bulletin/Bulletin.js",
                        "Behaviour/Default/DefaultBehaviourViewModel.js",
                        "Behaviour/BehaviourViewModelFactory.js",
                        "Behaviour/BehaviourViewModel.js",
                        "Bulletin/SelectedFlierViewModel.js",
                        "Bulletin/BulletinLayoutProperties.js",
                        "Bulletin/Detail.js"
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
            table.Add(scriptBundleDesk);
        }
    }
}