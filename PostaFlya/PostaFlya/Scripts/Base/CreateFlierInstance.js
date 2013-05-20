/**/
(function (window, $, undefined) {
    var bf = window.bf = window.bf || {};

    bf.CreateFlierInstance = function () {
        var self = this;
        self.CreateFlier = ko.observable();

        self.ImageSelector = new bf.ImageSelector({
            uploaderElementId: "create-flier-uploader",
            imageListId: "create-flier-imageList"
        });


        self.TagsSelector = new bf.TagsSelector({displayInline: true});


        self.CreateFlierLaunch = function () {

            if (!bf.currentBrowserInstance.IsParticipant())
                bf.pagedefaultaction.set('createflyainstance', 'create');
            if (bf.currentBrowserInstance.LoginNeeded())
                return;

            var data = { ContactDetails: bf.currentBrowserInstance.ContactDetails };
            var emptyFlier = new bf.CreateEditFlier(data, self.ImageSelector, self.TagsSelector, self.FlierFormClose);

            self.CreateFlier(emptyFlier);
            emptyFlier.InitControls();
        };

        self.InitialiseFlier = function (flier) {

            var editFlier = new bf.CreateEditFlier(flier, self.ImageSelector, self.TagsSelector, self.FlierFormClose);
            self.CreateFlier(editFlier);

            editFlier.InitControls();

        };



        self.EditFlierLaunch = function (flier, evnt) {
            


            self.apiUrl = sprintf("/api/Browser/%s/MyFliers", bf.currentBrowserInstance.BrowserId);
            $.ajax(self.apiUrl + "/" + flier.Id, {
                type: "get", contentType: "application/json",
                success: function (result) {
                    self.InitialiseFlier(result);
                }
            });
            evnt.stopImmediatePropagation();
            return false;
        };
        
        self.FlierFormClose = function (isCancel) {
            if (self.CreateFlier() != null && isCancel)
                self.CreateFlier().OnCancel();
            
            self.CreateFlier(null);
        };
    };

    $(function () {
        bf.globalCreateFlierInstance = new bf.CreateFlierInstance();
        var act = bf.pagedefaultaction.get('createflyainstance');
        if (act == 'create') {
            if (bf.currentBrowserInstance.IsParticipant()) {
                setTimeout(bf.globalCreateFlierInstance.CreateFlierLaunch, 1);
            }
            else {
                bf.pagedefaultaction.set('createflyainstance', 'create');
            }
        }
    });


})(window, JQuery);
/**/