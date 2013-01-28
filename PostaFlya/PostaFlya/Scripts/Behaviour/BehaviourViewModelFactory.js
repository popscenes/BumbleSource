(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.BehaviourViewModelFactory = function () {
        var self = this;

        self.Behaviours = ['Default', 'TaskJob'];
        self.hasHistory = !!(window.history && history.pushState);

        self.createViewModel = function (behaviourData) {

            var comments = new bf.CommentsViewModel('Flier', behaviourData.Flier.Id);
            var claims = new bf.ClaimsViewModel('Flier', behaviourData.Flier.Id);
            switch (behaviourData.Flier.FlierBehaviour) {
                case 'Default':
                    return new bf.DefaultBehaviourViewModel(behaviourData, comments, claims);
                case 'TaskJob':
                default:
                    return new bf.DefaultBehaviourViewModel(behaviourData, comments, claims);
            }
        };

        self.addSammyRoutes = function (rootPath, sam, obsViewMod, initpath) {
            self.addSammyRoute(rootPath, sam, obsViewMod);
            sam.get(initpath, function () {
                obsViewMod(null);
            });
        };

        self.getSammyRoot = function (rootPath) {
            var ret = '/';

//            if (!self.hasHistory)//if doesn't have history always go back
//                ret = rootPath;

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
            });
        };

        self.getSelectedDetail = function (path, obsViewMod, flierId) {
            obsViewMod(null);
            if (flierId == undefined)
                flierId = self.getIdFromPath(path);
            $.getJSON('/api/BulletinApi/' + flierId
                    , function (behaviourData) {
                        obsViewMod(self.createViewModel(behaviourData));
                    });
        };

        self.getDetailPath = function (rootPath, flier) {
            var url = self.addHashBang(rootPath);
            return url  + flier.FriendlyId;
        };

        self.addHashBang = function (path, prefix) {
//            if (!self.hasHistory && path.indexOf('#!/') < 0) {
//                if (prefix != undefined && prefix) {
//                    path = (path[0] != '/') ? '/#!/' + path : '/#!' + path;
//                } else {
//                    if (path.substr(path.length - 1) != "/")
//                        path = path + "/";
//                    path = path + '#!/';
//                }
//
//            }
            return path;
        };

        self.getBehaviourForPath = function (path) {
            for (var i = 0; i < self.Behaviours.length; i++) {
                var behaviour = self.Behaviours[i];
                if (path.indexOf(behaviour) != -1)
                    return behaviour;
            }
            return self.Behaviours[0];
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

            var isOwner = (behaviourViewModel.Flier.BrowserId == bf.currentBrowserInstance.BrowserId);
            var ret = 'Behaviour' + behaviourViewModel.Flier.FlierBehaviour + '-template-detail';
            return isOwner ? ret + '-owner' : ret;
        };


        self.isBehaviour = function (behaviour) {
            if (behaviour == undefined || behaviour == null)
                return false;
            return ($.inArray(params.behaviour, self.Behaviours) >= 0);
        };
    };


})(window);
/**/