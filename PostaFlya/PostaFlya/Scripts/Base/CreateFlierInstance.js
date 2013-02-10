/**/
(function (window, undefined) {
    var bf = window.bf = window.bf || {};

    bf.CreateFlierInstance = function () {
        var self = this;
        self.CreateFlier = ko.observable();

//        self.LocationSelector = new bf.LocationSelector({
//            displayInline: true,
//            mapElementId: 'creatre-flier-map',
//            locSearchId: 'creatre-flier-loc'
//        });
//        
//        self.AddressSelector = new bf.LocationSelector({
//            displayInline: false,
//            mapElementId: 'contact-address-map',
//            locSearchId: 'contact-address-loc'
//        });

        self.ImageSelector = new bf.ImageSelector({
            uploaderElementId: "create-flier-uploader",
            imageListId: "create-flier-imageList"
        });


        self.TagsSelector = new bf.TagsSelector({displayInline: true});


        self.CreateFlierLaunch = function () {
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
        
        self.FlierFormClose = function() {
            self.CreateFlier(null);
        };
    };

    $(function () {
        bf.globalCreateFlierInstance = new bf.CreateFlierInstance();
    });


})(window);
/**/