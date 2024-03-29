﻿(function(window, $, undefined) {


    ko.bindingHandlers.markdown = {
        update: function(element, valueAccessor) {
            var markdownValue = ko.utils.unwrapObservable(valueAccessor());
            var htmlValue = markdownValue && new Showdown.converter().makeHtml(markdownValue);
            $(element).html(htmlValue || "");
        }
    };


    ko.bindingHandlers.dateToggle = {
        init: function(element, valueAccessor, allBindingsAccessor, viewModel) {
            $(element).bind('click', function() {
                $("#" + valueAccessor()).datepicker("show");
            });
        }
    };

    ko.bindingHandlers.datePicker = {
        update: function(element, valueAccessor, allBindingsAccessor, viewModel) {
            $(element).datepicker(valueAccessor()); //.datepicker("setDate", new Date(viewModel.EventDates()));    
        }
    };

    //ko.bindingHandlers.dateTimePickerUpdate = {
    //    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
    //        var date = ko.utils.unwrapObservable(valueAccessor());
    //        $(element).datetimepicker("setDate", new Date(date));
    //    }
    //};

    ko.bindingHandlers.simpletext = {
        update: function(element, valueAccessor) {
            var $ele = $(element);
            var val = valueAccessor();

            $ele.empty();
            $.each(val.split("\n"), function(intIndex, objValue) {
                $("<span>").text(objValue, objValue).appendTo($ele);
                $("<br />").appendTo($ele);
            });
        }
    };


    //ko.bindingHandlers.imageSelectorBinding = {
    //    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
    //        viewModel.Init();
    //    }
    //};

    //$.fn.selectRange = function (start, end) {
    //    return this.each(function () {
    //        if (this.setSelectionRange) {
    //
    //            this.setSelectionRange(start, end);
    //        } else if (this.createTextRange) {
    //            var range = this.createTextRange();
    //            range.collapse(true);
    //            range.moveEnd('character', end);
    //            range.moveStart('character', start);
    //            range.select();
    //        }
    //    });
    //};


    ko.bindingHandlers.bannerText = {
        init: function(element, valueAccessor, allBindingsAccessor, viewModel) {

            var $input = $(element);
            var banner = ko.utils.unwrapObservable(allBindingsAccessor().bannerText);

            $input.attr('placeholder', banner);

            $input.focus(function() {
                if ($input.val() == banner)
                    $input.val('')
                        .removeClass('banner-text');
            }).blur(function() {
                if ($input.val() == '')
                    $input.val(banner)
                        .addClass('banner-text');
            });
        },
        update: function(element, valueAccessor, allBindingsAccessor, viewModel) {

        }
    };

    ko.bindingHandlers.touchHover = {
        init: function(element, valueAccessor, allBindingsAccessor, viewModel) {
            $(element).bind('touchend', function(e) {
                e.preventDefault();
                //alert("toggle class");
                $(valueAccessor()).toggleClass("touch-hover");
            });
        }
    };

    ko.bindingHandlers.linkInput = {
        init: function(element, valueAccessor, allBindingsAccessor, viewModel) {
            $(element).bind('focus', function(e) {
                e.preventDefault();
                //alert("toggle class");
                valueAccessor()("http://");
            });
        }
    };

    ko.bindingHandlers.placeAutoComplete = {
        init: function(element, valueAccessor, allBindingsAccessor, viewModel) {

            var options = {
                
            };

            var autocomplete = new google.maps.places.Autocomplete(element, options);
            google.maps.event.addListener(autocomplete, 'place_changed', function() {

                var venue = null;
                var item = autocomplete.getPlace();
                if (item.geometry) {

                    var data = {
                        PhoneNumber: item.formatted_phone_number,
                        WebSite: item.website,
                        Source: 'Google Place',
                        SourceId: item.reference,
                        SourceUrl: item.url,
                        PlaceName: item.name,
                        UtcOffset: item.utc_offset
                    };
                    var isEstablishment = $.inArray("establishment", item.types) > -1;
                    venue = new bf.VenueInformationModel(isEstablishment ? data : {});
                    venue.Address().SetFromGeo(item);
                }

                setTimeout(function() {
                    var venueObservable = valueAccessor();
                    venueObservable(venue);
                });

            });

            var $input = $(element);
            $input.focus(function() {
                $input.val('');

            }).blur(function() {
                var venue = ko.utils.unwrapObservable(valueAccessor());
                if (venue != null && venue.Description())
                    $input.val(venue.Description());
            });

        },
        update: function(element, valueAccessor, allBindingsAccessor, viewModel) {


            var venue = ko.utils.unwrapObservable(valueAccessor());
            if (venue != null && venue.Description())
                $(element).val(venue.Description());
            if (venue != null && venue.Address().ValidLocation() && venue.Description() === "")
                bf.reverseGeocode(venue);

        }
    };


    ko.bindingHandlers.locationAutoComplete = {
        init: function(element, valueAccessor, allBindingsAccessor, viewModel) {

            var $input = $(element);
            $input.focus(function() {
                var location = ko.utils.unwrapObservable(valueAccessor());
                if (location != null && location.Description() == $input.val())
                    $input.val('');

            }).blur(function() {
                var location = ko.utils.unwrapObservable(valueAccessor());
                if (location != null && location.Description())
                    $input.val(location.Description());
            });
        

            var eventFromSelector = false;
            $input.autocomplete({
                source: function (request, response) {

                    bf.lookupPlaceFromTerms(request.term)
                        .done(function(resp) {
                            response($.map(resp.Data, function (item) {
                                var loc = new bf.LocationModel({ SuburbId: item.Id, SuburbDesc: item.Description });
                                return { label: loc.Description(), value: loc.Description(), position: loc };
                            }));
                        });


//                    var geocoder = new google.maps.Geocoder();
//                    geocoder.geocode({ 'address': request.term },
//                        function(results, status) {
//                            if (status == google.maps.GeocoderStatus.OK) {
//                                response($.map(results, function(item) {
//                                    var loc = new bf.LocationModel({ Longitude: item.geometry.location.lng(), Latitude: item.geometry.location.lat() });
//                                    loc.SetFromGeo(item);
//                                    return { label: loc.Description(), value: loc.Description(), position: loc };
//                                }));
//                            }
//                        });
                },
                minLength: 3,
                select: function(event, ui) {
                    var location = valueAccessor();
                    location(ui.item.position);
                },
                open: function(event, ui) {
                    if ($input.siblings(".current-loc").length > 0) {
                       // $('ul.ui-autocomplete').css('left', function(index, value) { return ($input.siblings(".current-loc").outerWidth(true) * -1) + parseInt(value) - 1; });
                    }
                    //$('ul.ui-autocomplete').css('top', function(index, value) { return 1 + parseInt(value); });
                },
                close: function() { $(this).removeClass("ui-corner-top").addClass("ui-corner-all"); }
            });

            var changeHandler = function() {
                var desc = $input.val();
                var location = valueAccessor();
                var loc = location();
                if (!desc && !loc)
                    return;
                if (loc && (desc == loc.Description()))
                    return;
                if (!desc) {
                    location(null);
                    return;
                }
                $input.autocomplete("close");

                bf.lookupPlaceFromTerms(desc)
                    .done(function (resp) {
                        
                        if (resp.Data.length > 0) {
                            var item = results[0];
                            location(new bf.LocationModel({ SuburbId: item.Id, SuburbDesc: item.Description }));
                        } else {
                            location(null);
                        }

                    }).error(function() {
                        location(null);
                    });

//                var geocoder = new google.maps.Geocoder();
//                geocoder.geocode({ 'address': desc },
//                    function(results, status) {
//                        if (status == google.maps.GeocoderStatus.OK) {
//                            var item = results[0];
//                            var newLoc = new bf.LocationModel({ Longitude: item.geometry.location.lng(), Latitude: item.geometry.location.lat() });
//                            newLoc.SetFromGeo(item);
//                            location(newLoc);
//                        } else {
//                            location(null);
//                        }
//                    });
            };
            ko.utils.registerEventHandler(element, 'change', changeHandler);


        },
        update: function(element, valueAccessor, allBindingsAccessor, viewModel) {

            var location = ko.utils.unwrapObservable(valueAccessor());
            if (location != null && location.Description())
                $(element).val(location.Description());
            if (location != null && location.ValidLocation() && location.Description() === "")
                bf.reverseGeocode(location);

        }
    };

    ko.bindingHandlers.showOnce = {
        init: function(element, valueAccessor, allBindingsAccessor, viewModel) {
            var context = ko.utils.unwrapObservable(valueAccessor());

            $.cookie.json = true;
            var showonce = $.cookie('showOnce');
            if (!showonce)
                showonce = {};

            if (!showonce[context]) {
                $(element).show();
            } else {
                $(element).hide();
            }

            showonce[context] = true;
            $.cookie('showOnce', showonce, { expires: 1000 });
        }
    };

    ko.bindingHandlers.mapBinding = {
        init: function(element, valueAccessor, allBindingsAccessor, viewModel) {
            //        var map = bf.createMap(element);    
            //        $.data(element, 'map', map);
            //        $.data(element, 'circles', []);
            //        $.data(element, 'markers', []);
            //        $.data(element, 'beenshown', false);
        },
        update: function(element, valueAccessor, allBindingsAccessor, viewModel) {

            var visibleTrig = allBindingsAccessor().visibleTrigger;
            if (visibleTrig && !visibleTrig() || (!$(element).is(':visible')))
                return;

            var map = $.data(element, 'map');
            if (!map) {
                map = bf.createMap(element);
                $.data(element, 'map', map);
                $.data(element, 'circles', []);
                $.data(element, 'markers', []);
                google.maps.event.trigger(map, 'resize');
            }
            var location = ko.utils.unwrapObservable(valueAccessor());

            var distance = allBindingsAccessor().distance;
            distance = (distance === undefined) ? 0 : ko.utils.unwrapObservable(distance);
            var circles = $.data(element, 'circles');
            var markers = $.data(element, 'markers');
            bf.SetMapPosition(map, location.Longitude(), location.Latitude(), distance, markers, circles);
        }
    };

    ko.bindingHandlers.distanceSlider = {
        init: function(element, valueAccessor, allBindingsAccessor, viewModel) {

            var value = ko.utils.unwrapObservable(valueAccessor());

            $(element).slider({ value: value, min: 5, max: 30 });

            $(element).slider({
                change: function(event, ui) {
                    var obs = valueAccessor();
                    obs($(element).slider("value"));
                }
            });


        }
    };

    ko.bindingHandlers.distanceDropDown = {
        init: function(element, valueAccessor, allBindingsAccessor, viewModel) {
            var value = ko.utils.unwrapObservable(valueAccessor());

            for (var distVal = 5; distVal <= 30; distVal = distVal + 5) {
                var distElem = $('<li></li>');
                distElem.addClass("distance-choice");
                distElem.attr("data-distance", distVal);
                distElem.text(distVal + "km");

                distElem.click(function() {
                    var obs = valueAccessor();
                    obs($(this).attr("data-distance"));
                });

                distElem.bind("touchend", function() {
                    var obs = valueAccessor();
                    obs($(this).attr("data-distance"));
                });

                $(element).append(distElem);
            }
        }
    };

    //basically takes the class input and appends 'a', 'b', 'c' depending on the position of
    //the model in the parent c
    ko.bindingHandlers.cssAbcCollection = {
        update: function(element, valueAccessor, allBindingsAccessor, viewModel) {

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
        init: function(element, valueAccessor, allBindingsAccessor, viewModel) {
            var jele = $(element);
            var layout = valueAccessor();
            jele.addClass(layout.GetTileClass());
        },

        update: function(element, valueAccessor, allBindingsAccessor, viewModel) {

        }
    };

    ko.bindingHandlers.updateLayout = {
        init: function(element, valueAccessor, allBindingsAccessor, viewModel) {
        },

        update: function(element, valueAccessor, allBindingsAccessor, viewModel) {
            var layout = valueAccessor();
            layout.Update();
        }
    };

    ko.bindingHandlers.imgRetry = {
        init: function(element, valueAccessor, allBindingsAccessor, viewModel) {

            var jele = $(element);
            jele.data("reload-times", 0);
            var src = jele.attr("src");
            if (!src) {
                jele.removeAttr('src');
                var val = valueAccessor();
                src = ko.utils.unwrapObservable(val);
            }

            if (!src)
                return;

            jele.data("imgsrc", src);
            var imageReplacer = $('<img/>');
            imageReplacer.addClass('img-error');
            imageReplacer.hide();
            imageReplacer.insertAfter(jele);

            jele.bind("error", function() {
                var reloadTimes = jele.data("reload-times");
                var imgSrc = jele.data("imgsrc");
                var errorSrc = "/Img/GetError?id=" + imgSrc.substring(imgSrc.lastIndexOf('/') + 1);

                imageReplacer.attr('src', errorSrc);
                jele.hide();
                imageReplacer.show();
                //jele.attr("src", errorSrc);

                var timeOut = reloadTimes == 0 ? 0 : 1000;
                if (reloadTimes < 20) {
                    setTimeout(function() {
                        //jele.attr("src", null);
                        jele.attr("src", imgSrc + "?" + reloadTimes);
                        reloadTimes++;
                        jele.data("reload-times", reloadTimes);

                    }, timeOut);
                } else {
                    setTimeout(function() {
                        jele.addClass("img-load-failed");
                        jele.unbind("error");
                    });
                }
            });

            jele.bind("load", function() {
                jele.show();
                imageReplacer.hide();
            });

            jele.attr('src', src);
        },

        update: function(element, valueAccessor, allBindingsAccessor, viewModel) {

        }
    };

    ko.bindingHandlers.validate = {
        init: function(element, valueAccessor, allBindingsAccessor, viewModel) {
            var jele = $(element);

            jQuery.validator.unobtrusive.parse(jele);
            var validatorSettings = $.data(jele[0], 'validator').settings;
            validatorSettings.ignore = ".ignore *";
        },

        update: function(element, valueAccessor, allBindingsAccessor, viewModel) {
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
            validatorSettings.ignore = ".ignore *";


        }
    };

    ko.bindingHandlers.imgbluroverlay = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            
        },
        update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            var jele = $(element);

            var item = valueAccessor();

            if (!imagefilter.canRun() || !item)
                return;

            setTimeout(function() {
            
                var baseurl = bf.widgetbase ? bf.widgetbase : '';
                baseurl = baseurl + '/Img/ImgRet?imageUrl=' + encodeURIComponent(item);
                //var baseurl = item;

                var position = baseurl.indexOf(".jpg");
                var width = getFlierImageSizeFromWidth(jele.width());
                var url = [baseurl.slice(0, position), width, baseurl.slice(position)].join('');


                imagecanvas.overlayDiv(jele, url);
            }, 1);

        }
    };
    

    
    ko.bindingHandlers.bulletinimg = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            var jele = $(element);
            var baseurl = valueAccessor();
            var all = allBindingsAccessor();

            if (all.external) {
                jele.attr('src', baseurl);
                jele.attr('alt', all.alt);
                return;
            }

            var position = baseurl.indexOf(".jpg");
            var width = getFlierImageSizeFromWidth(jele.width());
            var url = [baseurl.slice(0, position), width, baseurl.slice(position)].join('');

            jele.attr('src', url);
            jele.attr('alt', all.alt);
        },

        update: function (element, valueAccessor, allBindingsAccessor, viewModel) {

        }
    };
    
    function getFlierImageSizeFromWidth(width) {

        if (width <= 150)
            return 'h150';
        if (width <= 228)
            return 'h228';
        if (width <= 450)
            return 'h450';

        return 'h900';

    };


    ko.bindingHandlers.flyerImg = {
        init: function(element, valueAccessor, allBindingsAccessor, viewModel) {
            var jele = $(element);
            var image = valueAccessor();
            var eleWidth = jele.width();
            var dimOrig = getDimensionFromSize(image, 0, 'Original');
            var wide = dimOrig.Width > dimOrig.Height;
            var dim = getDimensionFromSize(image, eleWidth, wide ? 'Square' : 'Horizontal');
            var url = image.BaseUrl + dim.UrlExtension;
            jele.attr('src', url);

        },

        update: function(element, valueAccessor, allBindingsAccessor, viewModel) {

        }
    };
    
    function getDimensionFromSize(image, size, axis, vertsize) {

        var dd = null;
        for (var d = 0 ; d < image.Extensions.length; d++) {
            if (image.Extensions[d].ScaleAxis != axis) continue;
            
            dd = image.Extensions[d];
            if (vertsize ? dd.Height >= size : dd.Width >= size) {
                break;
            }
        }
        return dd;
    }

    
    ko.bindingHandlers.fadeVisible = {
        init: function (element, valueAccessor) {
            // Initially set the element to be instantly visible/hidden depending on the value
            var value = valueAccessor();
            $(element).toggle(ko.utils.unwrapObservable(value)); // Use "unwrapObservable" so we can handle values that may or may not be observable
        },
        update: function (element, valueAccessor) {
            // Whenever the value subsequently changes, slowly fade the element in or out
            var value = valueAccessor();
            ko.utils.unwrapObservable(value) ? $(element).fadeIn() : $(element).fadeOut(2000);
        }
    };

    ko.bindingHandlers.absolutePosFromScroll = {
        update: function(element, valueAccessor, allBindingsAccessor) {

            var args = ko.utils.unwrapObservable(valueAccessor());
            var value = ko.utils.unwrapObservable(args.target);
            var $ele = $(element);
            if (value) {
                $ele.fadeIn();
                /*var top = $(window).scrollTop();
                $ele.css({
                    'top': top
                });*/
                
                var windowHeight = $(window).height();
                //var windowWidth = $(window).width();

                $("body").css({ 'height': windowHeight, 'overflow-y': 'hidden' });
                //$("header").css({ 'width': windowWidth });

                if (args.refreshtrig && value[args.refreshtrig] && ko.isObservable(value[args.refreshtrig])) {
                    value[args.refreshtrig].subscribe(function() {
                        window.scrollTo(0, top);
                    });
                }

            } else {
                $(element).fadeOut();
                
                //alert("fadeout");
                $("body").css({ 'height': 'auto', 'overflow-y': 'auto' });
                //$("header").css({ 'width': '100%' });
                //$(element).css({ 'height': 'auto' });
            }
        }
    };

    ko.bindingHandlers.dateString = {
        update: function(element, valueAccessor) {
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
    ko.observableArray.fn.pushAll = function(valuesToPush) {
        var underlyingArray = this();
        this.valueWillMutate();
        ko.utils.arrayPushAll(underlyingArray, valuesToPush);
        this.valueHasMutated();
        return this; //optional
    };
    
    
})(window, jQuery);




