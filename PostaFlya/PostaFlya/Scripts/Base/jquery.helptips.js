(function ($) {

    var defaults = {
        closeHref: "javascript:$(window.document.body).helptips('showHelp', false);",
        closeBtnClassAdd: '',
        nextBtnClassAdd: '',
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

    function init($eles, dataDefaults) {

        var data = $.extend({}, defaults, dataDefaults);
        
        if (!$eles.is('[data-helptip-text]'))
            $eles = $eles.find('[data-helptip-group], [data-helptip-group-content]');

        $eles.not('[data-helptip-group], [data-helptip-group-content]').each(function () {
            initElement($(this), data);
        });

        var $groupEles = $eles.filter('[data-helptip-group], [data-helptip-group-content]');
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
        if (!groupId)
            return;
        
        var $contentSrc = $allGroup.find('[data-helptip-group-content=' + groupId + ']');
        $contentSrc.show();


        $tooltip = $('<div>')
            .addClass("helptips-container")
            .attr("id", groupId + '-helptip');
        
        var $arrowCont = $("<div>")
            .addClass("helptips-arrow")
            .appendTo($tooltip);

        var $contentCont = ($contentSrc.length ? $contentSrc.clone() : $("<div>"))
            .addClass("helptips-content")
            .removeAttr("data-helptip-group-content")
            .appendTo($tooltip);

        var $footer = $("<div>")
            .addClass("helptips-footer");
        
        var $header = $("<div>")
            .addClass("helptips-header");
        
        var $close = $("<a>")
            .addClass('helptips-close')
            .addClass(data.closeBtnClassAdd)
            .attr('href', data.closeHref)
            .text('Close');
        
        var $next = $("<a>")
            .addClass('helptips-next')
            .addClass(data.nextBtnClassAdd)
            .text('Next Tip >')
            .bind('click', function () {
                $(document.body).helptips('focusNext');
                return false;
            });
        
        $footer.append($next);
        $footer.append($close);
        
        $header.append($next.clone(true));
        $header.append($close.clone(true));

            
        var $group = $allGroup.filter('[data-helptip-group=' + groupId + ']');
        if (!$contentSrc.length) {
            var $content = getContent($group.filter('[data-helptip-text]'));
            $content.appendTo($contentCont);
        }

        $header.prependTo($contentCont);
        $footer.appendTo($contentCont);
        
        $group.each(function () {
            $(this).data('helptips', $tooltip);
        });

        var $tipcontainer = $group.not('[data-helptip-text]');
        if ($tipcontainer.length) {
            $tooltip.appendTo($tipcontainer);
        }
        else if ($contentSrc.length) {
            $tooltip.insertAfter($contentSrc);
            $contentSrc.remove();
        }
        else{
            $tooltip.appendTo($(document.body));          
        }    
    }

    function showHelpForEles($ele, showOrHide) {
        var $tooltip = $ele.first().data('helptips');
        if (!$tooltip)
            return;
        
        if (showOrHide) {
            $tooltip.addClass('helptips-visible');
        }
        else {
            $tooltip.removeClass('helptips-visible');
            $tooltip.removeClass('focused');
        }
    }

    function showHelpFor($eles, showOrHide) {

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
    
    function focusNextImplementation() {
        var skip = true;
      
        var $current = null;
        var $next = null;
        var $tips = $(document.body).find('.helptips-container');
        for (var i = 0; i < 2; i++) {
            if ($next != null)
                break;
            $tips.each(function () {
                var $this = $(this);
                if ($this.hasClass('focused')) {
                    skip = false;
                    $current = $this;
                    return;
                }                
                if (skip) return;
                if ($next == null)
                    $next = $this;
            });
        }
        
        if ($next == null)
            $next = $tips.first();
        
        if ($current) {
            $current.removeClass('focused');
        }
        
        if ($next) {
            $next.addClass('focused');
        }
        
    }

    var publicmethods = {

        init: function (dataDefaults) {
            init(this, dataDefaults);
        },

        showHelp: function (showOrHide, dataDefaults) {
            var $eles = init(this, dataDefaults);

            showHelpFor($eles, showOrHide);
            if(showOrHide)
                focusNextImplementation();
            return this;         
        },
        
        anyShowing: function() {
            return this.find('.helptips-container').filter(':visible').length > 0;
        },
        
        focusNext: function() {
            focusNextImplementation();
            return this;
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
