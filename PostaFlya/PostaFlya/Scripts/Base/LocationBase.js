/**/
(function(window, $, undefined) {

    var bf = window.bf = window.bf || {};

    bf.getCurrentPosition = function(callback) {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                function(result) {
                    callback(result, 'OK');
                },
                function(error) {
                    callback(null, error);
                }
            );
        } else {
            callback(null, 'NOT_SUPPORTED');
        }
    };

    bf.getZoomLevel = function(radius) {
        
        if (radius < 10) {
            return 11;
        }
        else if (radius >= 10 && radius < 20) {
            return 10;
        }
        else if (radius >= 20 && radius < 30) {
            return 9;
        } else {
            return 8;
        }
    };
    
    bf.SetMapPosition = function(map, longitude, latitude, radius, markers, circles) {
        var clientPosition = new google.maps.LatLng(latitude, longitude);


        map.setMapTypeId(google.maps.MapTypeId.ROADMAP);
        if (latitude != -300 && latitude != -300) {
            map.setZoom(bf.getZoomLevel(radius));
            map.setCenter(clientPosition);
        } else {
            clientPosition = new google.maps.LatLng(0, 0);
            map.setZoom(2);
            map.setCenter(clientPosition);
            return;
        }
        

        for (var i = 0; i < markers.length; i++) {
            markers[i].setMap(null);
        }
        
        
        var marker = new google.maps.Marker({
            map: map,
            draggable: false,
            animation: google.maps.Animation.DROP,
            position: clientPosition
        });
        
        markers.push(marker);
        
        var circleCentre = new google.maps.LatLng(latitude, longitude);
        
        
        
        for (var j = 0; j < circles.length; j++) {
            circles[j].setMap(null);
        }

        circles.splice(0, circles.length);

        var circle = new google.maps.Circle({
            strokeColor: "#FF0000",
            strokeOpacity: 0.8,
            strokeWeight: 2,
            fillColor: "#FF0000",
            fillOpacity: 0.35,
            center: circleCentre,
            radius: radius * 1000,
            map: map
        });
        
        circles.push(circle);
        
        //google.maps.event.trigger(map, 'resize');
        
    };

    bf.createMap = function (mapElement) {
        
        var mapOptions = {
            zoom: 11,
            mapTypeId: google.maps.MapTypeId.ROADMAP,
        };

        var $element = $(mapElement);
        
        $element[0].style.display = "block";

        map = new google.maps.Map($element[0],
                mapOptions);

        return map;
    };

    bf.setMapCircleDistance = function (map, circles, radius) {
        for (var j = 0; j < circles.length; j++) {
            circles[j].setRadius(radius*1000);
        }

        map.setZoom(bf.getZoomLevel(radius));
    };

    bf.lookupPlaceFromTerms = function(terms) {

        return $.ajax(
            {
                dataType: (bf.widgetbase ? "jsonp" : "json"),
                url: "/webapi/search/byterms",
                crossDomain: (bf.widgetbase ? true : false),
                data: {q: terms}
            }
        );
    };
    
    /**/
    bf.reverseGeocode = function(locationModel, callback) {
        var latlng = new google.maps.LatLng(locationModel.Latitude(), locationModel.Longitude());
        var geocoder = new google.maps.Geocoder();
        geocoder.geocode({ 'location': latlng },
            function (results, status) {
                if (status === 'OK') {
                    locationModel.SetFromGeo(results[0]);
                    if (callback)
                        callback(locationModel);
                }
            });
    };
    
    
    bf.LocationSearch = function (searchText, response, geoservice) {

        if (!geoservice || geoservice == 'googlegeocode') {
            var geocoder = new google.maps.Geocoder();
            geocoder.geocode({ 'address': searchText },
                function (results, status) {
                    if (status == google.maps.GeocoderStatus.OK) {
                        response($.map(results, function(item) {
                            var loc = new bf.LocationModel({ Longitude: item.geometry.location.lng(), Latitude: item.geometry.location.lat() });
                            loc.SetFromGeo(item);
                            return loc;
                        }));
                    } else {
                        response([]);
                    }
                });
        }
        else if (geoservice == 'googleplaces') {
            
            var request = {
//                location: pyrmont,
//                radius: '500',
                query: searchText
            };

            var service = new google.maps.places.PlacesService();
            service.textSearch(request, function (results, status) {
                if (status == google.maps.places.PlacesServiceStatus.OK) {
                    response($.map(results, function (item) {
                        //var loc = new bf.LocationModel({ Longitude: item.geometry.location.lng(), Latitude: item.geometry.location.lat() });
                        //loc.SetFromGeo(item);
                        return null;
                    }));
                } else {
                    response([]);
                }
            });
            

        }
        else if (geoservice == 'geonames') {
            response([]);
        } else {
            response([]);
        }
        

    };


})(window, jQuery);
/**/