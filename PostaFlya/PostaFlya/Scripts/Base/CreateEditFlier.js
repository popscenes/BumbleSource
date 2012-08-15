(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.CreateEditFlier = function (data, locationSelector, imageSelector, tagsSelector, afterUpdateCallback) {

        var self = this;
        self.apiUrl = sprintf("/api/Browser/%s/MyFliers", bf.currentBrowserInstance.BrowserId);
        self.Steps = ['Flyer', 'Location', 'Info', 'Tags', 'Images'];

        ko.mapping.fromJS(data, {}, this);

        self.locationSelector = locationSelector;
        self.imageSelector = imageSelector;
        self.afterUpdateCallback = afterUpdateCallback;
        self.tagsSelector = tagsSelector;
        self.currentStep = ko.observable(0);

        //self.FlierImageUrl = ko.observable();

        self.FlierImageUrlH250 = ko.computed(
        {
            read: function () {
                if (self.FlierImageUrl() == undefined)
                    return '';
                else {
                    var newUrl = self.FlierImageUrl().replace('v100', 'h250');
                    return newUrl;
                }
            }
        });

        self.SetFlierImage = function (image) {
            if (self.Steps[self.currentStep()] == 'Flyer') {
                self.FlierImageId(image.ImageId);
                self.FlierImageUrl(image.ImageUrl);
            }

            self.ImageList.push(image);
        };

        self.imageSelector.SetCallback(self.SetFlierImage);

        if (self.TagsString() != null && self.TagsString() != '') {
            self.tagsSelector.SelectedTags(self.TagsString().split(","));
        }

        self.nextTemplate = function () {
            if (!$('#flierForm').valid()) {
                return false;
            }

            if (self.currentStep() < self.Steps.length - 1) {
                self.currentStep(self.currentStep() + 1);
                if ($(".imageSelector").length > 0) {
                    self.imageSelector.Init();
                }
            }
        };

        self.prevTemplate = function () {
            if (self.currentStep() > 0) {
                self.currentStep(self.currentStep() - 1);
                if ($(".imageSelector").length > 0) {
                    self.imageSelector.Init();
                }
            }
        };

        self.stepTemplate = function () {
            return self.Steps[self.currentStep()] + '-template';
        };

        self.isNextStep = function () {
            return self.currentStep() < (self.Steps.length - 1);
        };

        self.isPrevStep = function () {
            return self.currentStep() > 0;
        };

        self.isUpdate = function () {
            if (self.Id() == null || self.Id() == undefined || self.Id() == '')
                return false;

            return true;
        };

        self.update = function () {
            if (!$('#flierForm').valid()) {
                return false;
            }

            var tagString = self.tagsSelector.SelectedTags().join();

            $.ajax(self.apiUrl, {
                data: ko.toJSON({ Id: self.Id, Title: self.Title, Description: self.Description,
                    Location: self.locationSelector.currentLocation(), TagsString: tagString,
                    FlierImageId: self.FlierImageId, FlierBehaviour: 0, EffectiveDate: self.EffectiveDate, ImageList: self.ImageList
                }),
                type: "put", contentType: "application/json",
                success: function (result) {
                    if (self.afterUpdateCallback != undefined)
                        self.afterUpdateCallback();
                }
            });

            return false;
        };

        self.save = function () {

            if (!$('#flierForm').valid()) {
                return false;
            }

            var tagString = self.tagsSelector.SelectedTags().join();

            $.ajax(self.apiUrl, {
                data: ko.toJSON({ Title: self.Title, Description: self.Description,
                    Location: self.locationSelector.currentLocation(), TagsString: tagString,
                    FlierImageId: self.FlierImageId, FlierBehaviour: 0, EffectiveDate: self.EffectiveDate, ImageList: self.ImageList
                }),
                type: "post", contentType: "application/json",
                success: function (result) {
                    if (self.afterUpdateCallback != undefined)
                        self.afterUpdateCallback();
                }
            });

            return false;
        };
    };

})(window);