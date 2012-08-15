(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.ImageSelector = function (options) {
        var self = this;

        var defaults = {
            uploaderElementId: "uploader",
            imageListId: "imageList",
            callback: null
        }

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
                    self.slider = $('#' + options.imageListId).bxSlider({ displaySlideQty: imagesDisplayNum, moveSlideQty: 1, infiniteLoop: false });
                    self.slider.goToPreviousSlide();
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


