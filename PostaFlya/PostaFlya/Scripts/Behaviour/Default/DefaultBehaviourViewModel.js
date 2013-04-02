﻿(function (window, undefined) {

    var bf = window.bf = window.bf || {};
    
    bf.DefaultBehaviourViewModel = function (data) {
        var self = this;

        //mapping is buggy when source object is null
        data.ContactDetails = data.ContactDetails || {};
        var mapping = {
            'copy': ["Flier"],
            'ContactDetails': {
                update: function (options) {                               
                    return ko.observable(options.data == null || $.isEmptyObject(options.data) ? null : new bf.ContactDetailsModel(options.data));
                }
            }
            
        };

        ko.mapping.fromJS(data, mapping, self);
        
        self.IsPeeling = ko.observable(false);

        self.EventDatesString = ko.computed(function () {
            var date = new Date(self.Flier.EffectiveDate).toDateString();
            return date;
        });
        
        
        self.PeelText = ko.computed(function () {
            if (bf.currentBrowserInstance.IsOwner(self.Flier.BrowserId)) {
                return self.IsPeeling() ?
                    "Peeling" :
                    "Test Peel";
            } else {
                return self.IsPeeling() ?
                    (self.ContactDetails() ? "Sending" : "Peeling" ):
                    (self.ContactDetails() ? "Resend Peel" : "Peel");
            }

        }, self);

//        self.IsInFuture = ko.computed(function () {
//            
//        }, self);

        self.TearOff = function() {

            if (self.IsPeeling())
                return false;
            
            var reqdata = ko.toJSON({
                ClaimEntity: 'Flier',
                EntityId: self.Flier.Id,
                BrowserId: bf.currentBrowserInstance.BrowserId
            });

            self.IsPeeling(true);
            $.ajax('/api/claim/', {
                data: reqdata,
                type: "post", contentType: "application/json",
                success: function (result) {
                    $.getJSON('/api/BulletinApi/' + self.Flier.FriendlyId
                        , function (newdata) {
                            ko.mapping.fromJS(newdata, self);                       
                        }).always(function() {
                            self.IsPeeling(false);
                        });
                    
                },
                error: function (jqXhr, textStatus, errorThrown) {
                    bf.ErrorUtil.HandleRequestError(null, jqXhr);
                }
            }).fail(function () { self.IsPeeling(false); });
            return true;
        };

    };


})(window);