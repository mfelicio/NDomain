xcopy ..\..\source\NDomain.Azure\bin\Release\NDomain.Azure.dll lib\net45\ /y

NuGet.exe pack NDomain.Azure.nuspec -exclude *.cmd -OutputDirectory ..\