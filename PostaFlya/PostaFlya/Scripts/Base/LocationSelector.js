/**/
(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.LocationSelector = function (options) {
        var self = this;

        var defaults = {
            displayInline: false,
            mapElementId: 'map',
            locSearchId: 'locationSearch',
            bannerText: 'Please select a suburb or location'
        };

        options = $.extend(defaults, options);

        self.displayInline = ko.observable(options.displayInline);
        self.mapElementId = ko.observable(options.mapElementId);
        self.locSearchId = ko.observable(options.locSearchId);
        self.distanceSelector = new bf.DistanceSelector();
        self.bannerText = options.bannerText;

       
        self.errorMessage = ko.observable(null);
        self.map = ko.observable(null);
        self.mapMarkers = [];
        self.mapCircles = [];

        self.currentLocation = ko.observable(new bf.LocationModel());
        
        self.searchText = ko.computed(function () {
            if (self.currentLocation().Description() == "") {
                return self.bannerText;
            }
            return self.currentLocation().Description();
        });

        self.clearSearchText = function (data, event) {
            event.currentTarget.select();
        };

               
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

        self.TryInitMap = function() {
            if (!document.getElementById(self.mapElementId()))
                return;

            self.ShowMap();
            
            if (self.showMain()) {
                self.updateMap();
            }
        };

        self.updateMap = function () {
            if (!document.getElementById(self.mapElementId()))
                return;
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
                else{
                    self.canGetCurrentLocation(false);
                    self.locationType('search');
                    self.updateMap();
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
                bf.setMapCircleDistance(self.map(), self.mapCircles, newValue);
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