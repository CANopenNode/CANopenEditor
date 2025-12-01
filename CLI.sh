#!/bin/sh

dotnet run --project EDSSharp --framework net8.0 --property WarningLevel=0 "$@"
