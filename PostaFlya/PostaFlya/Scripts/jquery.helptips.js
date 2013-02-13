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
        position: "bottomright",
    };

    function initElement($element) {
        var $tooltip = $element.data('helptips');
        if ($tooltip)
            return;

        $tooltip = $('<div>')
            .addClass("helptips-container")
            .hide();

        $("<div>")
            .addClass("helptips-content")
            .text($element.attr("data-helptip-text"))
            .appendTo($tooltip);

        $tooltip.insertAfter($element);
        $element.data('helptips', $tooltip)
        $tooltip.appendTo($element.document[0].body);
    }

    function init($eles) {
        $eles = $eles.filter('[data-helptip-text]');

        $eles.not('[helptips-group]').each(function () {
            initElement($(this));
        });

        $eles.filter('[helptips-group]').each(function () {
            initGroup($(this));
        });

        return $eles;
    }

    function getContent($group) {
        var $ret = $([]);
        $group.each(function () {
            $this = $(this);
            var $row = $('<div>')
                .append($this.clone().removeAttr('data-helptip-text').removeAttr('id').addClass('helptips-icon'))
                .append($this.attr("data-helptip-text"));

            $ret.add($row);
                
        });
        return ret;
    }

    function initGroup($groupEle, $allGroup){

        var $tooltip = $groupEle.data('helptips');
        if ($tooltip)
            return;

        $tooltip = $('<div>')
            .addClass("helptips-container")
            .hide();

        var $contentCont = $("<div>")
            .addClass("helptips-content")
            .appendTo($tooltip);

        var groupId = $groupEle.attr('helptips-group');
        var $group = $allGroup.filter('[helptips-group=' + groupId + ']');
        var $content = methods.getContent($group);
        $content.appendTo($contentCont);
        $group.each(function () {
            $(this).data('helptips', $tooltip);
        });

        $tooltip.appendTo($element.document[0].body);
    }

    function showHelpForEles($ele, showOrHide) {
        var $tooltip = $ele.first().data('helptips');
        $tooltip.remove();
        if (showOrHide) {
            var pos = getPositionFor($ele);
            $tooltip.css("top", pos.top);
            $tooltip.css("right", pos.right);
            $tooltip.insertAfter($ele.last());
            $tooltip.show();
        }
        else {
            $tooltip.appendTo($ele.document[0].body);
            $tooltip.hide();
        }
    }

    function showHelpFor($eles, showOrHide) {

        $eles.not('[helptips-group]').each(function () {
            showHelpForEles($(this), showOrHide);
        });

        var done = {};
        $eles.filter('[helptips-group]').each(function () {
            var $this = $(this);
            var groupid = $this.attr('helptips-group');
            if (!done[groupid]) {
                showHelpForEles($eles.filter('[helptips-group=' + groupid + ']'));
                done[groupid] = true;
            }

        });
    };

    function getPositionFor($eles) {
        var pos = {top: 0, right:0};
        $eles.each(function () {
            var $this = $(this);
            var posEle = $this.position();
            if (posEle.left + $this.width() > pos.right)
                pos.right = posEle.left + $this.width();
            if (posEle.top + $this.height() > pos.top)
                pos.top = posEle.top + $this.height();
        });
        return pos;
    }
    
    var publicmethods = {

        init: function () {
            init(this);
        },

        showHelp: function (showOrHide) {
            var $eles = init(this);

            showHelpFor($eles, showOrHide);

            return this;         
        },

    };

    $.fn.helptips = function (method) {

        // Method calling logic
        if (publicmethods[method]) {
            return publicmethods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return publicmethods.init.apply(this, arguments);
        } else {
            $.error('Method ' + method + ' does not exist on jQuery.helptips');
        }
        return this;
    };

})(jQuery);
