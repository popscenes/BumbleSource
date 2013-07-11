(function (window, $, undefined) {


    var bf = window.bf = window.bf || {};
    bf.pageinit = bf.pageinit || {};
    bf.pageinit['board-page'] = function () {

//        bf.page = new bf.BoardPage(
//            new bf.SelectedFlierViewModel(new bf.BehaviourViewModelFactory())
//            , new bf.TagsSelector({
//                displayInline: true
//            })
//            , new bf.TileLayoutViewModel('#bulletinboard', new bf.BulletinLayoutProperties())
//            , $('#boardid').val()  
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
        
        bf.page = new bf.BoardPage(new bf.SelectedFlierViewModel(new bf.BehaviourViewModelFactory())
            , $('#boardid').val());
        bf.page._Init();
    };
    
    bf.BoardPage = function (selectedDetailViewModel, boardId) {
        var self = this;
        self.SelectedViewModel = selectedDetailViewModel;
        bf.GigGuideMixin(self);
        self.MinPs = 7;

        self.boardId = boardId;

        //mobileapi/gigs/bydate?lat=-37.769&lng=144.979&distance=10&start=2013-06-29&end=2013-07-02
        self.GetReqUrl = function () {
            return 'mobileapi/board/'+ self.boardId +'/gigs';
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

        self.Sam = Sammy('#bulletinboard');
        self.SelectedViewModel.addDetailRoutes(self.Sam);
        self.AddGetDateRoute(self.Sam);


        self._Init = function () {

            ko.applyBindings(self);
            
            self.SelectedViewModel.runSammy(self.Sam);

            //self.TryRequest();

        };

    };


})(window, jQuery);
