@echo off
REM Build and publish ExtractLocres as a single exe, then copy to current folder
setlocal
set PROJECT=ExtractLocres.csproj
set OUTDIR=publish
set EXENAME=ExtractLocres.exe

REM Publish as single file, self-contained

dotnet publish %PROJECT% -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -o %OUTDIR%

REM Copy the exe to current folder
copy /Y %OUTDIR%\%EXENAME% .

@echo Done. The executable is in the current folder: %EXENAME%
endlocal
