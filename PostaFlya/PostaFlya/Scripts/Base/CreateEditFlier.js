(function (window, undefined) {

    var bf = window.bf = window.bf || {};
    
    bf.FlierCostModel = function () {
        var self = this;
        self.credits = ko.observable(null);
        self.title = ko.observable(null);
        self.description = ko.observable(null);
    };

    bf.CreateEditFlier = function (data, imageSelector, tagsSelector, afterUpdateCallback) {

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
            ContactDetails: {},
            PostRadius: 5,
            FlierBehaviour: 'Default'
        };
        
        data = $.extend(defdata, data);

        var self = this;
        self.apiUrl = sprintf("/api/Browser/%s/MyFliers", bf.currentBrowserInstance.BrowserId);
        self.Steps = ['Flyer', 'Tags', 'Info', 'Location'];
        self.imageSelector = imageSelector;
        self.tagsSelector = tagsSelector;
        
        self.HelpTipPage = 'createflier';
        self.HelpTipGroups = 'image-upload,image-browse';

        var mapping = {
            'Location': {
                create: function(options) {
                    return ko.observable(new bf.LocationModel(options.data));
                }
            },   
            'ContactDetails': {
                create: function(options) {
                    return ko.observable(new bf.ContactDetailsModel(options.data));
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
            var distence = ko.utils.unwrapObservable(self.PostRadius());
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

        /*self.SetFlierImage = function (image) {
            if (self.Steps[self.currentStep()] == 'Flyer') {
                self.FlierImageId(image.ImageId);
                self.FlierImageUrl(image.ImageUrl);
            }

            self.ImageList.push(image);
        };*/
        
        self.imageSelector.selectedImage.subscribe(function(image) {
            if (self.Steps[self.currentStep()] == 'Flyer') {
                self.FlierImageId(image.ImageId);
                self.FlierImageUrl(image.ImageUrl);
            }

            self.ImageList.push(image);
        });

        self.imageSelector.SetCallback(self.SetFlierImage);

        if (self.TagsString() != null && self.TagsString() != '') {
            self.tagsSelector.SelectedTags(self.TagsString().split(","));
        }

        self.nextTemplate = function () {
            if (!$('#flierForm').valid()) {
                return;
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

            var reqData = ko.mapping.toJS(self);
            var tagString = self.tagsSelector.SelectedTags().join();
            reqData.TagsString = tagString;
            if (!self.ContactDetails().Address().ValidLocation())
                reqData.ContactDetails.Address = null;

            $.ajax(self.apiUrl, {
                data: ko.utils.stringifyJson(reqData),
                type: "put", contentType: "application/json",
                success: function (result) {
                    if (self.afterUpdateCallback != undefined)
                        self.afterUpdateCallback();
                }
            });

            return false;
        };

        self.save = function () {

            var validate = $('#flierForm').validate();
            if (!validate.form()) {
                return false;
            }

            $('#flierForm').trigger('reset.unobtrusiveValidation');
            
            var reqData = ko.mapping.toJS(self);
            var tagString = self.tagsSelector.SelectedTags().join();
            reqData.TagsString = tagString;
            if (!self.ContactDetails().Address().ValidLocation())
                reqData.ContactDetails.Address = null;

            $.ajax(self.apiUrl, {
                data: ko.utils.stringifyJson(reqData),
                type: "post", contentType: "application/json",
                success: function (result) {
                    if (result.Details[2].Message == "PaymentPending") {
                        window.location = "/profile/paymentpending";
                        return;
                    }
                    if (self.afterUpdateCallback != undefined)
                        self.afterUpdateCallback();
                    
                },
                error: function (jqXhr, textStatus, errorThrown) {

                    bf.ErrorUtil.HandleRequestError('#flierForm', jqXhr, self.ErrorHandler);
                }
            });

            return false;
        };

        self.InitControls = function() {

            self.imageSelector.Init();
            self.tagsSelector.LoadTags();
            
            if (self.FlierImageId())
                self.imageSelector.selectedImageId(self.FlierImageId());
            
            //bf.HelpTipsInstance.CheckFirstShowFor("createflier");
            
        };
    };

})(window);