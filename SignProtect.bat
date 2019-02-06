IF %3==Release call :Protect %1
ECHO Signing...
"%~1..\Tools\signtool.exe" sign /a /tr "http://sha256timestamp.ws.symantec.com/sha256/timestamp" "%~1\..\bin\%2\%3\IronstoneCore.dll"
exit /b

:Protect
ECHO Protecting...
cd /D "%~1"
"%~1..\Tools\DinkeyAdd\DinkeyAddCmd.exe" "%~1..\Protection.dapfj" /a2 /f0 || exit /b
