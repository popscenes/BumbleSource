/**/
(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.LocationService = function (track) {
        var self = this;

        self.currentLocation = null;
        self.updateCallback = null;
        self.track = track;
        self.IsAsking = ko.observable(false);

        self.StartTrackingLocation = function (callback) {
            self.updateCallback = callback;
            self.Start();
        };

        self.Start = function () {
            if (navigator.geolocation) {
                // Locate position
                self.IsAsking(true);
                navigator.geolocation.getCurrentPosition(self.Success, self.Err);
            } else {
                Err(null);
            }
        };

        self.Success = function (loc) {
            self.IsAsking(false);
            self.currentLocation = loc;
            if (self.updateCallback != null)
                self.updateCallback(self);
            if(self.track)
                setTimeout(self.Start, 3 * 60 * 1000);
        };

        self.Err = function (arg) {
            self.IsAsking(false);
            if (self.updateCallback != null)
                self.updateCallback(self);
        };

    };

})(window);
/**/