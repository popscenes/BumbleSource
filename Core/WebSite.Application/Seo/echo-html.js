var system = require('system');
var url = system.args[1] || '';
var waitforbodystatus = system.args[2];
if (url.length > 0) {
    
    var page = require('webpage').create();
    if (waitforbodystatus) {
        
        page.open(url, function (status) {
            if (status == 'success') {
                var delay, checker = (function () {

                    var html = page.evaluate(function () {
                            var body = document.getElementsByTagName('body')[0];
                            if (body.getAttribute('data-status') == 'ready') {
                                return document.getElementsByTagName('html')[0].outerHTML;
                            }
                        });

                    if (html) {
                        clearTimeout(delay);
                        console.log(html);
                        phantom.exit();
                    }
                });
                delay = setInterval(checker, 100);
            }

        });
    }
    else
    {
        page.open(url, function (status) {
            if (status == 'success') {
                var html = page.evaluate(function () {
                    return document.getElementsByTagName('html')[0].outerHTML;
                });

                if (html) {
                    console.log(html);
                }
                phantom.exit();
            }

        });
    }


} else {
    phantom.exit();
}
