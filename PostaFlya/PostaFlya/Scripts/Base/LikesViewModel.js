(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.LikeModel = function (data) {
        var self = this;
        $.extend(self, data);

        self.Browser = new bf.BrowserViewModel(data.Browser);
    };

    bf.LikesViewModel = function (entityType, entityId) {
        var self = this;

        var mapping = {
            create: function (options) {
                return new bf.LikeModel(options.data);
            }
        };

        self.EntityType = entityType;
        self.EntityId = entityId;
        self.List = ko.mapping.fromJS([], mapping);
        self.ILiked = ko.computed(function () {
            for (var i = 0; i < self.List.length; i++) {
                var curr = self.List[i];
                if (curr.Browser.BrowserId == bf.currentBrowserInstance.BrowserId)
                    return true;
            }
            return false;
        });

        self.LoadLikes = function () {
            $.getJSON('/api/like/', { entityTypeEnum: self.EntityType, id: self.EntityId }, function (data) {
                ko.mapping.fromJS(data, self.List);
            });

        };


        self.addLike = function () {
            var reqdata = ko.toJSON({
                LikeEntity: self.EntityType,
                EntityId: self.EntityId,
                BrowserId: bf.currentBrowserInstance.BrowserId
            });

            $.ajax('/api/like/', {
                data: reqdata,
                type: "post", contentType: "application/json",
                success: function (result) {
                    self.LoadLikes();
                },
                error: function (result) {
                }
            });
        };

        self._Init = function () {
            if (bf.pageState !== undefined && bf.pageState.Likes !== undefined) {
                ko.mapping.fromJS(bf.pageState.Likes, self.List);
            } else {
                self.LoadLikes();
            }
        };

        self._Init();
    };


})(window);