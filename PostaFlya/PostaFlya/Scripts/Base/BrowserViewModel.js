(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.BrowserViewModel = function (data) {
        var self = this;
        $.extend(self, data);

        self.ProfileUrl = function () {
            return '/' + self.Name;
        };
    };

})(window);