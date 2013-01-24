/**/
(function (window, undefined) {
    var bf = window.bf = window.bf || {};

    bf.CreateFlierInstance = function () {
        var self = this;
        self.CreateFlier = ko.observable();

        self.LocationSelector = new bf.LocationSelector({
            displayInline: true,
            mapElementId: 'creatre-flier-map',
            locSearchId: 'creatre-flier-loc'
        });


        self.ImageSelector = new bf.ImageSelector({
            uploaderElementId: "create-flier-uploader",
            imageListId: "create-flier-imageList"
        });


        self.TagsSelector = new bf.TagsSelector({displayInline: true});


        self.CreateFlierLaunch = function () {
            var emptyFlier = new bf.CreateEditFlier({ Id: '', Title: '', Description: '', TagsString: '', EffectiveDate: new Date(), FlierImageId: '', FlierImageUrl: '', ImageList: [], Location: {} },
                self.LocationSelector, self.ImageSelector, self.TagsSelector, self.FlierFormClose);

            self.CreateFlier(emptyFlier);
            self.ImageSelector.Init();
            self.TagsSelector.LoadTags();
            self.LocationSelector.Init();
            //self.locationSelectorCreateEdit.ShowMap();
        };

        self.InitialiseFlier = function (flier) {

            var editFlier = new bf.CreateEditFlier(flier, self.LocationSelector, self.ImageSelector, self.TagsSelector, self.FlierFormClose);
            self.CreateFlier(editFlier);

            self.ImageSelector.Init();
            self.ImageSelector.selectedImageId(editFlier.FlierImageId());
            self.LocationSelector.currentLocation(editFlier.Location);
            self.TagsSelector.LoadTags();
            self.LocationSelector.Init();
            //self.locationSelectorCreateEdit.ShowMap();
            if (!self.LocationSelector.ValidLocation() && editFlier.Location.Description() != "") {
                self.LocationSelector.errorMessage("Couldn't find " + editFlier.Location.Description() + " please search for location.");
            }
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