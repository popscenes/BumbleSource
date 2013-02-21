/**/
(function (window, undefined) {
    var bf = window.bf = window.bf || {};

    bf.HelpTips = function () {
        var self = this;
        
        self.showHelp = ko.observable(false);
        
        self.ToggleHelp = function () {
            self.showHelp(!self.showHelp());            
        };

        self.CheckFirstShowFor = function(context) {

            $.cookie.json = true;
            var helptipsshown = $.cookie('helptipsshown');
            if (!helptipsshown)
                helptipsshown = {};
            
            if (!helptipsshown[context]) {
                self.showHelp(true);
            }

            helptipsshown[context] = true;
            $.cookie('helptipsshown', helptipsshown, { expires: 1000});

        };

        self._Init = function() {
            self.showHelp.subscribe(function (newValue) {
                $(window.document.body).helptips('showHelp'
                    , newValue
                    , {
                        closeHref: "javascript:bf.HelpTipsInstance.ToggleHelp();",
                        closeBtnClassAdd: 'mini-button red-button',
                        nextBtnClassAdd: 'mini-button blue-button',
                    });
            });
        };
        self._Init();

    };

    $(function () {
        bf.HelpTipsInstance = new bf.HelpTips();
    });


})(window);