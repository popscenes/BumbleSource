/**/
(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.ScrollToTop = function () {
        var self = this;

        self.$window = $(window);
        self.$actiondiv = $('<div id="scrolltotop">Scroll To Top</div>').appendTo('body');

        self.OnScroll = function (arg) {
            if (self.$window.scrollTop() == 0)
                self.$actiondiv.hide();
            else
                self.$actiondiv.show();
        };

        self.ScrollToTop = function (arg) {
            self.$window.scrollTop(0);
        };

        self._Init = function () {
            self.$window.scroll(self.OnScroll);
            self.$actiondiv.click(self.ScrollToTop);
            self.OnScroll();
        };

        self._Init();
    };

})(window);
/**/