if "%EMULATED%" == "true" goto skipappinit

Echo Installing Application Initialization
PKGMGR.EXE /quiet /iu:IIS-ApplicationInit

:skipappinit