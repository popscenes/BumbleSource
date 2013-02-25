(function ($) {

    var defaults = {
        close: "javascript:$(window.document.body).helptips('showHelp', false);",
        closeBtnClassAdd: '',
        nextBtnClassAdd: '',
        pageMap:{} //map of page to arrays of groups
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
        
        var pageMap = $(document.body).data('helptipsPages');
        pageMap = $.extend({}, pageMap, data.pageMap);
        $(document.body).data('helptipsPages', pageMap);
        
        if (!$eles.is('[data-helptip-text]'))
            $eles = $eles.find('[data-helptip-group], [data-helptip-group-content]');

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
    
    function getGroupFromConatiner($tooltip) {
        var id = $tooltip.attr("id");
        if (!id)
            return null;
        var indx = id.lastIndexOf('-helptip');
        if (indx >= 0)
            id = id.substring(0, indx);
        return id;
    }
    
    function getConatinerIdForGroup(groupId) {
        return groupId + '-helptip';
    }
    
    function initGroup($groupEle, $allGroup, data) {

        var $tooltip = $groupEle.data('helptips');
        if ($tooltip)
            return;

        var groupId = $groupEle.attr('data-helptip-group');
        if (!groupId)
            return;
            
        var $contentSrc = $allGroup.find('[data-helptip-group-content=' + groupId + ']');

        $tooltip = $('<div>')
            .addClass("helptips-container")
            .attr("id", getConatinerIdForGroup(groupId));
        
        var $arrowCont = $("<div>")
            .addClass("helptips-arrow")
            .appendTo($tooltip);

        var $contentCont = ($contentSrc.length ? $contentSrc.clone() : $("<div>"))
            .addClass("helptips-content")
            .removeAttr("data-helptip-group-content")
            .show()
            .appendTo($tooltip);

        var $footer = $("<div>")
            .addClass("helptips-footer");
        
        var $header = $("<div>")
            .addClass("helptips-header");
        
        var $close = $("<a>")
            .addClass('helptips-close')
            .addClass(data.closeBtnClassAdd)
            .text('Close');
               
        var $next = $("<a>")
            .addClass('helptips-next')
            .addClass(data.nextBtnClassAdd)
            .text('Next Tip >>');

        
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

    function showHelpForEles($ele, pageId, showOrHide, data) {
        var $tooltip = $ele.first().data('helptips');
        if (!$tooltip)
            return;
        
        var pageMap = $(document.body).data('helptipsPages');
        var groupsForPage = pageMap[pageId];
        
        var grp = getGroupFromConatiner($tooltip);
        if (showOrHide && $.inArray(grp, groupsForPage) >= 0) {
            $tooltip.addClass('helptips-visible');
            $tooltip.find('a.helptips-next').bind('click', function () {
                $(document.body).helptips('focusNext', pageId);
                return false;
            });

            if ($.isFunction(data.close)) {
                $tooltip.find('a.helptips-close').bind('click', data.close);
            } else {
                $tooltip.find('a.helptips-close').attr('href', data.close);
            }
        }
        else {
            $tooltip.find('a.helptips-close').removeAttr('href');
            $tooltip.find('a.helptips-close').unbind('click');
            $tooltip.find('a.helptips-next').unbind('click');
            $tooltip.removeClass('helptips-visible');
            $tooltip.removeClass('focused');
        }
    }

    function showHelpFor($eles, pageId, showOrHide, data) {

        data = $.extend({}, defaults, data);
        var done = {};
        $eles.filter('[data-helptip-group]').each(function () {
            var $this = $(this);
            var groupid = $this.attr('data-helptip-group');
            if (!done[groupid]) {
                showHelpForEles($eles.filter('[data-helptip-group=' + groupid + ']'), pageId, showOrHide, data);
                done[groupid] = true;
            }

        });
    };
    
    function focusNextImplementation(pageId) {

        var $body = $(document.body);
        var pageMap = $body.data('helptipsPages');
        var groupsForPage = pageMap[pageId];
        
        var $current = $body.find('.helptips-container.focused');
        if ($current.length) {
            $current.removeClass('focused');
        }

        var grp = getGroupFromConatiner($current);
        var startIndx = $.inArray(grp, groupsForPage);
        var $next = null;
        var cnt = 0;
        do { //may not be part of the dom at some stage so just find next one that is
            startIndx = (startIndx + 1) % groupsForPage.length;
            $next = $body.find('#' + getConatinerIdForGroup(groupsForPage[startIndx]));
            $next.addClass('focused');
        } while (!$next.length && (++cnt < groupsForPage.length));
        
        if ($next && $next.length)
            $next[0].scrollIntoView(false);

    }

    var publicmethods = {

//        init: function (dataDefaults) {
//            init(this, dataDefaults);
//        },

        showHelp: function (showOrHide, pageId, dataDefaults) {
            var $eles = init(this, dataDefaults);

            showHelpFor($eles, pageId, showOrHide, dataDefaults);
            if(showOrHide)
                focusNextImplementation(pageId);
            return this;         
        },
        
        anyShowing: function (pageId) {
            return this.find('.helptips-container')
                .filter('[data-helptip-page="' + pageId + '"]')
                .filter(':visible').length > 0;
        },
        
        focusNext: function (pageId) {
            focusNextImplementation(pageId);
            return this;
        }

    };

    $.fn.helptips = function (method) {

        // Method calling logic
        if (publicmethods[method]) {
            return publicmethods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }
        //not using init
//        else if (typeof method === 'object' || !method) {
//            return publicmethods.init.apply(this, arguments);
//        }
        else {
            $.error('Method ' + method + ' does not exist on jQuery.helptips');
        }
        return this;
    };

})(jQuery);
