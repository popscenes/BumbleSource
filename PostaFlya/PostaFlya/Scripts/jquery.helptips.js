(function ($) {
    var $jQ = $;


    $jQ.tooltip = {


        parseElement: function (element) {

            var $element = $(element);
        },

        parse: function (selector) {

            $(selector).find("[data-tooltip]").each(function () {
                $jQ.tooltip.parseElement(this);
            });

        }
    };



    $(function () {
        $jQ.tooltip.parse(document);
    });
}(jQuery));



(function ($) {

    var defaults = {
        position: "bottomleft",
        distance: ""
    };
    
    var methods = {
        initElement: function ($element) {

            var tooltip = $("<div>")
                .addClass("ui-tooltip");
                
        },
        init: function () {
            return this.each(function () {
                methods.initElement($(this));
            });
        },
        showHelp: function (showOrHide) {

            return this.each(function () {

                var $this = $(this),
                    $data = $this.data('helptips');

                if (!$data) {
                    methods.initElement($this);
                    $data = $this.data('helptips');
                }

                $data.toggle(showOrHide);
            });
        },

    };

    $.fn.helptips = function (method) {

        // Method calling logic
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        } else {
            $.error('Method ' + method + ' does not exist on jQuery.helptips');
        }
        return this;
    };

})(jQuery);
