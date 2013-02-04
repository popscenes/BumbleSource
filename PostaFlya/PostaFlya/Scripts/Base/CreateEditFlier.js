﻿(function (window, undefined) {

    var bf = window.bf = window.bf || {};
    
    bf.FlierCostModel = function () {
        var self = this;
        self.credits = ko.observable(null);
        self.title = ko.observable(null);
        self.description = ko.observable(null);
    };

    bf.CreateEditFlier = function (data, locationSelector, imageSelector, tagsSelector, afterUpdateCallback, addressSelector) {

        var defdata = {
            Id: '',
            Title: '',
            Description: '',
            TagsString: '',
            EffectiveDate: new Date(),
            FlierImageId: '',
            FlierImageUrl: '',
            ImageList: [],
            Location: {},
            EnableAnalytics: false,
            AttachTearOffs: true,
            AttachSuppliedContactDetails: false,
            ContactDetails: {}            
        };
        
        data = $.extend(defdata, data);

        var self = this;
        self.apiUrl = sprintf("/api/Browser/%s/MyFliers", bf.currentBrowserInstance.BrowserId);
        self.Steps = ['Flyer', 'Location', 'Info', 'Tags'];
        self.locationSelectorCreateEdit = locationSelector;
        self.contactAddressSelector = addressSelector;
        self.imageSelector = imageSelector;
        self.tagsSelector = tagsSelector;

        var mapping = {
            'Location': {
                create: function(options) {
                    return new bf.LocationModel(options.data);
                }
            },   
            'ContactDetails': {
                create: function(options) {
                    return new bf.ContactDetailsModel(options.data, self.contactAddressSelector);
                }
            }
        };
        ko.mapping.fromJS(data, mapping, this);

        
        self.afterUpdateCallback = afterUpdateCallback;  
        self.currentStep = ko.observable(0);
        self.costBreakdown = ko.observable(false);

        self.showCostBreakdown = function() {
            self.costBreakdown(!self.costBreakdown());
        };


        self.flierCosts = ko.observableArray([]);
        
        self.radiusFlierCost = ko.computed(function() {
            var ratePerSqKm = 1;
            var distence = ko.utils.unwrapObservable(self.locationSelectorCreateEdit.currentDistance());
            var init = ((distence) * (distence) * 3.14 * ratePerSqKm);

            var model = new bf.FlierCostModel();
            model.credits(init + 5 - (init % 5));
            model.title("Flier Post Radius");
            model.description("This charge relates to the effective radius of your flier. The default distance is 5km, as you increase that disance the cost of your flier will increase.");
            return model;
        }, self);
        self.flierCosts.push(self.radiusFlierCost);

        self.analyticsCost = ko.computed(function () {
            
            var model = new bf.FlierCostModel();
            model.credits(self.EnableAnalytics() ? 500 : 0);
            model.title("Flier Analytics");
            model.description("This charge enables you to see how many people have viewed your flier, the location they viewed it and various other analytic details of flier views");
            return model;
            
        }, self);
        
        self.flierCosts.push(self.analyticsCost);

        self.totalCost = ko.computed(function() {
            var cost = 0;
            for (var i = 0; i < self.flierCosts().length; i++) {
                    cost += self.flierCosts()[i]().credits();
            }
            return cost;
        }, self);

        //self.FlierImageUrl = ko.observable();

        self.FlierImageUrlH250 = ko.computed(
        {
            read: function () {
                var newUrl = "";
                if (self.FlierImageUrl() == undefined)
                    return '';
                else {
                    if (self.FlierImageUrl().indexOf("v100") == -1) {
                        newUrl = self.FlierImageUrl().replace('.jpg', 'h250.jpg');
                    } else {
                        newUrl = self.FlierImageUrl().replace('v100', 'h250');
                    }
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
                
                self.TryInitMaps();

                if ($(".imageSelector").length > 0) {
                    self.imageSelector.Init();
                }
            }
        };

        self.TryInitMaps = function() {
            self.locationSelectorCreateEdit.TryInitMap();
            self.contactAddressSelector.TryInitMap();
        };

        self.prevTemplate = function () {
            if (self.currentStep() > 0) {
                self.currentStep(self.currentStep() - 1);
                self.TryInitMaps();
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
                    Location: ko.mapping.toJS(self.locationSelectorCreateEdit.currentLocation()), TagsString: tagString,
                    FlierImageId: self.FlierImageId(), FlierBehaviour: 0, EffectiveDate: self.EffectiveDate,
                    ImageList: self.ImageList, ExternalSource: self.ExternalSource, ExternalId: self.ExternalId, PostRadius: self.locationSelectorCreateEdit.currentDistance(),
                    EnableAnalytics: self.EnableAnalytics, AttachTearOffs: self.AttachTearOffs, AttachSuppliedContactDetails: self.AttachSuppliedContactDetails,
                    ContactDetails: ko.mapping.toJS(self.ContactDetails)
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
                    Location: self.locationSelectorCreateEdit.currentLocation(), TagsString: tagString,
                    FlierImageId: self.FlierImageId, FlierBehaviour: 0, EffectiveDate: self.EffectiveDate, ImageList: self.ImageList,
                    ExternalSource: self.ExternalSource, ExternalId: self.ExternalId, PostRadius: self.locationSelectorCreateEdit.currentDistance()
                }),
                type: "post", contentType: "application/json",
                success: function (result) {
                    if (result.Details[2].Message == "PaymentPending") {
                        window.location = "/profile/paymentpending";
                        return false;
                    }
                    if (self.afterUpdateCallback != undefined)
                        self.afterUpdateCallback();
                    
                }
            });

            return false;
        };

        self.InitControls = function() {
            self.locationSelectorCreateEdit.Init();
            self.contactAddressSelector.Init();
            self.imageSelector.Init();
            self.tagsSelector.LoadTags();
            
            if (self.FlierImageId())
                self.imageSelector.selectedImageId(self.FlierImageId());

            if (self.Location && self.Location.ValidLocation())
                self.locationSelectorCreateEdit.currentLocation(self.Location);
            
            if (self.ContactDetails.Address && self.ContactDetails.Address.ValidLocation())
                self.contactAddressSelector.currentLocation(self.ContactDetails.Address);

        };
    };

})(window);