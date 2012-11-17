ko.bindingHandlers.markdown = {
    update: function (element, valueAccessor) {
        var markdownValue = ko.utils.unwrapObservable(valueAccessor());
        var htmlValue = markdownValue && new Showdown.converter().makeHtml(markdownValue);
        $(element).html(htmlValue || "");
    }
};

ko.bindingHandlers.datePicker = {
    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        $(element).datepicker(valueAccessor()).datepicker("setDate", new Date(viewModel.EffectiveDate()));
    }
};

//ko.bindingHandlers.imageSelectorBinding = {
//    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
//        viewModel.Init();
//    }
//};

ko.bindingHandlers.mapBinding = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var newLocation = viewModel.savedLocations()[$(this)[0].selectedIndex - 1];
        $(element).change(function () {
            var newLocation = viewModel.savedLocations()[$(this)[0].selectedIndex - 1];
            viewModel.currentLocation(newLocation);
        });

        viewModel.ShowMap();

        
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var currLoc = valueAccessor();
        if (currLoc() != null && currLoc() != 'undefined')
            bf.SetMapPosition($('#' + viewModel.mapElementId()), currLoc().Longitude(), currLoc().Latitude());
    }
};

ko.bindingHandlers.distanceSlider = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {

        var value = ko.utils.unwrapObservable(valueAccessor());

        $(element).slider({ value: value, min: 5, max: 30 });

        $(element).slider({
            change: function (event, ui) {
                viewModel.currentDistance($(element).slider("value"));
                if (viewModel.updateCallback != null) {
                    viewModel.updateCallback();
                }
            }
        });
    }
};

//basically takes the class input and appends 'a', 'b', 'c' depending on the position of
//the model in the parent c
ko.bindingHandlers.cssAbcCollection = {
    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {

        var allBindings = allBindingsAccessor();
        var modulo = allBindings.modulo;
        var abcCss = allBindings.prefix;
        var collection = valueAccessor();
        var offset = collection.indexOf(viewModel) % modulo;
        var cssClass = abcCss + String.fromCharCode(97 + offset);
        
        $(element).addClass(cssClass);
    }
};

ko.bindingHandlers.bulletintile = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var jele = $(element);
        var layout = valueAccessor();
        jele.addClass(layout.GetTileClass());
    },

    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {

    }
};

ko.bindingHandlers.updateLayout = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
    },

    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var layout = valueAccessor();
        layout.Update();
    }
};

ko.bindingHandlers.imgRetry = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {

        var jele = $(element);
        jele.data("reload-times", 0);
        var src = jele.attr("src");
        if (src == undefined || src == null || src == "") {
            jele.removeAttr('src');
            var val = valueAccessor();
            src = ko.utils.unwrapObservable(val);
        }


        jele.data("imgsrc", src);
        var imageReplacer = $('<img/>');
        imageReplacer.addClass('img-error');
        imageReplacer.hide();
        imageReplacer.insertAfter(jele);

        jele.bind("error", function () {
            var reloadTimes = jele.data("reload-times");
            var imgSrc = jele.data("imgsrc");
            var errorSrc = "/Img/GetError" + imgSrc.substring(imgSrc.lastIndexOf('/'));

            imageReplacer.attr('src', errorSrc);
            jele.hide();
            imageReplacer.show();
            //jele.attr("src", errorSrc);

            var timeOut = reloadTimes == 0 ? 0 : 1000;
            if (reloadTimes < 5) {
                setTimeout(function () {
                    //jele.attr("src", null);
                    jele.attr("src", imgSrc + "?" + reloadTimes);
                    reloadTimes++;
                    jele.data("reload-times", reloadTimes);

                }, timeOut);
            } else {
                setTimeout(function () {
                    jele.addClass("img-load-failed");
                    jele.unbind("error");
                });
            }
        });

        jele.bind("load", function () {
            jele.show();
            imageReplacer.hide();
        });

        jele.attr('src', src);
    },

    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {

    }
};

ko.bindingHandlers.validate = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var jele = $(element);

        jQuery.validator.unobtrusive.parse(jele);
        var validatorSettings = $.data(jele[0], 'validator').settings;
        validatorSettings.ignore = "";
    },

    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        // Target Form 
        var $form = $("#" + element.id);

        // Unbind existing validation 
        $form.unbind();
        $form.data("validator", null);

        // Check document for changes 
        $.validator.unobtrusive.parse($form);

        // Re add validation with changes 
        $form.validate($form.data("unobtrusiveValidation").options);

        var validatorSettings = $.data($form[0], 'validator').settings;
        validatorSettings.ignore = "";


    }
};

ko.bindingHandlers.bulletinimg = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var jele = $(element);
        var baseurl = valueAccessor();
        var all = allBindingsAccessor();
        baseurl = baseurl + all.root.Layout.GetImgTileArgs();
        jele.attr('src', baseurl);
        jele.attr('alt', all.alt);

        var overlays = jele.siblings('.detail-overlay');
//        var toolbar = jele.siblings('.toolbar');
//        
//        overlays.height(overlays.height());
        overlays.mouseenter(function () { overlays.css('opacity', '0.8'); });
        overlays.mouseleave(function () { overlays.css('opacity', '0.0'); });
    },

    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {

    }
};

ko.bindingHandlers.absolutePosFromScroll = {
    update: function(element, valueAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor());
        if (value) {
            $(element).fadeIn();
            var top = $(window).scrollTop() + 100;
            $(element).css({
                'top': top
            });
        } else {
            $(element).fadeOut();
        }
    }
};

ko.bindingHandlers.dateString = {
    update: function (element, valueAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor());
        var dateNum = Date.parse(value);
        if (!dateNum)
            return;
        var d = new Date(dateNum);

// add parameter for binding to utc if we want        
//        var Y = d.getFullYear();
//        var M = d.getMonth();
//        var D = d.getDate();
//        var h = d.getHours();
//        var m = d.getMinutes();
//        var s = d.getSeconds();
//        var dateSetUtc = new Date(Date.UTC(Y, M, D, h, m, s));
//        $(element).text(dateSetUtc.toString());

        $(element).text(d.toString());
    }
};

//remove if they add this to core knockout. Just adds a bunch of elements and notifies once
ko.observableArray.fn.pushAll = function (valuesToPush) {
    var underlyingArray = this();
    this.valueWillMutate();
    ko.utils.arrayPushAll(underlyingArray, valuesToPush);
    this.valueHasMutated();
    return this;  //optional
};
