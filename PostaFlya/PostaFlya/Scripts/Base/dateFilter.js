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


