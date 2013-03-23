(function (window, undefined) {

    var bf = window.bf = window.bf || {};
    bf.pageinit = bf.pageinit || {};
    bf.pageinit['profile-credit'] = function () {
        bf.page = new bf.Payment();
    };
    bf.pageinit['profile-transaction'] = bf.pageinit['profile-credit'];
    bf.pageinit['profile-creditadded'] = bf.pageinit['profile-credit'];

    bf.Payment = function () {
        var self = this;
        self.CreateFlierInstance = bf.globalCreateFlierInstance;

        self.submit = function() {
            $("form").submit();
        };

        self._Init = function () {
            ko.applyBindings(self);
        };

        self._Init();
    };

})(window);

