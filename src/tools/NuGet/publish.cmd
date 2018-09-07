@echo off

setlocal

:: We do not publish the API key as part of the script itself.
if "%my_nuget_api_key%"=="" (
    echo You are prohibited from making these sorts of changes.
    goto :end
)

:setvars
:: Expecting NuGet to be found in the System Path.
set nuget_exe=NuGet.exe
set nuget_push_verbosity=detailed
set nuget_push_source=https://api.nuget.org/v3/index.json

set nuget_push_opts=%nuget_push_opts% %my_nuget_api_key%
set nuget_push_opts=%nuget_push_opts% -Verbosity %nuget_push_verbosity%
set nuget_push_opts=%nuget_push_opts% -NonInteractive
set nuget_push_opts=%nuget_push_opts% -Source %nuget_push_source%

:setconfig
set config=%1
if ("%config%" === "") (
    set config=Release
)

:: Do the main areas here.
::pushd .\packages
pushd ..\..

call :pubpkg BumpAssemblyVersions

popd

goto :end

:pubpkg
set f=%1
for %%f in ("%f%\bin\%config%\%f%.*.nupkg") do (
    %nuget_exe% push "%%f" %nuget_push_opts%
)
exit /b

:end

endlocal

pause
