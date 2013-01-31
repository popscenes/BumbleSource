if "%EMULATED%" == "true" goto skipappinit

Echo Installing Application Initialization
PKGMGR.EXE /quiet /iu:IIS-ApplicationInit


Echo Installing dependencies
pushd .\Dep

Echo Installing SqlServerTypes
call SqlServerTypesInstall.bat

popd


:skipappinit

