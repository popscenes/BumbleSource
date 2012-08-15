/**/
(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.DistanceSelector = function () {
        var self = this;

        self.currentDistance = ko.observable(10);
        self.updateCallback = null;

        self.Init = function () {
            if (bf.pageState !== undefined && bf.pageState.Distance !== undefined) {
                self.currentDistance(bf.pageState.Distance);
            }
        };
        self.Init();
    };


})(window);