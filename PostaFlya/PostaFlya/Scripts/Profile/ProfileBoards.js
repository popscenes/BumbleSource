(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};
    bf.pageinit = bf.pageinit || {};
    bf.pageinit['profile-boards'] = function () {
        bf.page = new bf.ProfileBoardsViewModel();
    };


    bf.ProfileBoardsViewModel = function () {
        var self = this;

        self.CreateFlierInstance = bf.globalCreateFlierInstance;

        self.boards = ko.observableArray([]);

        self.GetReqUrl = function () {
            return "/webapi/browser/boards";

        };

        self.GetReqArgs = function () {
            var params = {};
            return params;
        };

        self.Request = function () {

            var reqArgs = self.GetReqArgs(false);

            $.ajax(
                 {
                     dataType: (bf.widgetbase ? "jsonp" : "json"),
                     url: self.GetReqUrl(),
                     crossDomain: (bf.widgetbase ? true : false),
                     data: reqArgs
                 }
             ).done(function (resp) {

                 self.boards.pushAll(resp.Data);

             }).always(function () {

             });

        };


        self._Init = function () {

            ko.applyBindings(self);
            self.Request();
        };

        self._Init();

    };


})(window, jQuery);