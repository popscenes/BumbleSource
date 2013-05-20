(function (window, $, undefined) {
    var bf = window.bf = window.bf || {};
    bf.pageinit = bf.pageinit || {};
    
    bf.pageinit['flier-import'] = function () {
        bf.page = new bf.FlierImport();
    };

    bf.FlierImport = function () {
        var self = this;

        self.fliers = ko.observableArray([]);
        self.CreateFlierInstance = bf.globalCreateFlierInstance;
        

        self._Init = function () {
            
            if (bf.pageState !== undefined && bf.pageState.Fliers !== undefined) {
                    
                self.fliers([]);
                self.fliers(bf.pageState.Fliers);
            }
            ko.applyBindings(self);
        };

        self._Init();
    };


})(window, jQuery);