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
        self.map = ko.observable(null);
        self.mapMarkers = [];
        self.mapCircles = [];

        self.currentLocation = ko.observable(new bf.LocationModel());
               
        self.locationType = ko.observable('current');
        self.showMain = ko.observable(options.displayInline);
        self.canGetCurrentLocation = ko.observable(true);
        self.updateCallback = null;

        self.toggleShowMain = function () {
            var showMain = !self.showMain();
            self.showMain(showMain);
            
            if (showMain) {
                self.updateMap();
            }
        };

        self.currentDistance = ko.observable(self.distanceSelector.currentDistance());
        self.savedLocations = ko.observableArray(bf.currentBrowserInstance.SavedLocations);

        self.ValidLocation = function() {
            return self.currentLocation() != null &&  self.currentLocation() != 'undefined' &&
                self.currentLocation().ValidLocation();
        };
        
        self.ShowMap = function () {
            var map = bf.createMap(self.mapElementId());
            self.map(map);
            bf.LocationSearchAutoComplete(self.locSearchId(), self.map(), self.currentLocation);
        };

        self.updateMap = function() {
            bf.SetMapPosition(self.map(), self.currentLocation().Longitude(), self.currentLocation().Latitude(), self.currentDistance(), self.mapMarkers, self.mapCircles);
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

            bf.getCurrentPosition(function (position, status) {
                if (status === 'OK') {
                    self.currentLocation(
                        new bf.LocationModel({ Longitude: position.coords.longitude, Latitude: position.coords.latitude })
                    );

                    bf.reverseGeocode(position.coords.latitude, position.coords.longitude, self.currentLocation());
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
            self.currentDistance(self.distanceSelector.currentDistance());
        };


        self.Init = function () {
            
            self.currentLocation.subscribe(function (newValue) {
                if (self.updateCallback != null) {
                    self.updateCallback();
                }
                self.updateMap();
            });
            
            self.currentDistance.subscribe(function (newValue) {
                if (self.updateCallback != null) {
                    self.updateCallback();
                }
                bf.setMapCircleDistance(self.mapCircles, newValue);
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