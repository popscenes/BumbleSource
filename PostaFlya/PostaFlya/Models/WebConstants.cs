using System;
using System.Diagnostics;

namespace PostaFlya.Models
{
    public static class WebConstants
    {
        public static string GetNavigationItemClass(this PageModelInterface model, string navigationItem)
        {
            return navigationItem.Equals(!string.IsNullOrWhiteSpace(model.ActiveNav) ? model.ActiveNav : model.PageId)
                       ? ActiveNavigationItem
                       : InActiveNavigationItem;
        }

        public const string ActiveNavigationItem = "active";
        public const string InActiveNavigationItem = "";

        #region flierPages

        public const string FlierImportPage = "flier-import";
        #endregion

        #region ProfilePages

        public const string ProfileEditPage = "profile-edit";
        public const string ProfilePeeledPage = "profile-peeled";
        public const string ProfilePostedPage = "profile-posted";
        public const string ProfilePaymentPage = "profile-payment";
        public const string ProfileCreditPage = "profile-credit";
        public const string ProfileCreditAddedPage = "profile-creditadded";
        public const string ProfileTransactionPage = "profile-transaction";

        public const string ProfileNavEdit = ProfileEditPage;
        public const string ProfileNavPeeled = ProfilePeeledPage;
        public const string ProfileNavPosted = ProfilePostedPage;
        public const string ProfileNavPayment = ProfilePaymentPage;
        

        #endregion

        #region BulletinPages

        public const string BulletinBoardPage = "bulletin-board";
        public const string BulletinDetailPage = "bulletin-detail";

        #endregion

        #region AccountPages
        public const string AccountLoginPage = "login-page";
        #endregion

        //TODO get rid of this

        public static void SetViewBagForLocationPicker(dynamic viewBag)
        {
            if (string.IsNullOrWhiteSpace(viewBag.VisibleTrigger))
                viewBag.VisibleTrigger = "ShowMap";
            if (string.IsNullOrWhiteSpace(viewBag.DistanceVariable))
                viewBag.DistanceVariable = "Distance";
            if (string.IsNullOrWhiteSpace(viewBag.LocationVariable))
                viewBag.LocationVariable = "Location";
            if (string.IsNullOrWhiteSpace(viewBag.AutoCompleteLocationBannerText))
                viewBag.AutoCompleteLocationBannerText = "'Search Your Local Area for Flyas...'";
            if (string.IsNullOrWhiteSpace(viewBag.AutoCompleteLocationId))
                viewBag.AutoCompleteLocationId = "locationSearch";
            if (string.IsNullOrWhiteSpace(viewBag.AutoCompleteBinding))
                viewBag.AutoCompleteBinding = "locationAutoComplete";

        }


    }
}