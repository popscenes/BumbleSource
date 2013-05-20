/**/
(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};

    bf.VenueInformationModel = function (data) {

        var defaults = {
            PhoneNumber: null,
            EmailAddress: null,
            FirstName: null,
            MiddleNames: null,
            Surname: null,
            WebSite: null,
            Address: {},
            Source: null,
            SourceId: null,
            SourceUrl: null,
            PlaceName: null,
            BoardFriendlyId: null,
            UtcOffset: null,
        };

        data = $.extend(defaults, data);
        var self = this;

        var mapping = {
            'Address': {
                create: function (options) {
                    return ko.observable(new bf.LocationModel(options.data));
                }
            }
        };

        ko.mapping.fromJS(data, mapping, self);
        
        self.Description = ko.computed(function () {
            var addDesc = '';
            
            if (self.PlaceName()) {
                return self.PlaceName() + ' (' + self.Address().Street() + ' ' + self.Address().Locality() + ')'
            } else {
                return self.Address().LocalDescription();
            }

        });

    };



})(window, JQuery);