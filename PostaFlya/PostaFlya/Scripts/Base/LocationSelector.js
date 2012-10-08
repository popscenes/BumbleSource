/**/
(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.LocationSelector = function (options) {
        var self = this;

        var defaults = {
            displayInline: false,
            mapElementId: 'map',
            locSearchId: 'locationSearch'
        };

        var options = $.extend(defaults, options);

        self.displayInline = ko.observable(options.displayInline);
        self.mapElementId = ko.observable(options.mapElementId);
        self.locSearchId = ko.observable(options.locSearchId);

        self.description = ko.observable('');
        self.longitude = ko.observable(-300);
        self.latitude = ko.observable(-300);
        self.locationType = ko.observable('current');
        self.showMain = ko.observable(options.displayInline);
        self.canGetCurrentLocation = ko.observable(true);
        self.updateCallback = null;

        self.currentLocation = ko.computed({
            read: function () {
                return { Description: self.description(), Longitude: self.longitude(), Latitude: self.latitude() };
            },
            write: function (value) {
                if (value != undefined) {
                    self.description(value.Description);
                    self.longitude(value.Longitude);
                    self.latitude(value.Latitude);

                    if (self.updateCallback != null) {
                        self.updateCallback();
                    }
                }


            },
            owner: self
        });

        self.toggleShowMain = function () {
            var showMain = !self.showMain();
            self.showMain(showMain);
            $('#' + self.mapElementId()).gmap('refresh');
        };

        self.savedLocations = ko.observableArray(bf.currentBrowserInstance.SavedLocations);

        self.ShowMap = function () {
            $('#' + self.mapElementId()).gmap().bind('init', function (ev, map) {
                // $('#map').autocomplete($("#locationSearch")[0]);
                if (!self.ValidLocation()) {
                    self.SetCurrentlocationFromGeoCode();
                }
            });

            LocationSearchAutoComplete($("#" + self.locSearchId()), $('#' + self.mapElementId()), self.currentLocation);
        };

        self.SaveCurrentLocation = function () {
            var url = sprintf('/api/Browser/%s/SavedLocations', bf.currentBrowserInstance.BrowserId);
            $.ajax(url, {
                data: ko.toJSON(self.currentLocation()),
                type: "post", contentType: "application/json",
                success: function (result) {
                    self.savedLocations.push(self.currentLocation());
                }
            });
        };

        self.SetCurrentlocationFromGeoCode = function () {
            self.locationType('current');

            $('#' + self.mapElementId()).gmap('getCurrentPosition', function (position, status) {

                if (status === 'OK') {
                    //self.locationType('current');
                    var latlng = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);
                    $('#' + self.mapElementId()).gmap('search', { 'location': latlng },
                        function (results, status) {
                            if (status === 'OK') {
                                if (results.length >= 3) {
                                    self.description(results[2].formatted_address);
                                }
                                else {
                                    self.description(results[0].formatted_address);
                                }
                            }
                        });

                    self.currentLocation({ Description: self.description(), Longitude: position.coords.longitude, Latitude: position.coords.latitude });
                    SetMapPosition($('#' + self.mapElementId()), position.coords.longitude, position.coords.latitude);
                    $('#' + self.mapElementId()).gmap('refresh');


                }
                else if (status == 'NOT_SUPPORTED') {
                    self.showMain(true);
                    self.canGetCurrentLocation(false);
                    self.locationType('search');
                }

            });

            return true;
        };

        self.ValidLocation = function () {
            return !(self.longitude() < -180
                || self.longitude() > 180
                || self.latitude() < -90
                || self.latitude() > 90);
        };

        self.Init = function () {
            if (bf.pageState !== undefined && bf.pageState.Location !== undefined) {
                self.longitude(bf.pageState.Location.Longitude);
                self.latitude(bf.pageState.Location.Latitude);
                self.description(bf.pageState.Location.Description);
            }
        };
        self.Init();
    };


})(window);
/**/