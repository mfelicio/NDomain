xcopy ..\..\source\NDomain.Redis\bin\Release\NDomain.Redis.dll lib\net45\ /y

NuGet.exe pack NDomain.Redis.nuspec -exclude *.cmd -OutputDirectory ..\