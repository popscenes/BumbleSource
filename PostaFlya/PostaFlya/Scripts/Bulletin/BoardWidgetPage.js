(function (window, $, undefined) {


    var bf = window.bf = window.bf || {};
    bf.pageinit = bf.pageinit || {};
    bf.pageinit['board-page'] = function () {

//        bf.page = new bf.BoardPage(
//            new bf.WidgetSelectedFlierViewModel(new bf.BehaviourViewModelFactory())
//            , new bf.TagsSelector({
//                displayInline: true
//            })
//            , new bf.TileLayoutViewModel('#bulletinboard', new bf.BulletinLayoutProperties())
//            , bf.boardid
//        );
//
//        var endscroll = new EndlessScroll(window, {
//            fireOnce: false,
//            fireDelay: false,
//            content: false,
//            loader: "",
//            bottomPixels: 300,
//            insertAfter: "#bulletinboard",
//            resetCounter: function (num) {
//                bf.page.GetMoreFliers();
//                return false;
//            }
//
//        });
        //        endscroll.run();
        
        bf.page = new bf.BoardPage(new bf.WidgetSelectedFlierViewModel(new bf.BehaviourViewModelFactory())
            , bf.boardid);
        bf.page._Init();
        $('#popsceneswidget').show();
    };
    

    bf.WidgetSelectedFlierViewModel = function (behaviourViewModelFactory) {
        var self = this;

        self.viewModFactory = behaviourViewModelFactory;

        self.getDetailUrl = function (flier) {
            return bf.widgetbase + '/' + flier.FriendlyId;
        };

        self.hideDetailView = function () {
            self.viewModFactory.hideSelectedDetail(self.SelectedDetail);
        };

        self.showDetails = function (flier) {
            self.viewModFactory.getSelectedDetail(null, self.SelectedDetail, flier.FriendlyId);
        };

        self.getDetailTemplate = function (flier) {
            return self.viewModFactory.getDetailTemplate(flier);
        };

        self.SelectedDetail = ko.observable();

        self._Init = function () {

        };

        self._Init();

    };

    bf.BoardPage = function (selectedDetailViewModel
        , boardId) {
        var self = this;
        self.SelectedViewModel = selectedDetailViewModel;
        bf.GigGuideMixin(self);
        self.MinPs = 7;

        self.boardId = boardId;


        self.GetReqUrl = function () {
            return bf.widgetbase + '/mobileapi/board/' + self.boardId + '/gigs';
        };
        
        self.GetReqArgs = function (nextpage) {

            var params = {
                BoardId: self.boardId
            };

            var date = bf.getDateFromHash();
            if (date) {
                params.Start = date.toISOOffsetString();
            }

            return params;
        };

        self._Init = function () {

            ko.applyBindings(self);

            self.TryRequest();

        };

    };

})(window, jQuery);
