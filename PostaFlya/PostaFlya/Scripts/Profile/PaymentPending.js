(function (window, undefined) {

    var bf = window.bf = window.bf || {};
    
    bf.pageinit = bf.pageinit || {};
    bf.pageinit['paymentpending'] = function () {
        bf.page = new bf.PaymentPending();
    };

    bf.PaymentPending = function () {
        var self = this;
        self.CreateFlierInstance = bf.globalCreateFlierInstance;
        self.currentBrowser = bf.currentBrowserInstance;
        self.apiUrl = sprintf("/api/Browser/%s/pendingfliersapi", bf.currentBrowserInstance.BrowserId);
        self.MyPendingFliersList = ko.observable();
        
        self.GetMyPendingFliers = function () {
            $.ajax(self.apiUrl, {
                type: "get", contentType: "application/json",
                success: function (result) {
                    self.MyPendingFliersList(result);
                }
            });
        };

        self.payFlier = function (flier) {
            $.ajax(self.apiUrl + "/" + flier.Id, {
                //data: { flierId: flier.Id },
                type: "put", contentType: "application/json",
                success: function (result) {
                    self.GetMyPendingFliers();
                }
            });
        };

        self._Init = function () {
            ko.applyBindings(self);
            self.GetMyPendingFliers();
        };

        self._Init();
    };

})(window);

