#!/bin/sh

COLOR='\033[38;5;21m'
BG='\033[47m'
WHITE='\033[0;32m'

echo ${BG}${COLOR}"LIB and CLI tests:"${WHITE}
dotnet run --project Tests --framework net8.0 --property WarningLevel=0
echo ${BG}${COLOR}"GUI tests:"${WHITE}
dotnet run --project GUITests --framework net8.0 --property WarningLevel=0
