ECHO Signing...
"%~1..\Tools\signtool.exe" sign /a /tr "http://sha256timestamp.ws.symantec.com/sha256/timestamp" "%~1\..\bin\%2\%3\IronstoneCoreUI.dll"
exit /b