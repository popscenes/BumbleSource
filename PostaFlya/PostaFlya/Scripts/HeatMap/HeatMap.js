(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    ko.bindingHandlers.heatMapBinding = {
        update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            SetMapPosition($('#heatMap'), viewModel.currentLocation().Longitude, viewModel.currentLocation().Latitude);
            viewModel.loadHeatMapPoints();
        }
    };

    bf.HeatMapGet = function () {
        var self = this;
        var heatmap;

        self.currentLocation = ko.observable({ Description: '', Longitude: 0, Latitude: 0 });

        self.loadHeatMapPoints = function () {
            var url = sprintf('/api/HeatMapApi?loc.Latitude=%f&loc.Longitude=%f',
                self.currentLocation().Latitude, self.currentLocation().Longitude);
            $.getJSON(url, function (allData) {

                if (allData.length < 1)
                    return;
                
                var heatMapDataset =
                    {
                        max: 100,
                        data: allData
                    };

                if (heatmap != null)
                    heatmap.setDataSet(heatMapDataset);

            });
        };


        self._Init = function () {
            ko.applyBindings(self);
            $('#heatMap').gmap().bind('init', function (ev, map) {
                heatmap = new HeatmapOverlay(map, { "radius": 20, "visible": true, "opacity": 60 });
            });

            LocationSearchAutoComplete($("#locationSearch"), $('#heatMap'), self.currentLocation);

        };

        self._Init();

    };

})(window);