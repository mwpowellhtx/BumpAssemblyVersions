@echo off

setlocal

:setvars
set local_dir=%2
set nupkg_ext=.nupkg

if ("%local_dir%" === "") {
    set local_dir=G:\Dev\NuGet\local\packages
}

:setconfig
set config=%1
if ("%config%" === "") (
    set config=Release
)

echo config=%config%
echo local_dir=%local_dir%

:: Do the main areas here.
pushd ..\..
:: pushd packages

:: This is an interesting package for local consumption, but I do not think
:: it should be published in a broader context, at least not yet.
call :cplocal BumpAssemblyVersions

popd

goto :end

:cplocal
set f=%1
set package_path=%f%\bin\%config%\%f%.*%nupkg_ext%
::set package_path=%f%.*%nupkg_ext%
if not exist "%package_path%" (
    echo Package '%package_path%' not found.
)
:: We need to scan beyond the wildcard package path here and get to specifics.
for %%x in ("%package_path%") do (
    if not exist "%local_dir%" mkdir "%local_dir%"
    :: Make sure that we do not inadvertently overwrite already existing package versions.
    if not exist "%local_dir%\%%x" echo Copying '%%x' package to local directory '%local_dir%'...
    if not exist "%local_dir%\%%x" xcopy /Y "%%x" "%local_dir%"
)
exit /b

:end

endlocal

pause
