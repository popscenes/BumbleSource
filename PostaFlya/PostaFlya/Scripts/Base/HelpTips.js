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

        };

        self._Init = function() {
            self.showHelp.subscribe(function (newValue) {
                $(window.document.body).helptips('showHelp'
                    , newValue
                    , { closeHref: "javascript:bf.HelpTipsInstance.ToggleHelp();" });
            });
        };
        self._Init();

    };

    $(function () {
        bf.HelpTipsInstance = new bf.HelpTips();
    });


})(window);