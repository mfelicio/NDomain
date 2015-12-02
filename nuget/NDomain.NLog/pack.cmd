xcopy ..\..\source\NDomain.NLog\bin\Release\NDomain.NLog.dll lib\net45\ /y

NuGet.exe pack NDomain.NLog.nuspec -exclude *.cmd -OutputDirectory ..\