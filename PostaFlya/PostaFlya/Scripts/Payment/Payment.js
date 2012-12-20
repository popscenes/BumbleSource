(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.Payment = function () {
        var self = this;
        self.CreateFlierInstance = bf.globalCreateFlierInstance;

        self._Init = function () {
            ko.applyBindings(self);
        };

        self._Init();
    };

})(window);

$(function () {

    bf.page = new bf.Payment();
});
