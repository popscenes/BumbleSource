/**/
(function (window, undefined) {
    var bf = window.bf = window.bf || {};

    bf.ErrorUtils = function () {
        var self = this;

        self.HandleRequestError = function (formselector, jqXhr, errorhandler) {

            var errormsg = $.parseJSON(jqXhr.responseText);

            var ret;
            if (errormsg == null || errormsg.IsError == undefined) {
                ret = { "Message": "Error occurred" };
            }
            else {
                ret = errormsg;
                if (errormsg.Message == "Validation Error") {
                    var $form = $(formselector);
                    var $formvalidator = $form.validate();
                    ret = {};
                    var customerr = [];
                    for (var i = 0; errormsg.Details && i < errormsg.Details.length; i++) {
                        var dets = errormsg.Details[i];
                        var prop = self.FindElementName($formvalidator, dets.Property);
                        if (prop != null)
                            ret[prop] = dets.Message;
                        else {
                            customerr.push({
                                message: dets.Message,
                                element: $form[0]
                            });
                        }
                        
                    }
             
                    //hack the crap out of it. show errors inits errorlist, so add any unknowns after error list
                    //inited, then trigger invalid to show summary, unknowns will show in summary
                    $formvalidator.showErrors(ret);
                    for (var i = 0; i < customerr.length; i++)
                        $formvalidator.errorList.push(customerr[i]);
                    $form.triggerHandler("invalid-form", [$formvalidator]);
                    return ret;
                }
                if (errormsg.Message == "Invalid Access") {
                    if (errorhandler && $.isFunction(errorhandler))
                        errorhandler(ret);
                    self.NeedsLogin();
                    return ret;
                }
            }

            if (errorhandler && $.isFunction(errorhandler))
                errorhandler(ret);
            return ret;
        };

        self.NeedsLogin = function() {
            window.location = "/Account/LoginPage?ReturnUrl=" + encodeURIComponent(window.location.pathname);
        };

        self.FindElementName = function ($formvalidator, property) {

            var found = ($formvalidator.findByName(property).length > 0);
            while (!found) {

                var index = property.indexOf(".");
                if (index <= 0 || index + 1 > property.length)
                    return null;
                property = property.substring(index + 1);
                found = ($formvalidator.findByName(property).length > 0);
            }
            return property.length > 0 ? property : null;
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