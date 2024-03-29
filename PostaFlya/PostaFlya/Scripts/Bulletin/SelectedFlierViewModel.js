﻿(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};

    bf.SelectedFlierViewModel = function (behaviourViewModelFactory) {
        var self = this;

        self.viewModFactory = behaviourViewModelFactory;
        self.rootPath = self.viewModFactory.getSammyRoot();
        self.initPath = self.viewModFactory.getInitPath();

        self.getDetailUrl = function (flier) {
            return '/' + flier.FriendlyId;
        };

        self.hideDetailView = function (data, event) {          
            self.Sam.setLocation(self.LastLocation || self.initPath);
        };

        self.showDetails = function (flier) {
            self.LastLocation = self.Sam.getLocation();            
            self.Sam.setLocation(self.viewModFactory.getDetailPath(self.rootPath, flier));
        };

        self.getDetailTemplate = function (flier) {
            return self.viewModFactory.getDetailTemplate(flier);
        };

        self.addDetailRoutes = function (sam) {
            self.Sam = sam;
            self.viewModFactory.addSammyRoutes(self.rootPath, sam, self.SelectedDetail, self.initPath);
        };

        self.runSammy = function (sam) {
            sam.run(self.initPath);
        };

        self.SelectedDetail = ko.observable();

        self._Init = function () {
            //TODO make view models bind to their own parts of the document
            //ko.applyBindings(self, $('#flier-detail-div').get(0));
        };

        self._Init();

    };

})(window, jQuery); 