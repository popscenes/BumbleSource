(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.DefaultBehaviourViewModel = function (data) {
        var self = this;
        
        

        var mapping = {
            'copy': ["Flier"],
            'ContactDetails': {
                create: function (options) {                
                    return ko.observable(options.data == null ? null : new bf.ContactDetailsModel(options.data));
                }
            }
            
        };
        
        ko.mapping.fromJS(data, mapping, self);
        
        self.IsPeeling = ko.observable(false);

        self.TearOff = function() {

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
                        }).always(function () { self.IsPeeling(false);});
                    
                },
                error: function (jqXhr, textStatus, errorThrown) {
                    bf.ErrorUtil.HandleRequestError(null, jqXhr);
                }
            }).fail(function () { self.IsPeeling(false); });
            return true;
        };

    };


})(window);