@ECHO OFF

setlocal enabledelayedexpansion

set fullname=%~1
for %%F in ("%fullname%") do (
    set "filename=%%~nF"
)
echo "%filename%.svg"
echo "%fullname%"
d2 --animate-interval=800 "%fullname%" "%filename%.svg"
pause
