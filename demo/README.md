# CANopen-Demo-Netzwerk

Beispielnetzwerk (Classic CAN, 500 kbit/s wie CANbossTouch) für den
EDSSharp-CLI (`--export-project`): drei Geräteknoten
mit Datenpunkten und ein überwachender **CANboss-Master**. Aus jeder EDS
entstehen per CLI die CANopenNode-V4-Quellen (`OD.c`/`OD.h`) und ein
Proto-JSON-Export (CouchDB-/CANboss-Format) – die Geräteknoten lassen sich
damit auf beliebiger Hardware mit CANopenNode v4 ausführen, der Master läuft
auf dem CANbossTouch-Panel bzw. dient dem Terminal-CANboss als
Objektverzeichnis.

## Knoten

| Node-ID | Gerät | EDS | Rolle |
|--------:|-------|-----|-------|
| 16 | **Demo-IO** | `eds/demo_io.eds` | 4 digitale Ausgänge, 4 analoge Eingänge (±1000), Temperatur, Sollwert |
| 32 | **Demo-Antrieb** | `eds/demo_drive.eds` | CiA402-Teilmenge (vl velocity mode): Control-/Statusword, Drehzahlen, Motorstrom, Endstufentemperatur |
| 48 | **Demo-Sensor** | `eds/demo_sensor.eds` | Klimasensor: Temperatur, rel. Feuchte, Luftdruck, Messzähler, Alarmstatus, Grenzwerte |
| 127 | **CANboss-Master** | `eds/canboss_master.eds` | Monitor/Quasi-Master: empfängt alle TPDOs, kommandiert IO + Antrieb, überwacht Heartbeats, SDO-Client |

Alle Slaves senden Heartbeat (0x1017 = 1000 ms) und haben einen SDO-Server;
der Master konsumiert die Heartbeats (0x1016, Timeout 3000 ms) und
parametriert die Knoten über seinen SDO-Client (0x1280).

## PDO-Plan (COB-IDs)

| COB-ID | Quelle → Senke | Inhalt | Master-Spiegelobjekt |
|-------:|----------------|--------|----------------------|
| 0x190 | IO TPDO1 → Master RPDO1 | Analoge Eingänge 1–4 (4×INT16) | 0x2110 |
| 0x290 | IO TPDO2 → Master RPDO2 | Temperatur (REAL32), digitale Eingänge D1–D4 (4×BOOL) | 0x2111, 0x2112 |
| 0x1A0 | Antrieb TPDO1 → Master RPDO3 | Statusword, Drehzahl Rampe/Ist | 0x2120 |
| 0x2A0 | Antrieb TPDO2 → Master RPDO4 | Motorstrom mA, Temperatur Endstufe | 0x2121 |
| 0x1B0 | Sensor TPDO1 → Master RPDO5 | Temperatur, rel. Feuchte | 0x2130 |
| 0x2B0 | Sensor TPDO2 → Master RPDO6 | Luftdruck, Messzähler | 0x2131 |
| 0x3B0 | Sensor TPDO3 → Master RPDO7 | Alarmstatus (Bit0 Temp, Bit1 Feuchte), Alarm aktiv (BOOL) | 0x2132 |
| 0x210 | Master TPDO1 → IO RPDO1 | Ausgänge 1–4 (4×BOOL), Sollwert (INT16) | 0x2210 (Kommando) |
| 0x220 | Master TPDO2 → Antrieb RPDO1 | Controlword, Drehzahl-Sollwert, Bremse aktiv (BOOL) | 0x2220 (Kommando) |

BOOL-Datenpunkte kommen dabei in beiden Welten vor: als PDO-Nutzdaten
(CANopenNode mappt BOOLEAN byteweise, 8 Bit pro Eintrag – IO-Ausgänge im
RPDO, IO-Eingänge/Alarm im TPDO, Bremse im Antriebs-RPDO) und als
SDO-Datenpunkte (alle BOOL-Objekte sind SDO-lesbar/-schreibbar; die
Ausgangs-Bitmaske 0x2002 im Demo-IO ist bewusst ein reiner SDO-Datenpunkt
ohne PDO-Mapping).

Die Slave-TPDOs sind ereignisgesteuert (Transmission Type 255) mit
Event-Timer (IO/Antrieb: Status 200 ms, Messwerte 1000 ms; Sensor 1000 ms,
Alarm bei Änderung) und Inhibit-Zeit. Der Master sieht damit ohne einen
einzigen SDO-Transfer sämtliche Prozessdaten in seinem eigenen
Objektverzeichnis (0x2110–0x2132) und schreibt Kommandos einfach in
0x2210/0x2220 (TPDO bei Änderung).

## Generieren

```sh
./generate.sh
```

erzeugt (per `EDSSharp --export-project`, baut den CLI bei Bedarf):

```
generated/<knoten>/<knoten>.c/.h    CANopenNode-V4-Objektverzeichnis
generated/json/<Gerätetyp>.json     Proto-JSON (CouchDB-Dokumentinhalt)
```

Einzelaufruf, z. B. für den Master:

```sh
dotnet EDSSharp.dll --export-project --infile eds/canboss_master.eds \
    --outdir out --od OD --json out/CANboss-Master.json
```

Mit `--od OD` heißen die Quellen `OD.c`/`OD.h` und die Makros `OD_CNT_*` –
so wird das Master-OD im CANbossTouch-Repo (`App/OD/`) erzeugt.

## EDS-Erweiterung `;CO_countLabel=`

CANopenNode v4 braucht pro Objekt ein „Count Label" (NMT, EM, HB_PROD,
SDO_SRV, RPDO, TPDO, …), aus dem die `OD_CNT_*`-Zähler in `OD.h` entstehen.
Bisher konnte nur das XDD/XPD-Format diese Information transportieren; die
Demo-EDS-Dateien nutzen die neue EDS-Kommentar-Erweiterung analog zu
`;StorageLocation=`:

```ini
[1017]
ParameterName=Producer heartbeat time
ObjectType=0x7
;StorageLocation=PERSIST_COMM
;CO_countLabel=HB_PROD
DataType=0x0006
AccessType=rw
DefaultValue=1000
PDOMapping=0
```

Ohne Count Labels wäre das aus einer EDS exportierte OD für CANopenNode
nicht initialisierbar (alle `OD_CNT_*` fehlen).

## Verwendung

- **CANbossTouch** (`protronic/CANbossTouch`): `eds/` enthält Kopien der
  Demo-EDS-Dateien; `App/OD/OD.c/.h` ist das aus `canboss_master.eds`
  generierte Master-OD des Panels (Node 127).
- **CANboss** (`protronic/CANboss`, Terminal): lädt die Proto-JSONs aus
  `generated/json/` direkt (`--proto-file`/`--eds-dir`) oder aus CouchDB;
  `examples/demo-network.sample.json` beschreibt das Setup.
