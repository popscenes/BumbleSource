
(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};
    bf.pageinit = bf.pageinit || {};
    bf.pageinit['default'] = function () {
        var self = this;
        self.CreateFlierInstance = bf.globalCreateFlierInstance;
        ko.applyBindings(self);
    };
    
    $(function () {
        var init = bf.pageinit[$('body[data-pageid]').attr('data-pageid')];
        if (init === undefined)
            init = bf.pageinit['default'];
        init();

        bf.ScrollToTop();
    });
    
})(window, jQuery);

