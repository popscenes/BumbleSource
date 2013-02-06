(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.MyFliers = function (imageSelector, tagsSelector, layout) {
        var self = this;
        self.apiUrl = sprintf("/api/Browser/%s/MyFliers", bf.currentBrowserInstance.BrowserId);
        //self.locationSelector = locationSelector;
        self.imageSelector = imageSelector;
        self.tagsSelector = tagsSelector;
        self.Layout = layout;

        self.CreateEditFlier = ko.observable();
        self.MyFliersList = ko.observable();
        
        self.ShowMyFliers = function () {
            $.ajax(self.apiUrl, {
                type: "get", contentType: "application/json",
                success: function (result) {
                    self.MyFliersList(result);
                    self.CreateEditFlier(null);
                }
            });
        };

        self.editFlier = function (flier) {
            $.ajax(self.apiUrl + "/" + flier.Id, {
                type: "get", contentType: "application/json",
                success: function (result) {
                    var editFlier = new bf.CreateEditFlier(result, self.imageSelector, self.tagsSelector, self.ShowMyFliers);
                    self.MyFliersList(null);
                    self.CreateEditFlier(editFlier);
                    editFlier.InitControls();
                }
            });
        };

        self.CreateFlier = function () {
            var emptyFlier = new bf.CreateEditFlier({}, self.imageSelector, self.tagsSelector, self.ShowMyFliers);

            self.CreateEditFlier(emptyFlier);
            self.MyFliersList(null);
            self.imageSelector.Init();
            emptyFlier.InitControls();
        };


        self._Init = function () {
            ko.applyBindings(self);
            self.ShowMyFliers();
        };

        self._Init();

    };

})(window);