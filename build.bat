@echo off
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
"%CSC%" /nologo /target:winexe /out:"%~dp0build\MwStRechner.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll "%~dp0src\Program.cs" "%~dp0src\AssemblyInfo.cs"
if %ERRORLEVEL% neq 0 (
    echo Fehler beim Kompilieren.
    pause
)
