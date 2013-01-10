/**/
(function(window, undefined) {

    var bf = window.bf = window.bf || {};
    
    bf.SetMapPosition = function(map, longitude, latitude) {
        var clientPosition = new google.maps.LatLng(latitude, longitude);

        map.gmap('clear', 'markers');

        map.gmap('addMarker', { 'position': clientPosition, 'bounds': true });
        map.gmap('option', 'center', clientPosition);


        map.gmap('option', 'zoom', 11);
    };

    bf.circle = new google.maps.Circle();

    bf.setMapCircle = function (map, longitude, latitude, radius) {
        var clientPosition = new google.maps.LatLng(latitude, longitude);
        bf.circle.setMap(null);
        var mapOptions = {
            strokeColor: "#FF0000",
            strokeOpacity: 0.8,
            strokeWeight: 2,
            fillColor: "#FF0000",
            fillOpacity: 0.35,
            map: map.gmap('get', 'map'),
            center: clientPosition,
            radius: radius*1000
        };
        
        bf.circle = new google.maps.Circle(mapOptions);
    };
    
    /**/
    bf.LocationSearchAutoComplete = function(autoComplete, map, updateLocation) {
        var self = this;
        self.autoComplete = autoComplete;
        self.map = map;
        self.updateLocation = updateLocation;

        self.autoComplete.autocomplete({
            source: function(request, response) {
                self.map.gmap('search', { 'address': request.term },
                    function(results, status) {
                        if (status === 'OK') {
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
                self.updateLocation(
                    ui.item.position
                    //new bf.LocationModel({ Description: ui.item.label, Longitude: ui.item.position.lng(), Latitude: ui.item.position.lat() })
                );
            },
            open: function() { $(this).removeClass("ui-corner-all").addClass("ui-corner-top"); },
            close: function() { $(this).removeClass("ui-corner-top").addClass("ui-corner-all"); }
        });
    };
})(window);
/**/