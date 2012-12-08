(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.DefaultBehaviourViewModel = function (data, commentsViewModel, claimsViewModel) {
        var self = this;

        var mapping = {
            'copy': ["Flier"]
        };

        self.comments = ko.observable();
        self.comments(commentsViewModel);
        self.claims = ko.observable();
        self.claims(claimsViewModel);
        ko.mapping.fromJS(data, mapping, self);

        self.print = function() {

        };
        
        self.preparePrint = function () {

        };
        
    };


})(window);