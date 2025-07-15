@echo off
echo Compiling UDP test sender...
csc test_udp_sender.cs
if %errorlevel% neq 0 (
    echo Compilation failed!
    pause
    exit /b 1
)

echo.
echo Starting UDP test sender...
echo This will send test packets to localhost:8765
echo Make sure the desktop application is running first!
echo.
echo Press Ctrl+C to stop
echo.

test_udp_sender.exe 