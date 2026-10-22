@echo off
rem Build TheOtherUs.dll with one command
rem Usage: build.bat [Release|Debug|Beta] (default: Release)
setlocal

set CONFIG=%1
if "%CONFIG%"=="" set CONFIG=Release

set ROOT=%~dp0
set PROJ=%ROOT%TheOtherRoles\TheOtherRoles.csproj
set OUT=%ROOT%dist
set GAME_PLUGINS=D:\games\au\Temporary\aubody\BepInEx\plugins

echo ================================
echo  Build config: %CONFIG%
echo ================================

dotnet build "%PROJ%" -c %CONFIG% --nologo
if errorlevel 1 (
    echo.
    echo [FAILED] Build error, check messages above.
    exit /b 1
)

if not exist "%OUT%" mkdir "%OUT%"
copy /y "%ROOT%TheOtherRoles\bin\%CONFIG%\net6.0\TheOtherUs.dll" "%OUT%\" >nul
if errorlevel 1 (
    echo [FAILED] Cannot copy DLL to dist folder.
    exit /b 1
)

if exist "%GAME_PLUGINS%\" (
    copy /y "%OUT%\TheOtherUs.dll" "%GAME_PLUGINS%\" >nul
    if errorlevel 1 (
        echo [FAILED] Cannot copy DLL to game plugins folder.
        exit /b 1
    )
    echo [OK] Copied to game: %GAME_PLUGINS%
) else (
    echo [WARN] Game plugins folder not found, skipped auto-copy.
)

echo.
echo [OK] Output: dist\TheOtherUs.dll
endlocal
