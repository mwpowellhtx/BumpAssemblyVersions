@echo off

setlocal

:set_vars
set nuget_api_key=%my_nuget_api_key%

rem We do not publish the API key as part of the script itself.
if /i "%nuget_api_key%" equ "" (
    echo You are prohibited from making these sorts of changes.
    goto :fini
)

rem Default list delimiter is Comma (,).
:redefine_delim
if /i "%delim%" equ "" (
    set delim=,
)
rem Redefine the delimiter when a Dot (.) is discovered.
rem Anticipates potentially accepting Delimiter as a command line arg.
if /i "%delim%" equ "." (
    set delim=
    goto :redefine_delim
)

set destination_local=local
set destination_nuget=nuget
set debug_dry=dry
set debug_wet=wet

rem Go ahead and pre-seed the Projects up front.
:set_projects
set projects=
rem Setup All Projects
set all_projects=BumpAssemblyVersions
rem Setup Bump Projects
set bump_projects=BumpAssemblyVersions

:next_arg

:set_no_pause
if /i "%1" equ "--pause" (
    set should_pause=yes
    goto :fini_next_arg
)
if /i "%1" equ "--no-pause" (
    set should_pause=no
    goto :fini_next_arg
)

:set_drive_letter
if /i "%1" equ "--drive-letter" (
    set drive=%2
    shift
    goto :fini_next_arg
)
if /i "%1" equ "--drive" (
    set drive=%2
    shift
    goto :fini_next_arg
)

:set_destination
if /i "%1" equ "--%destination_nuget%" (
    set destination=%destination_nuget%
    goto :fini_next_arg
)
if /i "%1" equ "--%destination_local%" (
    set destination=%destination_local%
    goto :fini_next_arg
)

:set_dry_run
if /i "%1" equ "--%debug_dry%" (
    set dry=%debug_dry%
    goto :fini_next_arg
)
if /i "%1" equ "--%debug_dry%-run" (
    set dry=%debug_dry%
    goto :fini_next_arg
)
if /i "%1" equ "--%debug_wet%" (
    set dry=%debug_wet%
    goto :fini_next_arg
)
if /i "%1" equ "--%debug_wet%-run" (
    set dry=%debug_wet%
    goto :fini_next_arg
)

:set_config
if /i "%1" equ "--config" (
    set config=%2
    shift
    goto :fini_next_arg
)

:add_bump_projects
if /i "%1" equ "--bump" (
    if /i "%projects%" equ "" (
        set projects=%bump_projects%
    ) else (
        set projects=%projects%%delim%%bump_projects%
    )
    goto :fini_next_arg
)

:add_all_projects
if /i "%1" equ "--all" (
    set projects=%all_projects%
    goto :fini_next_arg
)

:add_project
if /i "%1" equ "--project" (
    if /i "%projects%" equ "" (
        set projects=%2
    ) else (
        set projects=%projects%%delim%%2
    )
    shift
    goto :fini_next_arg
)

:fini_next_arg

shift

if /i "%1" equ "" goto :fini_args

goto :next_arg

:fini_args

:verify_args

:verify_dry
rem Assumes we want a Live (Wet) Run when unspecified.
if /i "%dry%" equ "" set dry=%debug_wet%

:verify_destination
if /i "%destination%" equ "" set destination=local

:verify_config
rem Assumes Release Configuration when not otherwise specified.
if /i "%config%" equ "" set config=Release

:verify_drive
set publish_local_drive=%drive%

:publish_projects

:set_vars

set xcopy_exe=xcopy.exe
set nuget_exe=NuGet.exe

set nupkg_ext=.nupkg
if "%publish_local_drive%" == "" set publish_local_drive=E:
set publish_local_dir=%publish_local_drive%\Dev\NuGet\local\packages

rem Expecting NuGet to be found in the System Path.
set nuget_api_src=https://api.nuget.org/v3/index.json
set nuget_push_verbosity=detailed

set nuget_push_opts=%nuget_push_opts% %nuget_api_key%
set nuget_push_opts=%nuget_push_opts% -Verbosity %nuget_push_verbosity%
set nuget_push_opts=%nuget_push_opts% -NonInteractive
set nuget_push_opts=%nuget_push_opts% -Source %nuget_api_src%

rem Do the main areas here.
pushd ..\..

if not "%projects%" equ "" (
    echo Processing '%config%' configuration for '%projects%' ...
)
:next_project
if not "%projects%" equ "" (
    for /f "tokens=1* delims=%delim%" %%p in ("%projects%") do (
        call :publish_pkg %%p
        set projects=%%q
        goto :next_project
    )
)

popd

goto :fini

:publish_pkg
for %%f in ("%1\bin\%config%\%1*%nupkg_ext%") do (

    if /i "%destination%-%dry%" equ "%destination_local%-%debug_dry%" (
        echo Debug: %xcopy_exe% /q /y "%%f" "%publish_local_dir%"
    )

    if /i "%destination%-%dry%" equ "%destination_local%-%debug_wet%" (
        if not exist "%publish_local_dir%" mkdir "%publish_local_dir%"
        echo Copying "%%f" package to local directory "%publish_local_dir%" ...
        %xcopy_exe% /q /y "%%f" "%publish_local_dir%"
    )

    if /i "%destination%-%dry%" equ "%destination_nuget%-%debug_dry%" (
        echo Debug: %nuget_exe% push "%%f"%nuget_push_opts%
    )

    if /i "%destination%-%dry%" equ "%destination_nuget%-%debug_wet%" (
        echo Running: %nuget_exe% push "%%f"%nuget_push_opts%
        %nuget_exe% push "%%f"%nuget_push_opts%
    )
)
exit /b

:fini

endlocal

if /i "%should_pause%" equ "yes" pause
