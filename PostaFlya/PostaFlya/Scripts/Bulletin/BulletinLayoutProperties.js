(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.BulletinLayoutProperties = function () {
        var self = this;

        self.imglimit = 250;
        self.imglimitdim = 'h';
        self.orientation = 'v';

        self.FlierTemplate = function (flier) {
            return 'Behaviour' + flier.FlierBehaviour + '-template';
        };

        self.GetTileClass = function () {
            return 'ui-flier-tile-' + self.orientation + '-' + self.imglimit + self.imglimitdim;
        };

    };


})(window);