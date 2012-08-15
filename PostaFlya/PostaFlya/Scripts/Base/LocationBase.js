﻿/**/
SetMapPosition = function(map, longitude, latitude) {
    var clientPosition = new google.maps.LatLng(latitude, longitude);

    map.gmap('clear', 'markers');

    map.gmap('addMarker', { 'position': clientPosition, 'bounds': true });
    map.gmap('option', 'center', clientPosition);


    map.gmap('option', 'zoom', 15);
};
/**/
LocationSearchAutoComplete = function(autoComplete, map, updateLocation) {
    var self = this;
    self.autoComplete = autoComplete;
    self.map = map;
    self.updateLocation = updateLocation;

    self.autoComplete.autocomplete({
        source: function(request, response) {
            self.map.gmap('search', { 'address': request.term },
                function(results, status) {
                    if (status === 'OK') {
                        response($.map(results, function(item) {
                            return { label: item.formatted_address, value: item.formatted_address, position: item.geometry.location };
                        }));
                    }
                });
        },

        minLength: 3,
        select: function(event, ui) {
            self.updateLocation({ Description: ui.item.label, Longitude: ui.item.position.lng(), Latitude: ui.item.position.lat() });
        },
        open: function() { $(this).removeClass("ui-corner-all").addClass("ui-corner-top"); },
        close: function() { $(this).removeClass("ui-corner-top").addClass("ui-corner-all"); }
    });
};
/**/