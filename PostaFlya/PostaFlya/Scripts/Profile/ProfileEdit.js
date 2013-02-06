﻿(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.ProfileEditViewModel = function (imageSelector) {


        var self = this;
        self.imageSelector = imageSelector;
        self.apiUrl = sprintf("/api/Browser/%s/MyDetails", bf.currentBrowserInstance.BrowserId);

        self.CreateFlierInstance = bf.globalCreateFlierInstance;

        self.ErrorHandler = ko.observable(null);

        self.ImageSelectorVisible = ko.observable(false);

        self.showImageSelect = function() {
            self.ImageSelectorVisible(!self.ImageSelectorVisible());
            
            if (self.ImageSelectorVisible() == true) {
                self.imageSelector.Init();
            }
        };
        
        self.update = function () {
            if (!$('#profileeditform').valid()) {
                return false;
            }

            self.AvatarImageId(self.imageSelector.selectedImageId());

            var reqdata = ko.mapping.toJS(self);
            
            if (!self.Address().ValidLocation())
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

            var mapping = {
                'Address': {
                    create: function(options) {
                        return ko.observable(new bf.LocationModel(options.data));
                    }
                }
            };

            $.ajax(self.apiUrl, {
                type: "get", contentType: "application/json",
                success: function (result) {
                    
                    ko.mapping.fromJS(result, mapping, self);
                    
                    ko.applyBindings(self);
                    
                    self.imageSelector.Init();
                    self.imageSelector.selectedImageId(self.AvatarImageId());
                    
                }
            });
        };

        self.load();

    };


})(window);