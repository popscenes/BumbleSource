/**/
(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};
    
    bf.GigGuideMixin = function (self) {

        self.CreateFlierInstance = bf.globalCreateFlierInstance;
        self.CreateFlier = ko.observable();

        self.DateSections = ko.observableArray([]);

        self.moreFliersPending = ko.observable(false);
        self.noMoreFliersText = ko.observable('');

        self.Request = function () {

            if (self.moreFliersPending())
                return;

            self.moreFliersPending(true);
            self.noMoreFliersText('');

            $.ajax(
                 {
                     dataType: (bf.widgetbase ? "jsonp" : "json"),
                     url: self.GetReqUrl(),
                     crossDomain: (bf.widgetbase ? true : false),
                     data: self.GetReqArgs(false)
                 }
             ).done(function (resp) {

                 processContent(resp, self.DateSections);
                 if (resp.Data.Dates.length == 0)
                     self.setNoMoreFlyas();

             }).always(function () {
                 self.moreFliersPending(false);
             });

        };

        self.navigateToDate = function (dateData) {
            if ($(dateData.DateLink).length > 0)
                return true;

            self.DateSections.removeAll();
            window.location.hash = dateData.DateLink;
            self.Request();
            return false;
        };


        self.TearOff = function (flier) {

            bf.pagedefaultaction.set('bulletin-detail', 'peel');
            self.SelectedViewModel.showDetails(flier);
            return true;
        };

        self.FlierTemplate = function (flier) {
            var isOwner = (bf.currentBrowserInstance && flier.BrowserId == bf.currentBrowserInstance.BrowserId);
            var ret = 'BehaviourDefault-template';
            return isOwner ? ret + '-owner' : ret;
        };

        self.getDetailUrl = function (flier) {
            return self.SelectedViewModel.getDetailUrl(flier);
        };

        self.TryRequest = function () {
            self.Request();
        };
        
        self.setNoMoreFlyas = function () {
            var nomore = 'No more gigs';
            self.noMoreFliersText(nomore);
        };


        self.StatusText = ko.computed(function () {

            if (self.moreFliersPending() || (self.noMoreFliersText() && self.DateSections().length == 0))
                return '';
            
            var showingmostrecent = "Showing gigs from ";
            var date = bf.getDateFromHash() || new Date();
            showingmostrecent += date.format("DDD DD MMM");

            return showingmostrecent;

        }, self);

    };
    

    function processContent(resp, itemArray) {

        var allData = resp.Data;
        for (var i = 0; i < allData.Dates.length; i++) {
            var next = allData.Dates[i];
            next.Flyers = [];
            for (var f = 0; f < next.FlyerIds.length; f++) {
                var flyer = allData.Flyers[next.FlyerIds[f]];
                var dd = getImageExt(flyer.Image, 150, 'Square');

                flyer.Image.FlierImageUrl = flyer.Image.BaseUrl + dd.UrlExtension;
                flyer.Image.Width = dd.Width;
                flyer.Image.Height = dd.Height;
                next.Flyers.push(flyer);
            }

            next.Date = new Date(next.Date);
            next.DateLink = next.Date.format("DD-MM-YYYY");
            next.Dates = [];
            for (var d = -2; d <= 2; d++) {
                var pickDate = new Date(next.Date);
                pickDate.setDate(pickDate.getDate() + d);
                next.Dates.push({
                    Datestring: pickDate.format("DDD DD MMM"),
                    DateLink: '#' + pickDate.format("DD-MM-YYYY"),
                    Date: pickDate,
                    Ishistory: d < 0,
                    Iscurrent: d == 0,
                    Isfuture: d > 0
                });
            }

            itemArray.push(next);
        }
    };

    function getImageExt(image, width, axis) {
        var dd = image.Extensions[0];
        for (var d = 0 ; d < image.Extensions.length; d++) {
            var ext = image.Extensions[d];
            if (ext.Width == width && ext.ScaleAxis == axis) {
                dd = ext;
                break;
            }
        }
        return dd;
    }


})(window, jQuery);
/**/