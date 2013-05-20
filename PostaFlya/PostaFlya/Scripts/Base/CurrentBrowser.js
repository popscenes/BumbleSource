/**/
(function (window, $, undefined) {
    var bf = window.bf = window.bf || {};

    bf.CurrentBrowser = function () {
        var self = this;
        var browserObj = jQuery.parseJSON($("#browserInfo").val());
        $.extend(self, browserObj);
        //if we need observables
        //ko.mapping.fromJS(browserObj, {}, this);

        self.IsParticipant = function () {
            return self.IsInRole('Participant');
        };

        self.IsInRole = function (role) {
            return ($.inArray(role, self.Roles) >= 0);
        };
        
        self.IsOwner = function (browserId) {
            return self.BrowserId == browserId;
        };

        self.LoginNeeded = function(url) {
            if (!self.IsParticipant()) {
                bf.ErrorUtil.NeedsLogin();
                return true;
            }
            return false;
        };
    };

    $(function () {
        bf.currentBrowserInstance = new bf.CurrentBrowser();
    });
    

})(window, JQuery);
/**/