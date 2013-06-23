(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};

    bf.BehaviourViewModelFactory = function () {
        var self = this;

        self.hasHistory = !!(window.history && history.pushState);
        self.disablePushState = false;

        self.createViewModel = function (behaviourData) {

            return new bf.DefaultBehaviourViewModel(behaviourData);
        };

        self.addSammyRoutes = function (rootPath, sam, obsViewMod, initpath) {
            self.addSammyRoute(rootPath, sam, obsViewMod);
            sam.get(initpath, function () {           
                self.hideSelectedDetail(obsViewMod);
                _gaq.push(['_trackPageview', this.path]);
            });
        };

        self.getSammyRoot = function (rootPath) {
            var ret = '/';

            if (ret.substr(ret.length - 1) != "/")
                ret = ret + "/";

            ret = self.addHashBang(ret);
            return ret;
        };


        self.getInitPath = function () {
            var ret = window.location.pathname;
            ret = self.addHashBang(ret);
            return ret;
        };

        self.addSammyRoute = function (absPath, sam, obsViewMod) {
            var rootUrl = self.stripDetailFromPath(absPath);
            rootUrl = self.addHashBang(rootUrl);
            
            var route = rootUrl + '(.+@[0-9]{2}-[a-zA-Z]{3}-[0-9]{2})';
            sam.get(route, function () {
                var flierId = this.params['splat'];
                
                self.getSelectedDetail(this.path, obsViewMod, flierId);
                _gaq.push(['_trackPageview', this.path]);
            });
        };

        self.getSelectedDetail = function (path, obsViewMod, flierId) {
            obsViewMod(null);
            if (flierId == undefined)
                flierId = self.getIdFromPath(path);
            
            $.ajax({
                        dataType: (bf.widgetbase ? "jsonp" : "json"),
                        url: self.getDetailUrl() + flierId,
                        crossDomain: (bf.widgetbase ? true : false),
                    }
                ).done(function (behaviourData) {
                    obsViewMod(self.createViewModel(behaviourData));
                });
        };

        self.getDetailUrl = function() {
            return (bf.widgetbase ? bf.widgetbase : '') + '/api/BulletinApi/';
        };

        self.hideSelectedDetail = function (obsViewMod) {
            obsViewMod(null);
        };

        self.getDetailPath = function (rootPath, flier) {
            var url = self.addHashBang(rootPath);
            return url  + flier.FriendlyId;
        };

        self.addHashBang = function (path) {
            
            if (self.disablePushState && path.indexOf('#!/') < 0) {

                if (path.substr(path.length - 1) != "/")
                    path = path + "/";
                path = path + '#!/';
            }
            return path;
        };

        self.stripDetailFromPath = function (path) {
            var indx = path.lastIndexOf('/');
            if (indx != -1) {
                return path.substring(0, indx);
            }
            return path;
        };

        self.getIdFromPath = function (path) {

            var indx = path.lastIndexOf('/');
            if (indx != -1) {
                return path.substring(indx + 1);
            }
            return path;
        };

        self.getDetailTemplate = function (behaviourViewModel) {
            if (!behaviourViewModel || behaviourViewModel == 'undefined') return 'empty-detail';

            var isOwner = (bf.currentBrowserInstance && behaviourViewModel.Flier.BrowserId == bf.currentBrowserInstance.BrowserId);
            var ret = 'Behaviour' + behaviourViewModel.Flier.FlierBehaviour + '-template-detail';
            return isOwner ? ret + '-owner' : ret;
        };

    };


})(window, jQuery);
/**/