pushd
cd %~dp0..
NuGet Update -self
call "%VS140COMNTOOLS%vsvars32.bat"
if not .%1.==.. NuGet SetApiKey %1
msbuild vm.Aspects.csproj /t:Rebuild /p:Configuration=Release /m
if errorlevel 1 goto exit
NuGet Pack NuGet\vm.Aspects.nuspec -Prop Configuration=Release
if errorlevel 1 goto exit
NuGet Push *.nupkg
:exit
del *.nupkg
popd
pause