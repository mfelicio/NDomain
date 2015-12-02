xcopy ..\..\source\NDomain\bin\Release\NDomain.dll lib\net45\ /y

NuGet.exe pack NDomain.nuspec -exclude *.cmd -OutputDirectory ..\