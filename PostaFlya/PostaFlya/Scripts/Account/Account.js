﻿(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};
    bf.pageinit = bf.pageinit || {};
    bf.pageinit['login-page'] = function () {
        bf.page = new bf.Account();
    };

    bf.Account = function () {
        var self = this;
        self.CreateFlierInstance = bf.globalCreateFlierInstance;

        self._Init = function () {
            ko.applyBindings(self);
        };

        self._Init();
    };

})(window, jQuery);
