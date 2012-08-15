/**/
(function (window, undefined) {
    var bf = window.bf = window.bf || {};

    bf.ErrorUtils = function () {
        var self = this;

        self.HandleSubmitError = function (formselector, jqXhr, errorhandler) {

            var errormsg = $.parseJSON(jqXhr.responseText);

            var ret;
            if (errormsg == null || errormsg.IsError == undefined) {
                ret = { "Message": "Error occurred" };
            }
            else {
                ret = errormsg;
                if (errormsg.Message == "Validation Error") {
                    var $formvalidator = $(formselector).validate();
                    ret = {};
                    for (var i = 0; errormsg.Details && i < errormsg.Details.length; i++) {
                        var dets = errormsg.Details[i];
                        if ($formvalidator.findByName(dets.Property).length > 0)
                            ret[dets.Property] = dets.Message;
                    }

                    $formvalidator.showErrors(ret);
                    return;
                }
            }

            if (errorhandler && $.isFunction(errorhandler))
                errorhandler(ret);
        };

        self.HandleError = function (jqXhr, viewmodel) {

            var errormsg = $.parseJSON(jqXhr.responseText);

            var ret;
            if (!errormsg.IsError) {
                ret = { "Message": "Error occurred" };
            }
            else {
                ret = errormsg;
            }

            if (viewmodel && viewmodel.ErrorHandler)
                viewmodel.ErrorHandler(ret);
        };
    };

    bf.ErrorUtil = new bf.ErrorUtils();
    $(function () {
        bf.ErrorUtil = new bf.ErrorUtils();
    });


})(window);
/**/