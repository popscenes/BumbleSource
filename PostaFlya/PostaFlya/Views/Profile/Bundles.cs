using System.Web.Optimization;
using Website.Common.Extension;

namespace PostaFlya.Views.Profile
{
    public class Bundles
    {
        private static readonly string[]
            CoreJs = {
                        "Behaviour/Default/DefaultBehaviourViewModel.js",
                        "Base/GigGuideMixin.js",
                        "Behaviour/BehaviourViewModelFactory.js",
                        "Bulletin/SelectedFlierViewModel.js",
                        "Bulletin/BulletinLayoutProperties.js",
                        "Profile/PeeledPosted.js",
                        "Profile/ProfileEdit.js",
                        "Profile/PaymentPending.js",
                        "Payment/Payment.js",
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

            var scriptBundleDesk = new ScriptBundle("~/Script/Profile.js");
            scriptBundleDesk.AddScriptFiles(CoreJs);
            table.Add(scriptBundleDesk); 
        }
    }
}