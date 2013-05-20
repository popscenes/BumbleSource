(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};

    bf.BulletinLayoutProperties = function () {
        var self = this;

        self.imglimit = 250;
        self.imglimitdim = 'h';
        self.orientation = 'v';

        self.FlierTemplate = function (flier) {
            var isOwner = (flier.BrowserId == bf.currentBrowserInstance.BrowserId);
            var ret = 'Behaviour' + flier.FlierBehaviour + '-template';
            return isOwner ? ret + '-owner' : ret;
        };

        self.GetTileClass = function () {
            return 'ui-flier-tile-' + self.orientation + '-' + self.imglimit + self.imglimitdim;
        };

    };


})(window, JQuery);