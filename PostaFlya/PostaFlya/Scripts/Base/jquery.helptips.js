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
        
        var id = $element.attr('id');
        if (!id)
            id = $element.attr('name');
        if (id)
            $tooltip.attr('id', id + '-helptip');
        
        var $arrowCont = $("<div>")
            .addClass("helptips-arrow")
            .appendTo($tooltip);

        $("<div>")
            .addClass("helptips-content")
            .text($element.attr("data-helptip-text"))
            .appendTo($tooltip);

        $tooltip.insertAfter($element);
        $element.data('helptips', $tooltip);
    }

    function init($eles) {
        if (!$eles.is('[data-helptip-text]'))
            $eles = $eles.find('[data-helptip-text], [data-helptip-group]');

        $eles.not('[data-helptip-group]').each(function () {
            initElement($(this));
        });

        var $groupEles = $eles.filter('[data-helptip-group]');
        $groupEles.each(function () {
            initGroup($(this), $groupEles);
        });

        return $eles;
    }

    function getContent($group) {
        var $ret = $([]);
        $group.each(function () {
            var $this = $(this);
            var $row = $('<div>')
                .append($this.clone()
                    .removeAttr('data-helptip-text')
                    .removeAttr('data-helptip-group')
                    .removeAttr('id')
                    .addClass('helptips-group-element'))
                .append(
                    $('<div>')
                    .addClass('helptips-group-text')
                    .text($this.attr("data-helptip-text")));

            $ret = $ret.add($row);
                
        });
        return $ret;
    }

    function initGroup($groupEle, $allGroup){

        var $tooltip = $groupEle.data('helptips');
        if ($tooltip)
            return;

        var groupId = $groupEle.attr('data-helptip-group');
        
        $tooltip = $('<div>')
            .addClass("helptips-container")
            .attr("id", groupId + '-helptip')
            .hide();
        
        var $arrowCont = $("<div>")
            .addClass("helptips-arrow")
            .appendTo($tooltip);

        var $contentCont = $("<div>")
            .addClass("helptips-content")
            .appendTo($tooltip);

        
        var $group = $allGroup.filter('[data-helptip-group=' + groupId + ']');
        var $content = getContent($group.filter('[data-helptip-text]'));
        $content.appendTo($contentCont);
        $group.each(function () {
            $(this).data('helptips', $tooltip);
        });

        var $tipcontainer = $group.not('[data-helptip-text]');
        if (!$tipcontainer.length)
            $tipcontainer = $(document.body);
        
        $tooltip.appendTo($tipcontainer);
    }

    function showHelpForEles($ele, showOrHide) {
        var $tooltip = $ele.first().data('helptips');
        if (showOrHide) {
            $tooltip.show();
        }
        else {
            $tooltip.hide();
        }
    }

    function showHelpFor($eles, showOrHide) {

        $eles.not('[data-helptip-group]').each(function () {
            showHelpForEles($(this), showOrHide);
        });

        var done = {};
        $eles.filter('[data-helptip-group]').each(function () {
            var $this = $(this);
            var groupid = $this.attr('data-helptip-group');
            if (!done[groupid]) {
                showHelpForEles($eles.filter('[data-helptip-group=' + groupid + ']'), showOrHide);
                done[groupid] = true;
            }

        });
    };

//    function getPositionFor($eles) {
//        var pos = {top: 0, right:0};
//        $eles.each(function () {
//            var $this = $(this);
//            var posEle = $this.position();
//            if (posEle.left + $this.width() > pos.right)
//                pos.right = posEle.left + $this.width();
//            if (posEle.top + $this.height() > pos.top)
//                pos.top = posEle.top + $this.height();
//        });
//        return pos;
//    }
    
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
