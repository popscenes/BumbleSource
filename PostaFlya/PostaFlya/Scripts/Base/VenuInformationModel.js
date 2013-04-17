/**/
(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.VenuInformationModel = function (data) {

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

    };



})(window);