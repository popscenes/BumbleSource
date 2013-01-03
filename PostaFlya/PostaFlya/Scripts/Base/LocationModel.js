/**/
(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.LocationModel = function (initData) {
        var defaults = {
            Longitude:-300,
            Latitude:-300,
            Description:'',
            StreetAddress:'',
            Locality:'',
            Region:'',
            PostCode:'',
            CountryName:'' 
        };
        initData = $.extend(defaults, initData);
        
        var self = this;
        ko.mapping.fromJS(initData, {}, self);

        self.LatLong = function() {
            return {Latitude: self.Latitude(), Longitude: self.Longitude()};
        };
        
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
            var streetNo = '';
            var streetName = '';
            var data = results.address_components;
            for (var i = 0; i < data.length; i++) {
                var addressPart = data[i];
                if ($.inArray('street_number', addressPart.types) >= 0) {
                    streetNo = addressPart.long_name;
                }
                else if ($.inArray('route', addressPart.types) >= 0) {
                    streetName = addressPart.long_name;
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
            self.StreetAddress(streetNo + ' ' + streetName);
            self.Description(results.formatted_address);

        };

    };



})(window);
/**/