/**/
(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.ContactDetailsModel = function (data) {

        var defaults = {
            PhoneNumber: null,
            EmailAddress: null,
            FirstName: null,
            MiddleNames: null,
            Surname: null,
            WebSite: null,
            Address: {}
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
/**/