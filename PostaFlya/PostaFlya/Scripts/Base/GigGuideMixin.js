﻿/**/
(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};
    
    bf.GigGuideMixin = function (self) {

        self.CreateFlierInstance = bf.globalCreateFlierInstance;
        self.CreateFlier = ko.observable();

        self.DateSections = ko.observableArray([]);

        self.moreFliersPending = ko.observable(false);
        self.noMoreFliersText = ko.observable('');

        self.MinPs = 1;

        self.CurrentStartDate = ko.observable(getCurrentDate());

        self.Request = function () {

            if (self.moreFliersPending())
                return;
            
            self.DateSections.removeAll();

            self.moreFliersPending(true);
            self.noMoreFliersText('');

            var reqArgs = self.GetReqArgs(false);
            var date = getCurrentDate();
            if (date) {
                reqArgs.Start = date.toISOOffsetString();
            }
            self.CurrentStartDate(date);

            $.ajax(
                 {
                     dataType: (bf.widgetbase ? "jsonp" : "json"),
                     url: self.GetReqUrl(),
                     crossDomain: (bf.widgetbase ? true : false),
                     data: reqArgs
                 }
             ).done(function (resp) {

                 processContent(resp, self.DateSections, self.MinPs, self.MaxPs);
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
        
        //todo do this nicer
        self.PrevWeekLink = ko.computed(function () {
            return getPrevWeek(self.CurrentStartDate()).format("#DD-MM-YYYY");
        });
        
        self.PrevWeek = ko.computed(function () {
            return 'Back to ' + getPrevWeek(self.CurrentStartDate()).format("DDD DD MMM");
        });
        
        self.CurrentDateLink = ko.computed(function () {
            return self.CurrentStartDate().format("#DD-MM-YYYY");
        });
        
        self.CurrentDate = ko.computed(function () {
            return self.CurrentStartDate().format("DDD DD MMM");
        });
           
        self.NextWeekLink = ko.computed(function () {
            return getNextWeek(self.CurrentStartDate()).format("#DD-MM-YYYY");
        });
        
        self.NextWeek = ko.computed(function () {
            return 'Jump to ' + getNextWeek(self.CurrentStartDate()).format("DDD DD MMM");
        });
        //end todo do this nicer

        self.TearOff = function (flier) {

            if (bf.widgetbase) {
                window.location = bf.widgetbase + '/' + flier.FriendlyId;
                return true;
            }
            
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
            self.SelectedViewModel.SelectedDetail(null);
            self.Request();
        };

        self.AddGetDateRoute = function(sam) {
            var route = window.location.pathname + '#([0-9]{2}-[0-9]{2}-[0-9]{4})';
            sam.get(route, function () {
                self.TryRequest();
            });
            sam.get(window.location.pathname, function () {
                self.TryRequest();
            });

        };
        
        
        self.setNoMoreFlyas = function () {
            //var nomore = 'No gigs from ' + self.CurrentStartDate().format("DDD DD MMM");
            //var nomore = 'No gigs from ' + self.CurrentStartDate().format("DDD DD MMM");
            self.noMoreFliersText('');
        };


        self.StatusText = ko.computed(function () {
            return '';
//            if (self.moreFliersPending() || (self.noMoreFliersText() && self.DateSections().length == 0))
//                return '';
//            
//            var showingmostrecent = 'Showing gigs from ';
//            showingmostrecent += self.CurrentStartDate().format("DDD DD MMM");
//
//            return showingmostrecent;

        }, self);

    };
    
    function getCurrentDate() {
        return bf.getDateFromHash() || new Date();
    }
    
    function getNextWeek(fromDate) {
        var curr = new Date(fromDate);
        curr.setDate(curr.getDate() + 7);
        return curr;
    }
    
    function getPrevWeek(fromDate) {
        var curr = new Date(fromDate);
        curr.setDate(curr.getDate() - 7);
        return curr;
    }

    function processContent(resp, itemArray, minFlyers, maxFlyers) {

        var dateLinks = {};
        var sectArr = [];
        var allData = resp.Data;
        var index = 0;
        while (index < allData.Dates.length) {
            var nextRange = getNextDates(allData.Dates, index, minFlyers, maxFlyers);
            var flyerDates = {};
            flyerDates.DateStart = new Date(nextRange[0].Date);
            flyerDates.DateEnd = new Date(nextRange[nextRange.length - 1].Date);
            flyerDates.DateLink = flyerDates.DateStart.format("DD-MM-YYYY");
            flyerDates.Spans = nextRange.length > 1;
            
            flyerDates.Flyers = [];
            for (var i = 0; i < nextRange.length; i++) {
                
                var next = nextRange[i];
                var flyerDate = new Date(next.Date).format("DDD DD MMM");;
                
                for (var f = 0; f < next.FlyerIds.length; f++) {

                    var flyer = $.extend({}, allData.Flyers[next.FlyerIds[f]]);
//                    var dd = getImageExt(flyer.Image, 150, 'Square');
//                    flyer.Image.FlierImageUrl = flyer.Image.BaseUrl + dd.UrlExtension;
//                    flyer.Image.Width = dd.Width;
//                    flyer.Image.Height = dd.Height;
                    flyer.FlyerDate = (flyerDates.Spans) ? flyerDate : null;
                    dateLinks[flyerDate] = flyerDates.DateLink;

                    flyerDates.Flyers.push(flyer);
                } 
            }
             
            sectArr.push(flyerDates);
            
            index += nextRange.length;
        }
        
        for (var s = 0; s < sectArr.length; s++) {
            var section = sectArr[s];
            section.Dates = [];
            for (var d = -2; d <= 2; d++) {
                var pickDate = new Date(d <= 0 ? section.DateStart : section.DateEnd);
                pickDate.setDate(pickDate.getDate() + d);
                var dateString = (section.Spans && d == 0)
                    ? section.DateStart.format("DDD DD MMM") /*+ " to " + section.DateEnd.format("DDD DD MMM")*/
                    : pickDate.format("DDD DD MMM");
                section.Dates.push({
                    Datestring: dateString,
                    DateLink: '#' + (dateLinks[dateString] || pickDate.format("DD-MM-YYYY")),
                    Date: pickDate,
                    Ishistory: d < 0,
                    Iscurrent: d == 0,
                    Isfuture: d > 0,
                    HideMobile: d < -1 || d > 1,
                    HideMobileSeperator: d < -1 || d > 0
                });
            }

            itemArray.push(section);
        }

//        for (var i = 0; i < allData.Dates.length; i++) {
//            var next = allData.Dates[i];
//            next.Flyers = [];
//            for (var f = 0; f < next.FlyerIds.length; f++) {
//                
//                var flyer = $.extend({}, allData.Flyers[next.FlyerIds[f]]);
//
//                var dd = getImageExt(flyer.Image, 150, 'Square');
//                flyer.Image.FlierImageUrl = flyer.Image.BaseUrl + dd.UrlExtension;
//                flyer.Image.Width = dd.Width;
//                flyer.Image.Height = dd.Height;
//                next.Flyers.push(flyer);
//            }
//
//            next.DateStart = new Date(next.Date);
//            next.DateEnd = new Date(next.Date);    
//            next.DateLink = next.Date.format("DD-MM-YYYY");
//            next.Dates = [];
//
//
//            itemArray.push(next);
//        }
    };
    
    function getNextDates(dates, startindex, minFlyers, maxFlyers) {
        var ret = [];
        var total = 0;
        for (var i = startindex; i < dates.length; i++) {
            var numflys = dates[i].FlyerIds.length;
            total += numflys;
            if (maxFlyers && total > numflys && total > maxFlyers)
                return ret;
            ret.push(dates[i]);
            if (total >= minFlyers)
                return ret;
        }
        return ret;
    }
    
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