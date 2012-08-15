(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.NoLayoutViewModel = function (layoutProperties) {
        var self = this;

        self.Properties = layoutProperties;
        self.GetImgTileArgs = function () {
            return self.Properties.GetImgTileArgs();
        };

        self.FlierTemplate = function (flier) {
            return self.Properties.FlierTemplate(flier);
        };

        self.GetTileClass = function () {
            return self.Properties.GetTileClass();
        };
    };

    bf.TileLayoutViewModel = function (targetdiv, layoutProperties) {
        var self = this;

        self.Properties = layoutProperties;
        self.GetImgTileArgs = function () {
            return self.Properties.GetImgTileArgs();
        };

        self.FlierTemplate = function (flier) {
            return self.Properties.FlierTemplate(flier);
        };

        self.GetTileClass = function () {
            return self.Properties.GetTileClass();
        };


        self.IsInit = false;

        self.$container = $(targetdiv);
        self.$container.empty();
        
        self.Init = function () {            
            self.InitAfterLoadImages($('.tile-img', self.$container));
        };

        self.InitAfterLoadImages = function ($elem) {
            var dfd = $elem.imagesLoaded();
            dfd.done(function ($images) {
                self.Layout();
            });
            // Rejected attempt to wait for images to load again
            dfd.fail(function ($images, $proper, $broken) {
                self.Layout();
                setTimeout(
                    function () {
                        self.InitAfterLoadImages($broken.not('.img-load-failed'));
                    }, 1000);
            });
        };

        self.Layout = function () {
            var $newEles = $('div.addedtile', self.$container);
            $newEles.removeClass('addedtile');
            if (self.orientation == 'v')
                self.$container.isotope({ itemSelector: '.' + self.GetTileClass() });
            else
                self.$container.isotope({ itemSelector: '.' + self.GetTileClass(), masonryHorizontal: {} });
            self.IsInit = true;
        };

        self.UnInit = function () {
            if (self.IsInit) {
                self.$container.isotope('destroy');
                self.IsInit = false;
            }
        };

        self.UpdateAfterLoadImages = function ($imageEles, isFail) {

            var dfd = $imageEles.imagesLoaded();
            dfd.done(function ($images) {
                if (isFail)
                    self.$container.isotope('reLayout');
                else {
                    var $newEles = $('div.addedtile', self.$container);
                    $newEles.removeClass('addedtile');
                    self.$container.isotope('appended', $newEles);
                }
            });
            // Rejected attempt to wait for images to load again
            dfd.fail(function ($images, $proper, $broken) {
                var $newEles = $('div.addedtile', self.$container);
                $newEles.removeClass('addedtile');
                self.$container.isotope('appended', $newEles);
                setTimeout(
                    function () {
                        self.UpdateAfterLoadImages($broken.not('.img-load-failed'), true);
                    }, 1000);
            });
        };

        self.Update = function () {

            var $items = $('.' + self.GetTileClass(), self.$container);
            if ($items.length == 0) {
                self.UnInit();
                return;
            }

            if (!self.IsInit) {
                self.Init();
                return;
            }

            var $newItems = $('div.addedtile', self.$container).not('.isotope-item');
            if (!$newItems.length)
                return;
            var $imgs = $('.tile-img', $newItems);
            self.UpdateAfterLoadImages($imgs, false);
        };

    };


})(window); 
