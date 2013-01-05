(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.PaymentPending = function () {
        var self = this;
        self.CreateFlierInstance = bf.globalCreateFlierInstance;

        self._Init = function () {
            ko.applyBindings(self);
        };

        self._Init();
    };

})(window);

$(function () {

    bf.page = new bf.PaymentPending();
});
