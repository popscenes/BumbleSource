
using System.Linq;
using System.Web.Optimization;

namespace Website.Common.Extension
{
    public static class BundleExtensions
    {

        #region Javascript Bundles

        public static void AddScriptFiles(this ScriptBundle bundle, string [] filenames)
        {
            bundle.Include(filenames.ToScriptPath());
        }

        public static string[] ToScriptPath(this string[] stylefiles)
        {
            return stylefiles.Select(s => "~/Scripts/" + s).ToArray();
        }
        
        #endregion

        #region Structure CSS bendles


        public static void AddStructureCssFiles(this StyleBundle bundle, params string[][] filenameGroups)
        {
            foreach (var filenames in filenameGroups)
            {
                bundle.Include(filenames.ToStructurePaths());
            }
        }

        public static string[] ToStructurePaths(this string[] stylefiles)
        {
            return stylefiles.Select(s => "~/Content/structure/" + s).ToArray();
        }

        public static void AddStructureCssCollection(this BundleCollection collection
            , string collectionName
            , params string[][] filenameCollections)
        {
            var cssBundle = new StyleBundle("~/Content/structure/" + collectionName);
            cssBundle.AddStructureCssFiles(filenameCollections);
            collection.Add(cssBundle);
        }

        #endregion

        #region Theme CSS bundles


        public static void AddThemedCssFiles(this StyleBundle bundle, string theme, params string[][] filenameGroups)
        {
            foreach (var filenames in filenameGroups)
            {
                bundle.Include(filenames.ToThemePaths(theme));
            }
        }

        public static void AddThemedCssCollection(this BundleCollection collection
            , string collectionName
            , string[] themes
            , params string[][] filenameCollections)
        {
            foreach (var theme in themes)
            {
                var cssBundle = new StyleBundle("~/Content/themes/" + theme + "/" + collectionName);
                cssBundle.AddThemedCssFiles(theme, filenameCollections);
                collection.Add(cssBundle);
            }
        }

        public static string[] ToThemePaths(this string[] stylefiles, string theme)
        {
            return stylefiles.Select(s => "~/Content/themes/" + theme + "/" + s).ToArray();
        }

        #endregion
    }
}