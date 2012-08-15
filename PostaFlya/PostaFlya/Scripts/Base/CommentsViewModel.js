(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.CommentModel = function (data) {
        var self = this;
        $.extend(self, data);

        self.Browser = new bf.BrowserViewModel(data.Browser);
    };

    bf.CommentsViewModel = function (entityType, entityId) {
        var self = this;

        var mapping = {
            create: function (options) {
                return new bf.CommentModel(options.data);
            }
        };

        self.EntityType = entityType;
        self.EntityId = entityId;
        self.List = ko.mapping.fromJS([], mapping);

        self.LoadComments = function () {
            $.getJSON('/api/comment/', { entityTypeEnum: self.EntityType, id: self.EntityId }, function (data) {
                ko.mapping.fromJS(data, self.List);
            });

        };

        self.isCommentError = ko.observable(false);
        self.commentToAdd = ko.observable("");
        self.addComment = function () {
            if (self.commentToAdd() != "") {

                self.isCommentError(false);

                var reqdata = ko.toJSON({
                    CommentEntity: self.EntityType,
                    EntityId: self.EntityId,
                    Comment: self.commentToAdd,
                    BrowserId: bf.currentBrowserInstance.BrowserId
                });

                $.ajax('/api/comment/', {
                    data: reqdata,
                    type: "post", contentType: "application/json",
                    success: function (result) {
                        var newComment = $.parseJSON(reqdata);
                        newComment.CommentTime = (new Date()).toISOString();
                        newComment.Browser = bf.currentBrowserInstance;
                        self.List.push(new bf.CommentModel(newComment));
                        self.commentToAdd("");
                    },
                    error: function (result) {
                        self.isCommentError(true);
                    }
                });
            }
        };

        self._Init = function () {
            if (bf.pageState !== undefined && bf.pageState.Comments !== undefined) {
                ko.mapping.fromJS(bf.pageState.Comments, self.List);
            } else {
                self.LoadComments();
            }
        };

        self._Init();
    };


})(window);