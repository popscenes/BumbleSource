/**/
(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.ContactDetailsModel = function (data, contactAddressSelector) {

        var defaults = {
            PhoneNumber: '',
            EmailAddress: '',
            FirstName: '',
            MiddleNames: '',
            Surname: '',
            WebSite: '',
            Address: {}
        };
        
        data = $.extend(defaults, data);
        var self = this;

        self.contactAddressSelector = contactAddressSelector;
        
        var mapping = {
            'Address': {
                create: function (options) {
                    return new bf.LocationModel(options.data);
                }
            }
        };  
        
        ko.mapping.fromJS(data, mapping, this);

    };



})(window);
/**/