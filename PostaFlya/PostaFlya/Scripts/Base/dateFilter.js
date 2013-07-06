(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};

    bf.dateFilter = function (viewModel, observable) {
        viewModel.getDateString = function (date) {
            var newDate = new Date(date);
            return newDate.getFullYear() + " " + (newDate.getMonth() + 1) + " " + newDate.getDate();
        };
        
        viewModel.setDate = function (date) {
            observable(date);
        };

        viewModel.clearDate = function (date) {
            observable(null);
        };

        viewModel.setDateToday = function () {
            var currentdate = new Date();
            viewModel.setDate(viewModel.getDateString(currentdate));
        };

        viewModel.setDateTomorrow = function () {
            var currentdate = new Date();
            currentdate = currentdate.setDate(currentdate.getDate() + 1);
            var dateString = viewModel.getDateString(currentdate);
            viewModel.setDate(dateString);
        };

        viewModel.setDateThisWeek = function () {
            var currentdate = new Date();
            var day = currentdate.getDay();
            var diff = currentdate.getDate() - day + (day == 0 ? -6 : 1);
            viewModel.setDate(viewModel.getDateString(currentdate.setDate(diff)));

        };

        viewModel.setDateNextWeek = function () {

            var currentdate = new Date();
            var day = currentdate.getDay();
            var diff = currentdate.getDate() - day + (day == 0 ? -6 : 1);
            viewModel.setDate(viewModel.getDateString(currentdate.setDate(diff + 7)));

        };

        viewModel.setDateThisMOnth = function () {
            var currentdate = new Date();
            viewModel.setDate(viewModel.getDateString(currentdate.setDate(1)));
        };
    };
})(window, jQuery);

// Date.prototype.format() - By Chris West - MIT Licensed
(function (window) {
    var D = "Sunday,Monday,Tuesday,Wednesday,Thursday,Friday,Saturday".split(","),
        M = "January,February,March,April,May,June,July,August,September,October,November,December".split(",");
    Date.prototype.format = function (format) {
        var me = this;
        return format.replace(/a|A|Z|S(SS)?|ss?|mm?|HH?|hh?|D{1,4}|M{1,4}|YY(YY)?|'([^']|'')*'/g, function (str) {
            var c1 = str.charAt(0),
                ret = str.charAt(0) == "'"
                ? (c1 = 0) || str.slice(1, -1).replace(/''/g, "'")
                : str == "a"
                  ? (me.getHours() < 12 ? "am" : "pm")
                  : str == "A"
                    ? (me.getHours() < 12 ? "AM" : "PM")
                    : str == "Z"
                      ? (("+" + -me.getTimezoneOffset() / 60).replace(/^\D?(\D)/, "$1").replace(/^(.)(.)$/, "$10$2") + "00")
                      : c1 == "S"
                        ? me.getMilliseconds()
                        : c1 == "s"
                          ? me.getSeconds()
                          : c1 == "H"
                            ? me.getHours()
                            : c1 == "h"
                              ? (me.getHours() % 12) || 12
                              : (c1 == "D" && str.length > 2)
                                ? D[me.getDay()].slice(0, str.length > 3 ? 9 : 3)
                                : c1 == "D"
                                  ? me.getDate()
                                  : (c1 == "M" && str.length > 2)
                                    ? M[me.getMonth()].slice(0, str.length > 3 ? 9 : 3)
                                    : c1 == "m"
                                      ? me.getMinutes()
                                      : c1 == "M"
                                        ? me.getMonth() + 1
                                        : ("" + me.getFullYear()).slice(-str.length);
            return c1 && str.length < 4 && ("" + ret).length < str.length
              ? ("00" + ret).slice(-str.length)
              : ret;
        });
    };
    
    function pad(number) {
        var r = String(number);
        if (r.length === 1) {
            r = '0' + r;
        }
        return r;
    }
    
    Date.prototype.toISOOffsetString = function () {
        var ret = this.getUTCFullYear()
            + '-' + pad(this.getMonth() + 1)
            + '-' + pad(this.getDate())
            + 'T' + pad(this.getHours())
            + ':' + pad(this.getMinutes())
            + ':' + pad(this.getSeconds())
            + '.' + String((this.getMilliseconds() / 1000).toFixed(3)).slice(2, 5)
            + (this.getTimezoneOffset() <= 0 ? '+' : '-')
            + pad((Math.abs(this.getTimezoneOffset() / 60)))
            + ':' + pad((this.getTimezoneOffset() % 60));
        return ret;
    };


    bf.getDateFromHash = function() {
        var parts = window.location && window.location.hash && window.location.hash.slice(1).split("-");
        if (parts && parts.length >= 3) {

            var date = new Date();
            if (parts[2])
                date.setFullYear(parts[2]);
            if (parts[1])
                date.setMonth(parts[1] - 1);
            if (parts[0])
                date.setDate(parts[0]);
            return date;
        }
        return null;
    };

})(window);


