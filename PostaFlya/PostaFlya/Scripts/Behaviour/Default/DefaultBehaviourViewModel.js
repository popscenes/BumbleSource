(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};
    
    bf.DefaultBehaviourViewModel = function (data) {
        var self = this;

        //mapping is buggy when source object is null
        data.VenueInformation = data.VenueInformation || {};
        var mapping = {
            'copy': ["Flier"],
            'VenueInformation': {
                update: function (options) {                               
                    return ko.observable(options.data == null || $.isEmptyObject(options.data) ? null : new bf.VenueInformationModel(options.data));
                }
            }
            
        };

        ko.mapping.fromJS(data, mapping, self);
        
        self.IsPeeling = ko.observable(false);
        
        self.PeelText = ko.computed(function () {
            if (bf.currentBrowserInstance && bf.currentBrowserInstance.IsOwner(self.Flier.BrowserId)) {
                return self.IsPeeling() ?
                    "Peeling" :
                    "Test Peel";
            } else {
                return self.IsPeeling() ? "Peeling" : "Peel Details";
            }

        }, self);

//        self.IsInFuture = ko.computed(function () {
//            
        //        }, self);

        self.facebook = function() {
            bf.postToFacebook(self.Flier.TinyUrl, self.Flier.FlierImageUrl, self.Flier.Title, self.Flier.Description, self.Flier.Title);
        };

        self.twitter = function() {
            var url = "https://twitter.com/intent/tweet?original_referer=&text=" + encodeURIComponent(self.Flier.Title) + "&tw_p=tweetbutton&url=" + encodeURIComponent(self.Flier.TinyUrl);
            
            return url;
        };

        self.TearOff = function() {

            if (self.IsPeeling())
                return false;
            
            var reqdata = ko.toJSON({
                ClaimEntity: 'Flier',
                EntityId: self.Flier.Id,
                BrowserId: bf.currentBrowserInstance.BrowserId
            });
            
            _gaq.push(['_trackEvent', 'detail', 'peel', 'start']);

            self.IsPeeling(true);
            $.ajax('/api/claim/', {
                data: reqdata,
                type: "post", contentType: "application/json",
                success: function (result) {
                    $.getJSON('/api/BulletinApi/' + self.Flier.FriendlyId
                        , function (newdata) {
                            ko.mapping.fromJS(newdata, self); 
                            _gaq.push(['_trackEvent', 'detail', 'peel', 'end']);
                        }).always(function() {
                            self.IsPeeling(false);                     
                        });                    
                }
            }).fail(function(jqXhr, textStatus, errorThrown) {
                self.IsPeeling(false);
                bf.ErrorUtil.HandleRequestError(null, jqXhr, function (err) {
                    if (err.Message == "Invalid Access") {
                        bf.pagedefaultaction.set('bulletin-detail', 'peel');
                    }
                });
            });
            return true;
        };

        self.Init = function() {

            if (!bf.pagedefaultaction) return;
            var act = bf.pagedefaultaction.get('bulletin-detail');
            if (act == 'peel') {
                self.TearOff();
            }
        };

        self.Init();

    };


})(window, jQuery);