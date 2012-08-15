(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.DefaultBehaviourViewModel = function (data, commentsViewModel, likesViewModel) {
        var self = this;

        var mapping = {
            'copy': ["Flier"]
        };

        self.comments = ko.observable();
        self.comments(commentsViewModel);
        self.likes = ko.observable();
        self.likes(likesViewModel);
        ko.mapping.fromJS(data, mapping, self);
    };


})(window);