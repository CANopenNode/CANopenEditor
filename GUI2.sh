#!/bin/sh

dotnet run --project EDSEditorGUI2 --framework net8.0 --property WarningLevel=0 "$@"
