/**/
(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};

    bf.LocationModel = function (initData) {
        var defaults = {
            Longitude:-300,
            Latitude: -300,
            StreetNumber: '',
            Street:'',
            Locality:'',
            Region:'',
            PostCode:'',
            CountryName: '',
        };
        initData = $.extend(defaults, initData);
        
        var self = this;
        
        var mapping = {
            'ignore': ["Description"]
        };
        ko.mapping.fromJS(initData, mapping, self);
        
        self.ValidLocation = function () {
            return !(self.Longitude() < -180
                || self.Longitude() > 180
                || self.Latitude() < -90
                || self.Latitude() > 90);
        };

        self.ValidLocationForValidation = function() {
            if (self.ValidLocation()) {
                return "valid";
            } else {
                return "";
            }
        };


        self.SetFromGeo = function (results) {

            self.Longitude(results.geometry.location.lng());
            self.Latitude(results.geometry.location.lat());
            
            var data = results.address_components;
            for (var i = 0; i < data.length; i++) {
                var addressPart = data[i];
                if ($.inArray('street_number', addressPart.types) >= 0) {
                    self.StreetNumber(addressPart.long_name);
                }
                else if ($.inArray('route', addressPart.types) >= 0) {
                    self.Street(addressPart.long_name);
                }
                else if ($.inArray('locality', addressPart.types) >= 0) {
                    self.Locality(addressPart.long_name);
                }
                else if ($.inArray('administrative_area_level_1', addressPart.types) >= 0) {
                    self.Region(addressPart.long_name);
                }
                else if ($.inArray('country', addressPart.types) >= 0) {
                    self.CountryName(addressPart.long_name);
                }
                else if ($.inArray('postal_code', addressPart.types) >= 0) {
                    self.PostCode(addressPart.long_name);
                }
            }
        };
        
        self.AddAddressPart = function (addressPart, current, separator) {
            if (!addressPart || addressPart.length == 0)
                return current;
            return current + ((current.length > 0) ? separator + addressPart : addressPart);
        };

        self.Description = ko.computed(function () {
            var addDesc = '';

            addDesc = self.AddAddressPart(self.StreetNumber(), addDesc, ', ');
            addDesc = self.AddAddressPart(self.Street(), addDesc, ' ');
            addDesc = self.AddAddressPart(self.Locality(), addDesc, ', ');
            addDesc = self.AddAddressPart(self.Region(), addDesc, ' ');
            addDesc = self.AddAddressPart(self.PostCode(), addDesc, ' ');
            return self.AddAddressPart(self.CountryName(), addDesc, ', ');
        });
        
        self.LocalDescription = ko.computed(function () {
            var addDesc = '';

            addDesc = self.AddAddressPart(self.StreetNumber(), addDesc, ', ');
            addDesc = self.AddAddressPart(self.Street(), addDesc, ' ');
            return self.AddAddressPart(self.Locality(), addDesc, ', ');
        });
        
        self.SearchLink = ko.computed(function () {
            return "http://maps.google.com/?q=" + self.Description();
        });



    };



})(window, JQuery);
/**/