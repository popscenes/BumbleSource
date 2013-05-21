(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};

    bf.BehaviourViewModel = function (behaviourViewModelFactory, rootPath) {
        var self = this;

        self.viewModFactory = behaviourViewModelFactory;
        self.selectedDetail = ko.observable();
        self.CreateFlierInstance = bf.globalCreateFlierInstance;
        
        self._Init = function () {

            if (bf.pageState !== undefined && bf.pageState.FlierDetail !== undefined) {
                self.selectedDetail(self.viewModFactory.createViewModel(bf.pageState.FlierDetail));
            } else {
                self.viewModFactory.getSelectedDetail(rootPath, self.selectedDetail);
            }
                       
            ko.applyBindings(self);
        };

        self._Init();

    };


})(window, jQuery);