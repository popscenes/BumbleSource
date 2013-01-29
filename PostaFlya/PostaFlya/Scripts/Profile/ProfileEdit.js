(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.ProfileEditViewModel = function (locationSelector, imageSelector) {
        var self = this;
        self.locationSelector = locationSelector;
        self.imageSelector = imageSelector;
        self.apiUrl = sprintf("/api/Browser/%s/MyDetails", bf.currentBrowserInstance.BrowserId);

        self.CreateFlierInstance = bf.globalCreateFlierInstance;
        //self.locationSelector.ShowMap();

        self.ErrorHandler = ko.observable(null);

        self.ImageSelectorVisible = ko.observable(false);

        self.showImageSelect = function() {
            self.ImageSelectorVisible(!self.ImageSelectorVisible());
        };
        
        self.update = function () {
            if (!$('#profileeditform').valid()) {
                return false;
            }

            //self.Address(self.locationSelector.currentLocation());
            self.AvatarImageId(self.imageSelector.selectedImageId());

            var reqdata = ko.mapping.toJS(self);
            if (self.locationSelector.ValidLocation())
                reqdata.Address = ko.mapping.toJS(self.locationSelector.currentLocation());
            else
                reqdata.Address = null;

            $.ajax(self.apiUrl, {
                data: ko.utils.stringifyJson(reqdata),
                type: "put", contentType: "application/json",
                success: function (result) {
                    if (self.afterUpdateCallback != undefined)
                        self.afterUpdateCallback();
                },
                error: function (jqXhr, textStatus, errorThrown) {
                    bf.ErrorUtil.HandleSubmitError('#profileeditform', jqXhr, self.ErrorHandler);
                }
            });

            return false;
        };

        self.load = function () {


            
            $.ajax(self.apiUrl, {
                type: "get", contentType: "application/json",
                success: function (result) {
                    ko.mapping.fromJS(result, {}, self);
                    
                    ko.applyBindings(self);
                    
                    self.imageSelector.Init();
                    self.imageSelector.selectedImageId(self.AvatarImageId());
                    
                    self.locationSelector.Init();
                    self.locationSelector.ShowMap();
                    
                    if (!ko.isObservable(self.Address)) {
                        //self.locationSelector.currentLocation(ko.mapping.toJS(self.Address));
                        self.locationSelector.currentLocation(self.Address);
                        
                    } else {
                        self.locationSelector.currentLocation(new bf.LocationModel());
                    }
                    
                    
                    
                    
                    

                }
            });
        };

        self.load();

    };


})(window);