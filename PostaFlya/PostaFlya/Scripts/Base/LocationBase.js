/**/
(function(window, undefined) {

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


        google.maps.event.trigger(map, 'resize');
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
        
        
    };

    bf.createMap = function(mapElement) {
        var mapOptions = {
            zoom: 11,
            mapTypeId: google.maps.MapTypeId.ROADMAP,
        };

        map = new google.maps.Map(document.getElementById(mapElement),
                mapOptions);

        return map;
    };

    bf.setMapCircleDistance = function (map, circles, radius) {
        for (var j = 0; j < circles.length; j++) {
            circles[j].setRadius(radius*1000);
        }

        map.setZoom(bf.getZoomLevel(radius));
    };
    
    /**/
    bf.reverseGeocode = function(latitude, longitude, locationModel) {
        var latlng = new google.maps.LatLng(latitude, longitude);
        var geocoder = new google.maps.Geocoder();
        geocoder.geocode({ 'location': latlng },
            function (results, status) {
                if (status === 'OK') {
                    locationModel.SetFromGeo(results[0]);
                }
            });
    };

    bf.LocationSearchAutoComplete = function (autoComplete, map, updateLocation) {

        var $input = $("#" + autoComplete);
        
        $input.autocomplete({
            source: function (request, response) {
                var geocoder = new google.maps.Geocoder();
                geocoder.geocode({ 'address': request.term },
                    function (results, status) {
                        if (status == google.maps.GeocoderStatus.OK) {
                            response($.map(results, function (item) {
                            var loc = new bf.LocationModel({Longitude: item.geometry.location.lng(), Latitude: item.geometry.location.lat() });
                            loc.SetFromGeo(item);
                            return { label: item.formatted_address, value: item.formatted_address, position: loc };
                        }));
                    }
                });
            },
            minLength: 3,
            select: function(event, ui) {
                updateLocation(
                    ui.item.position
                );
            },
            open: function() { $(this).removeClass("ui-corner-all").addClass("ui-corner-top"); },
            close: function() { $(this).removeClass("ui-corner-top").addClass("ui-corner-all"); }
        });
    };
})(window);
/**/