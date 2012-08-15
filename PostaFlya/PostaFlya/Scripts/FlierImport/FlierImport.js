(function (window, undefined) {
    var bf = window.bf = window.bf || {};

    bf.FlierImport = function () {
        var self = this;

        self.fliers = ko.observableArray([]);
        self.CreateFlierInstance = bf.globalCreateFlierInstance;
        

        self._Init = function () {
            ko.applyBindings(self);
            if (bf.pageState !== undefined && bf.pageState.Fliers !== undefined) {
                    
                self.fliers([]);
                self.fliers(bf.pageState.Fliers);
            }
        };

        self._Init();
    };


})(window);