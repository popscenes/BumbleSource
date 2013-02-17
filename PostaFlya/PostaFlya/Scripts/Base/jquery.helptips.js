(function ($) {

    var defaults = {
        closeHref: "javascript:$(window.document.body).helptips('showHelp', false);"
    };
    
    function getHelpIdForElement($element) {
        var id = $element.attr('data-helptip-id');
        if (!id) {
            id = $element.attr('id');
            if (!id)
                id = $element.attr('name');
            if (id)
                id = id + '-helptip';
        }
        return id;
    }

    function initElement($element, data) {
        var $tooltip = $element.data('helptips');
        if ($tooltip)
            return;
        
        $tooltip = $('<div>')
            .addClass("helptips-container")
            .hide();

        var id = getHelpIdForElement($element);   
        if (id)
            $tooltip.attr('id', id);
        
        var $arrowCont = $("<div>")
            .addClass("helptips-arrow")
            .appendTo($tooltip);
        

        var $content = $("<div>")
            .addClass("helptips-content")
            .text($element.attr("data-helptip-text"))
            .appendTo($tooltip);
        
        var $footer = $("<div>")
            .addClass("helptips-footer");

        var $close = $("<a>")
            .addClass('helptips-close')
            .attr('href', data.closeHref)
            .text('Close');

        $footer.append($close);
        $footer.appendTo($content);

        $tooltip.insertAfter($element);
        $element.data('helptips', $tooltip);
    }

    function init($eles, dataDefaults) {

        var data = $.extend({}, defaults, dataDefaults);
        
        if (!$eles.is('[data-helptip-text]'))
            $eles = $eles.find('[data-helptip-text], [data-helptip-group]');

        $eles.not('[data-helptip-group]').each(function () {
            initElement($(this), data);
        });

        var $groupEles = $eles.filter('[data-helptip-group]');
        $groupEles.each(function () {
            initGroup($(this), $groupEles, data);
        });

        return $eles;
    }

    function getContent($group) {
        var $ret = $([]);
        $group.each(function () {
            var $this = $(this);
            
            var id = getHelpIdForElement($this);

            //override anything you want removed using helptips-group-icon class
            var $icondiv = $('<div>')          
                .addClass('helptips-group-icon');
            //attempt to locate an image to use otherwise just copy class from source element
            var $img = $this.is('img') ? $this : $this.find('img');
            if ($img.length > 0) {
                $icondiv.append($img.clone()
                    .removeAttr("id")
                    .removeAttr("data-helptip-text")
                    .removeAttr("data-helptip-group")
                );
            }
            else {
                $icondiv.addClass($this.attr("class"));
            }
          
            var $row = $('<div>')
                .append($icondiv)
                .append(
                    $('<div>')
                    .addClass('helptips-group-text')
                    .text($this.attr("data-helptip-text")))
                .addClass('helptips-group-row');

            if (id)
                $row.attr("id", id);
            
            $ret = $ret.add($row);
                
        });

        $ret.append(
            $('<div>').css("clear", "both"));

        return $ret;
    }

    function initGroup($groupEle, $allGroup, data) {

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

        var $footer = $("<div>")
            .addClass("helptips-footer");
        
        var $close = $("<a>")
            .addClass('helptips-close')
            .attr('href', data.closeHref)
            .text('Close');
        
        $footer.append($close);
        
        
        var $group = $allGroup.filter('[data-helptip-group=' + groupId + ']');
        var $content = getContent($group.filter('[data-helptip-text]'));
        $content.appendTo($contentCont);
        
        $footer.appendTo($contentCont);
        
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

        init: function (dataDefaults) {
            init(this, dataDefaults);
        },

        showHelp: function (showOrHide, dataDefaults) {
            var $eles = init(this, dataDefaults);

            showHelpFor($eles, showOrHide);

            return this;         
        },
        
        anyShowing: function() {
            return this.find('.helptips-container').filter(':visible').length > 0;
        }

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
