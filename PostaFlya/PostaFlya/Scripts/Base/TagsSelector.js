(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};

    bf.TagsSelector = function (options) {
        var self = this;

        var defaults = {
            displayInline: false
        };

        var options = $.extend(defaults, options);

        self.displayInline = ko.observable(options.displayInline);


        self.Tags = ko.observableArray([]);
        self.SelectedTags = ko.observableArray([]);
        self.ShowTags = ko.observable(options.displayInline);
        self.updateCallback = null;

        var done = function(allData) {
            ko.mapping.fromJS(allData, {}, self.Tags);
        };
        self.LoadTags = function () {
            $.ajax(
                {
                    dataType: (bf.widgetbase ? "jsonp" : "json"),
                    url: self.GetReqUrl(),
                    crossDomain: (bf.widgetbase ? true : false),
                    success: done
                }
            );

        };

        self.GetReqUrl = function () {
            return (bf.widgetbase ? bf.widgetbase : '') + '/api/TagsApi/';
        };

        makeSafeForCSS = function (name) {
            return name.replace(/[^a-z0-9]/g, function (s) {
                var c = s.charCodeAt(0);
                if (c == 32) return '-';
                if (c >= 65 && c <= 90) return '_' + s.toLowerCase();
                return '__' + ('000' + c.toString(16)).slice(-4);
            });
        };

        self.tagSelected = function (tag) {
            if (self.SelectedTags.indexOf(tag) == -1) {
                self.SelectedTags.push(tag);
            }
            else {
                self.SelectedTags.remove(tag);
            }

            if (self.updateCallback != null) {
                self.updateCallback();
            }
        };

        self.Init = function () {
            if (bf.pageState !== undefined && bf.pageState.Tags !== undefined) {
                ko.mapping.fromJS(bf.pageState.Tags, self.SelectedTags);
            }
        };
        self.Init();

    };

})(window, jQuery);