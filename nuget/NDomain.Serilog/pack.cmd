xcopy ..\..\source\NDomain.Serilog\bin\Release\NDomain.Serilog.dll lib\net45\ /y

NuGet.exe pack NDomain.Serilog.nuspec -exclude *.cmd -OutputDirectory ..\