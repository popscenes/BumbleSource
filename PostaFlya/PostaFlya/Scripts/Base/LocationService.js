/**/
(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.LocationService = function () {
        var self = this;

        self.currentLocation = null;
        self.updateCallback = null;

        self.StartTrackingLocation = function (callback) {
            self.updateCallback = callback;
            self.Start();
        };

        self.Start = function () {
            if (navigator.geolocation) {
                // Locate position
                navigator.geolocation.getCurrentPosition(self.Success, self.Err);
            } else {
                Err(null);
            }
        };

        self.Success = function (loc) {
            self.currentLocation = loc;
            if (self.updateCallback != null)
                self.updateCallback(self);
            setTimeout(self.Start, 3 * 60 * 1000);
        };

        self.Err = function (arg) {
            if (self.updateCallback != null)
                self.updateCallback(self);
        };

    };

})(window);
/**/