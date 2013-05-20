(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};

    bf.ClaimModel = function (data) {
        var self = this;
        $.extend(self, data);

        self.Browser = new bf.BrowserViewModel(data.Browser);
    };

    bf.ClaimsViewModel = function (entityType, entityId) {
        var self = this;

        var mapping = {
            create: function (options) {
                return new bf.ClaimModel(options.data);
            }
        };

        self.EntityType = entityType;
        self.EntityId = entityId;
        self.List = ko.mapping.fromJS([], mapping);
        self.IClaimed = ko.computed(function () {
            for (var i = 0; i < self.List.length; i++) {
                var curr = self.List[i];
                if (curr.Browser.BrowserId == bf.currentBrowserInstance.BrowserId)
                    return true;
            }
            return false;
        });

        self.LoadClaims = function () {
            $.getJSON('/api/claim/', { entityTypeEnum: self.EntityType, id: self.EntityId }, function (data) {
                ko.mapping.fromJS(data, self.List);
            });

        };


        self.addClaim = function () {
            var reqdata = ko.toJSON({
                ClaimEntity: self.EntityType,
                EntityId: self.EntityId,
                BrowserId: bf.currentBrowserInstance.BrowserId
            });

            $.ajax('/api/claim/', {
                data: reqdata,
                type: "post", contentType: "application/json",
                success: function (result) {
                    self.LoadClaims();
                },
                error: function (result) {
                }
            });
        };

        self._Init = function () {
            if (bf.pageState !== undefined && bf.pageState.Claims !== undefined) {
                ko.mapping.fromJS(bf.pageState.Claims, self.List);
            } else {
                self.LoadClaims();
            }
        };

        self._Init();
    };


})(window, jQuery);