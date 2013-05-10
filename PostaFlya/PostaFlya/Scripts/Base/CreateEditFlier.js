(function (window, undefined) {

    var bf = window.bf = window.bf || {};
    
    function formatCurrency(num, incdollarsign, showcents) {
        num = num.toString().replace(/\$|\,/g, '');
        if (isNaN(num)) num = "0";
        sign = (num == (num = Math.abs(num)));
        num = Math.floor(num * 100 + 0.50000000001);
        cents = num % 100;
        num = Math.floor(num / 100).toString();
        if (!showcents)
            return (((sign) ? '' : '-') + (incdollarsign ? '$' : '') + num);
        
        if (cents < 10) cents = "0" + cents;
        for (var i = 0; i < Math.floor((num.length - (1 + i)) / 3) ; i++)
            num = num.substring(0, num.length - (4 * i + 3)) + ',' + num.substring(num.length - (4 * i + 3));
        return (((sign) ? '' : '-') + (incdollarsign ? '$' : '') + num + '.' + cents);
    }


    bf.FlierCostModel = function () {
        var self = this;
        self.credits = ko.observable(null);
        self.title = ko.observable(null);
        self.description = ko.observable(null);
    };
    
    bf.UserLinkModel = function (type, text, link) {
        var self = this;
        self.Type = ko.observable(type);
        self.Text = ko.observable(text);
        self.Link = ko.observable(link);
    };

    bf.CreateEditFlier = function (data, imageSelector, tagsSelector, afterUpdateCallback) {

        var defdata = {
            Id: '',
            Title: '',
            Description: '',
            TagsString: '',
            EventDates: [],
            FlierImageId: '',
            FlierImageUrl: '',
            ImageList: [],
            Location: {},
            EnableAnalytics: false,
            VenueInformation: {},
            PostRadius: 5,
            FlierBehaviour: 'Default',
            TotalPaid: 0,
            UserLinks:[]
        };
        
        data = $.extend(defdata, data);

        var self = this;
        self.apiUrl = sprintf("/api/Browser/%s/MyFliers", bf.currentBrowserInstance.BrowserId);
        self.Steps = ['AddImages', 'DetailsAndTags', 'UserLinks', 'PostLocation', 'Summary', 'Complete'];


        self.UserLinkTypes = ko.observableArray([]);
        var facebookLinkType = new bf.UserLinkModel("Facebook", "Facebook", "Your Facebook link");
        self.UserLinkTypes.push(facebookLinkType);
        
        var twitterLinkType = new bf.UserLinkModel("Twitter", "Twitter", "Your Twitter link");
        self.UserLinkTypes.push(twitterLinkType);
        
        var ticketsLinkType = new bf.UserLinkModel("Tickets", "Get Tickets", "Your Tickets link");
        self.UserLinkTypes.push(ticketsLinkType);
        
        var wevbsiteLinkType = new bf.UserLinkModel("Website", "Your Website", "Your Website link");
        self.UserLinkTypes.push(wevbsiteLinkType);
        

        self.selectedLinkType = ko.observable(null);

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
            'VenueInformation': {
                create: function(options) {
                    return ko.observable(new bf.VenueInformationModel(options.data));
                }
            }
        };
        ko.mapping.fromJS(data, mapping, this);

        
        self.afterUpdateCallback = afterUpdateCallback;  
        self.currentStep = ko.observable(0);
        self.costBreakdown = ko.observable(false);

        self.posting = ko.observable(false);
        self.flierStatus = ko.observable('');

        self.userLinkTypeChange = function() {
            var newUserLink = new bf.UserLinkModel(self.selectedLinkType().Type(), self.selectedLinkType().Text(), self.selectedLinkType().Link());
            self.editableUserLink(newUserLink);
        };

        self.saveText = ko.computed(function () {
            return self.posting() == true ? "Saving" : "Save";
        }, self);
        
        self.updateText = ko.computed(function () {
            return self.posting() == true ? "Updating" : "Update";
        }, self);

        self.showCostBreakdown = function() {
            self.costBreakdown(!self.costBreakdown());
        };

        self.editableUserLink = ko.observable(new bf.UserLinkModel());
        self.editUserLinkMode = ko.observable(null);
            
        self.addUserLink = function() {
            if (!$('#flierForm').valid()) {
                return;
            }
            self.UserLinks.push(self.editableUserLink());
            self.editableUserLink(new bf.UserLinkModel(self.selectedLinkType().Type(), self.selectedLinkType().Text(), self.selectedLinkType().Link()));
        };
        
        self.editUserLinkSave = function () {
            if (!$('#flierForm').valid()) {
                return;
            }
            self.editUserLinkMode(null);
            self.editableUserLink(new bf.UserLinkModel(self.selectedLinkType().Type(), self.selectedLinkType().Text(), self.selectedLinkType().Link()));
        };

        self.removeUserLink = function() {
            self.UserLinks.remove(this);
        };
        
        self.editUserLink = function () {
            self.editableUserLink(this);
            self.editUserLinkMode(true);
        };

        self.reparseValidation = function() {
            var $form = $("#flierForm");

            // Unbind existing validation 
            $form.unbind();
            $form.data("validator", null);

            // Check document for changes 
            $.validator.unobtrusive.parse($form);

            // Re add validation with changes 
            $form.validate($form.data("unobtrusiveValidation").options);
            var validatorSettings = $.data($form[0], 'validator').settings;
            validatorSettings.ignore = ".ignore *";
        };


        self.flierCosts = ko.observableArray([]);
        
        self.radiusFlierCost = ko.computed(function() {
            var ratePerSqKm = 1;
            var distance = ko.utils.unwrapObservable(self.PostRadius());
            var init = ((distance) * (distance) * 3.14 * ratePerSqKm) * (5 / distance);

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
        
        self.featureCost = ko.computed(function () {
            var cost = 0;
            for (var i = 0; i < self.flierCosts().length; i++) {
                cost += self.flierCosts()[i]().credits();
            }
            
            return cost;
        }, self);
        
        self.featureCostFmt = ko.computed(function () {
            return formatCurrency(self.featureCost());
        }, self);
        
        self.totalCost = ko.computed(function () {
            var cost = 0;
            cost = self.featureCost() - self.TotalPaid();
            if (cost < 0)
                cost = 0;
            return formatCurrency(cost);
        }, self);

        self.FlierImageUrlH250 = ko.computed(
        {
            read: function () {
                var newUrl = "";
                if (self.FlierImageUrl() == undefined)
                    return '';
                else {
                    if (self.FlierImageUrl().indexOf("v100") == -1) {
                        newUrl = self.FlierImageUrl().replace('.jpg', 'h228.jpg');
                    } else {
                        newUrl = self.FlierImageUrl().replace('v100', 'h228');
                    }
                    return newUrl;
                }
            }
        });

        self.imageSelector.selectedImage.subscribe(function(image) {
            if (self.Steps[self.currentStep()] == 'AddImages') {
                self.FlierImageId(image.ImageId);
                self.FlierImageUrl(image.ImageUrl);
            }
        });

        if (self.TagsString() != null && self.TagsString() != '') {
            self.tagsSelector.SelectedTags(self.TagsString().split(","));
        }

        self.nextTemplate = function () {
            if (!$('#flierForm').valid() && $('#flierForm').attr("data-validate-on-next") != "false") {
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
            var ret = self.Steps[self.currentStep()] + '-template';
            self.trackEvent(self.Steps[self.currentStep()]);
            return ret;
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
            if (self.posting() == true) {
                return false;
            }

            if (!$('#flierForm').valid()) {
                return false;
            }

            var reqData = ko.mapping.toJS(self);
//            for (var i = 0; i < reqData.EventDates.length; i++) {
//                reqData.EventDates[i] = new Date(reqData.EventDates[i]).toISOString();
//            }
           
            var tagString = self.tagsSelector.SelectedTags().join();
            reqData.TagsString = tagString;
            if (!self.VenueInformation().Address().ValidLocation())
                reqData.VenueInformation.Address = null;

            self.posting(true);
            $.ajax(self.apiUrl, {
                data: ko.utils.stringifyJson(reqData),
                type: "put",
                contentType: "application/json",
                success: function(result) {
                    self.flierStatus(result.Details[2].Message);
                    self.posting(false);
                    self.nextTemplate();
                }
            }).fail(function (jqXhr, textStatus, errorThrown) {
                self.posting(false);
                    bf.ErrorUtil.HandleRequestError('#flierForm', jqXhr, self.ErrorHandler);
            });
            

            return false;
        };

        self.setDate = function(date) {
                self.EventDates.removeAll();
                self.EventDates.push(date);
        };

        self.save = function () {
            if (self.posting() == true) {
                return false;
            }

            var validate = $('#flierForm').validate();
            if (!validate.form()) {
                return false;
            }

            $('#flierForm').trigger('reset.unobtrusiveValidation');
            
            var reqData = ko.mapping.toJS(self);
//            for (var i = 0; i < reqData.EventDates.length; i++) {
//                reqData.EventDates[i] = new Date(reqData.EventDates[i]).toISOString();
//            }
           
            var tagString = self.tagsSelector.SelectedTags().join();
            reqData.TagsString = tagString;
            //if (!self.ContactDetails().Address().ValidLocation())
            //    reqData.ContactDetails.Address = null;

            self.posting(true);
            $.ajax(self.apiUrl, {
                data: ko.utils.stringifyJson(reqData),
                type: "post", contentType: "application/json",
                success: function (result) {
                    self.posting(false);
                    self.flierStatus(result.Details[2].Message);
                    self.nextTemplate();
                },
                error: function (jqXhr, textStatus, errorThrown) {
                    self.posting(false);
                    bf.ErrorUtil.HandleRequestError('#flierForm', jqXhr, self.ErrorHandler);
                }
            });

            return false;
        };

        self.finish = function() {

            if (self.flierStatus() == "PaymentPending") {
                window.location = "/profile/paymentpending";
                return;
            }
            if (self.afterUpdateCallback != undefined) {
                self.afterUpdateCallback(false);
            }
            
        };

        self.events = {};
        self.trackEvent = function (event) {

            event = event + "-new";
            if (!self.isUpdate() && !self.events[event]) 
                _gaq.push(['_trackEvent', 'createFlya', event, self.flierStatus()]);
            self.events[event] = true;
        };

        self.OnCancel = function() {
            self.trackEvent('Cancel');
        };

        self.InitControls = function() {

            self.imageSelector.Init();
            self.tagsSelector.LoadTags();
            
            if (self.FlierImageId())
                self.imageSelector.selectedImageId(self.FlierImageId());
            
        };
    };

})(window);