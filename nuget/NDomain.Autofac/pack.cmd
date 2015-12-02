xcopy ..\..\source\NDomain.Autofac\bin\Release\NDomain.Autofac.dll lib\net45\ /y

NuGet.exe pack NDomain.Autofac.nuspec -exclude *.cmd -OutputDirectory ..\