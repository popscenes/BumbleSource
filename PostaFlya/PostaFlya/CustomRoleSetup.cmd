if "%EMULATED%" == "true" goto skipappinit

Echo Installing Application Initialization
PKGMGR.EXE /quiet /iu:IIS-ApplicationInit


Echo Installing dependencies
pushd .\Dep

Echo Installing SqlServerTypes
call SqlServerTypesInstall.bat

popd

%windir%\system32\inetsrv\appcmd set config /section:urlCompression /doDynamicCompression:True
%windir%\system32\inetsrv\appcmd set config /section:urlCompression /doStaticCompression:True
%windir%\system32\inetsrv\appcmd set config -section:system.webServer/httpCompression /+"dynamicTypes.[mimeType='application/javascript',enabled='True']" 
%windir%\system32\inetsrv\appcmd set config -section:system.webServer/httpCompression /+"dynamicTypes.[mimeType='application/json',enabled='True']" 


Echo Enabling compression for 1.0
%windir%\system32\inetsrv\appcmd set config -section:httpCompression -noCompressionForHttp10:false
%windir%\system32\inetsrv\appcmd set config -section:httpCompression -noCompressionForProxies:false


:skipappinit

