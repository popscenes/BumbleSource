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

        options = $.extend(defaults, options);

        self.displayInline = ko.observable(options.displayInline);
        self.mapElementId = ko.observable(options.mapElementId);
        self.locSearchId = ko.observable(options.locSearchId);
        self.distanceSelector = new bf.DistanceSelector();

        self.errorMessage = ko.observable(null);

        self.currentLocation = ko.observable(new bf.LocationModel());
               
        self.locationType = ko.observable('current');
        self.showMain = ko.observable(options.displayInline);
        self.canGetCurrentLocation = ko.observable(true);
        self.updateCallback = null;

        self.toggleShowMain = function () {
            var showMain = !self.showMain();
            self.showMain(showMain);
            $('#' + self.mapElementId()).gmap('refresh');
        };

        self.currentDistance = ko.observable(self.distanceSelector.currentDistance);

        self.savedLocations = ko.observableArray(bf.currentBrowserInstance.SavedLocations);

        self.ValidLocation = function() {
            return self.currentLocation() != null &&  self.currentLocation() != 'undefined' &&
                self.currentLocation().ValidLocation();
        };
        
        self.ShowMap = function () {
            $('#' + self.mapElementId()).gmap().bind('init', function (ev, map) {

            });

            bf.LocationSearchAutoComplete($("#" + self.locSearchId()), $('#' + self.mapElementId()), self.currentLocation);
        };

        self.searchFromDescription = function (description) {
            $('#' + self.mapElementId()).gmap('search', { 'address': description },
            function (results, status) {
                if (status === 'OK') {
                    response($.map(results, function (item) {
                        self.currentLocation({ Description: item.formatted_address, Longitude: item.geometry.location.lng(), Latitude: item.geometry.location.lat() });
                    }));
                }
            });
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
                    
                    self.currentLocation(
                        new bf.LocationModel({ Longitude: position.coords.longitude, Latitude: position.coords.latitude })
                    );

                    var latlng = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);
                    $('#' + self.mapElementId()).gmap('search', { 'location': latlng },
                        function (results, status) {
                            if (status === 'OK') {
                                self.currentLocation().SetFromGeo(results[0]);
                            }
                        });

                    bf.SetMapPosition($('#' + self.mapElementId()), position.coords.longitude, position.coords.latitude);
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
        
        self.DistanceCallback = function () {
            self.currentDistance = ko.observable(self.distanceSelector.currentDistance);
            if (self.updateCallback != null) {
                self.updateCallback();
            }
        };


        self.Init = function () {
            
            self.currentLocation.subscribe(function (newValue) {
                if (self.updateCallback != null) {
                    self.updateCallback();
                }
            });
            
            self.distanceSelector.updateCallback = self.DistanceCallback;

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