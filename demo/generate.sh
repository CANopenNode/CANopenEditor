#!/usr/bin/env bash
# Erzeugt aus den Demo-Netzwerk-EDS-Dateien die CANopenNode-V4-ODs (.c/.h)
# und die Proto-JSON-Exporte (CouchDB-/CANboss-Format) per EDSSharp-CLI.
#
# Voraussetzung: dotnet 8 SDK (baut den CLI bei Bedarf selbst).
#
# Aufruf:  ./generate.sh
set -euo pipefail
cd "$(dirname "$0")"

EDSSHARP_DLL="../EDSSharp/bin/Release/net8.0/EDSSharp.dll"

if [ ! -f "$EDSSHARP_DLL" ]; then
    dotnet build ../EDSSharp/EDSSharp.csproj -c Release -p:BuildNet8=true
fi

run() { dotnet "$EDSSHARP_DLL" "$@"; }

# Je Knoten: OD-C-Quellen nach generated/<name>/ und Proto-JSON nach
# generated/json/<ProductName>.json (Dateiname = ProductName = Geraetetyp,
# den CANboss als CouchDB-Dokument-ID bzw. --eds-dir-Dateiname erwartet).
run --export-project --infile eds/demo_io.eds        --outdir generated/demo_io        --od demo_io        --json generated/json/Demo-IO.json
run --export-project --infile eds/demo_drive.eds     --outdir generated/demo_drive     --od demo_drive     --json generated/json/Demo-Antrieb.json
run --export-project --infile eds/demo_sensor.eds    --outdir generated/demo_sensor    --od demo_sensor    --json generated/json/Demo-Sensor.json
run --export-project --infile eds/canboss_master.eds --outdir generated/canboss_master --od canboss_master --json generated/json/CANboss-Master.json

echo "Fertig: generated/<knoten>/*.c/.h + generated/json/*.json"
