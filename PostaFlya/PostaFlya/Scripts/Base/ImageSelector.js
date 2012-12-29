(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.ImageSelector = function (options) {
        var self = this;

        var defaults = {
            uploaderElementId: "uploader",
            imageListId: "imageList",
            callback: null
        };

        var options = $.extend(defaults, options);

        self.uploaderElementId = ko.observable(options.uploaderElementId);
        self.imageListId = ko.observable(options.imageListId);

        self.imageList = ko.observableArray([]);
        self.selectedImageId = ko.observable();
        self.selectedImageUrl = ko.observable();

        

        self.SetCallback = function (callback) {
            options.callback = callback;
        };

        self.Init = function () {

            self._LoadImageList();
            self._InitPLUpload();
            $("#" + self.uploaderElementId()).dialog({ autoOpen: false });
        };

        self.clickImage = function (image) {
            self.selectedImageId(image.ImageId);
            self.selectedImageUrl(image.ImageUrl);

            if (options.callback != null) {
                options.callback(image);
            }
        };

        self.toggleAddImage = function () {
            var uploader = $("#" + self.uploaderElementId()).pluploadQueue();

            if ($("#" + self.uploaderElementId()).dialog("isOpen")) {
                $("#" + self.uploaderElementId()).dialog("close");
            } else {
                $("#" + self.uploaderElementId()).dialog("open");
            }

            uploader.refresh();
        };

        self._LoadImageList = function () {
            var url = sprintf("/api/Browser/%s/MyImages", bf.currentBrowserInstance.BrowserId);

            $.getJSON(url, function (allData) {
                var imagesDisplayNum = 8;


                self.imageList(allData);
                if (self.imageList().length < imagesDisplayNum)
                    imagesDisplayNum = self.imageList().length;

                if (imagesDisplayNum > 0) {
                    //self.slider = $('#' + options.imageListId).bxSlider({ displaySlideQty: imagesDisplayNum, moveSlideQty: 1, infiniteLoop: false });
                    //self.slider.goToPreviousSlide();
                    var gallery = $('#thumbs').galleriffic({
                        delay: 3000, // in milliseconds
                        numThumbs: 6, // The number of thumbnails to show page
                        preloadAhead: 40, // Set to -1 to preload all images
                        enableTopPager: false,
                        enableBottomPager: true,
                        maxPagesToShow: 7,  // The maximum number of pages to display in either the top or bottom pager
                        imageContainerSel: '.image-holder', // The CSS selector for the element within which the main slideshow image should be rendered
                        controlsContainerSel: '', // The CSS selector for the element within which the slideshow controls should be rendered
                        captionContainerSel: '', // The CSS selector for the element within which the captions should be rendered
                        loadingContainerSel: '', // The CSS selector for the element within which should be shown when an image is loading
                        renderSSControls: true, // Specifies whether the slideshow's Play and Pause links should be rendered
                        renderNavControls: true, // Specifies whether the slideshow's Next and Previous links should be rendered
                        playLinkText: 'Play',
                        pauseLinkText: 'Pause',
                        prevLinkText: 'Previous',
                        nextLinkText: 'Next',
                        nextPageLinkText: 'Next &rsaquo;',
                        prevPageLinkText: '&lsaquo; Prev',
                        enableHistory: false, // Specifies whether the url's hash and the browser's history cache should update when the current slideshow image changes
                        enableKeyboardNavigation: true, // Specifies whether keyboard navigation is enabled
                        autoStart: false, // Specifies whether the slideshow should be playing or paused when the page first loads
                        syncTransitions: false, // Specifies whether the out and in transitions occur simultaneously or distinctly
                        defaultTransitionDuration: 1000, // If using the default transitions, specifies the duration of the transitions
                        onSlideChange: undefined, // accepts a delegate like such: function(prevIndex, nextIndex) { ... }
                        onTransitionOut: undefined, // accepts a delegate like such: function(slide, caption, isSync, callback) { ... }
                        onTransitionIn: undefined, // accepts a delegate like such: function(slide, caption, isSync) { ... }
                        onPageTransitionOut: undefined, // accepts a delegate like such: function(callback) { ... }
                        onPageTransitionIn: undefined, // accepts a delegate like such: function() { ... }
                        onImageAdded: undefined, // accepts a delegate like such: function(imageData, $li) { ... }
                        onImageRemoved: undefined  // accepts a delegate like such: function(imageData, $li) { ... }
                    });
                    self._loadSelectedImageFromId();
                }
            });
        };

        self._loadSelectedImageFromId = function () {
            if (self.selectedImageId() == undefined)
                return;

            for (var i = 0; i < self.imageList().length; i++) {
                if (self.selectedImageId() == self.imageList()[i].ImageId) {
                    self.selectedImageUrl(self.imageList()[i].ImageUrl);
                    break;
                }
            }
        };

        self._InitPLUpload = function () {
            $("#" + self.uploaderElementId()).pluploadQueue({

                runtimes: 'html5,html4',
                url: '/Img/Post/',
                max_file_size: '10mb',
                unique_names: true,
                multiple_queues: true,
                filters: [
                    { title: "Image files", extensions: "jpg,gif,png" }
                ]

            });

            var uploader = $("#" + self.uploaderElementId()).pluploadQueue();
            uploader.bind('UploadComplete', function (up, files) {

                if (self.imageList().length > 0)
                    self.slider.destroyShow();
                self._LoadImageList();

                $("#" + self.uploaderElementId()).dialog("close");
            });
        };
    };

})(window);


