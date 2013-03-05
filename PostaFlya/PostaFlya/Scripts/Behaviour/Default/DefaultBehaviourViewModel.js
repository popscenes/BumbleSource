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

        self.TearOff = function() {

            var reqdata = ko.toJSON({
                ClaimEntity: 'Flier',
                EntityId: self.Flier.Id,
                BrowserId: bf.currentBrowserInstance.BrowserId
            });

            $.ajax('/api/claim/', {
                data: reqdata,
                type: "post", contentType: "application/json",
                success: function (result) {
                    $.getJSON('/api/BulletinApi/' + self.Flier.Id
                        , function (newdata) {
                            ko.mapping.fromJS(newdata, self);
                        });
                },
                error: function (jqXhr, textStatus, errorThrown) {
                    bf.ErrorUtil.HandleRequestError(null, jqXhr);
                }
            });
            return true;
        };

    };


})(window);