# Release und Deployment

Dieses Dokument beschreibt, wie Release-Pakete für die Verteilung von EDSEditor erstellt werden.

## Automatische ZIP-Erstellung

Bei jedem Release-Build wird automatisch ein ZIP-Archiv erstellt:

```powershell
# Build für alle Targets (erstellt automatisch ZIPs)
dotnet build EDSEditorGUI/EDSEditorGUI.csproj -c Release

# Build nur für .NET Framework 4.8.1
dotnet build EDSEditorGUI/EDSEditorGUI.csproj -c Release -p:BuildNet481=true

# Build nur für .NET 8.0
dotnet build EDSEditorGUI/EDSEditorGUI.csproj -c Release -p:BuildNet8=true
```

Die ZIP-Dateien werden automatisch in den entsprechenden Ausgabeverzeichnissen erstellt:
- `EDSEditorGUI/bin/Release/net481/net481.zip`
- `EDSEditorGUI/bin/Release/net8.0-windows/net8.0-windows.zip`

## Manuelle Paket-Erstellung mit Versionierung

Für versionierte Release-Pakete kann das PowerShell-Script verwendet werden:

```powershell
# Erstellt versionierte Pakete in ./releases/
.\create_release_packages.ps1

# Mit benutzerdefiniertem Ausgabeverzeichnis
.\create_release_packages.ps1 -OutputDir "C:\Releases"

# Für Debug-Builds (normalerweise nicht nötig)
.\create_release_packages.ps1 -Configuration Debug
```

Das Script erstellt Pakete mit der Version aus `version.txt`:
- `releases/EDSEditor-v{version}-net481.zip`
- `releases/EDSEditor-v{version}-net8.0-windows.zip`

## Ausgabeverzeichnisse

Nach einem erfolgreichen Release-Build enthalten die Ausgabeverzeichnisse:

### .NET Framework 4.8.1
- Verzeichnis: `EDSEditorGUI/bin/Release/net481/`
- Ausführbare Datei: `EDSEditor.exe`
- Alle erforderlichen DLLs und Abhängigkeiten
- Profile im Unterverzeichnis `Profiles/`
- ZIP-Archiv: `net481.zip`

### .NET 8.0
- Verzeichnis: `EDSEditorGUI/bin/Release/net8.0-windows/`
- Ausführbare Datei: `EDSEditor.exe`
- Runtime-Dateien und Abhängigkeiten
- Lokalisierungsressourcen in Sprachordnern (de/, en/, etc.)
- ZIP-Archiv: `net8.0-windows.zip`

## Verteilung

Die ZIP-Archive enthalten alles, was für die Ausführung benötigt wird:

### Voraussetzungen für Endnutzer:
- **net481.zip**: Benötigt .NET Framework 4.8.1 (normalerweise bereits auf Windows 10/11 installiert)
- **net8.0-windows.zip**: Benötigt .NET 8.0 Desktop Runtime

### Installation:
1. ZIP-Archiv entpacken
2. `EDSEditor.exe` ausführen
3. Keine zusätzliche Installation erforderlich

## CI/CD Integration

Für automatisierte Builds kann das Script in CI/CD-Pipelines integriert werden:

```powershell
# Build
dotnet build EDSEditorGUI/EDSEditorGUI.csproj -c Release

# Pakete erstellen
.\create_release_packages.ps1 -OutputDir "artifacts"
```

Die erstellten Pakete im `artifacts/`-Verzeichnis können dann als Build-Artefakte veröffentlicht werden.

## Inhalt der Pakete

Die ZIP-Archive enthalten:
- Haupt-Executable (EDSEditor.exe)
- Alle benötigten DLL-Abhängigkeiten
- Konfigurationsdateien
- Lokalisierungsressourcen
- Profile-Vorlagen
- Dokumentation (version.txt, style.css)

## Fehlerbehebung

**Problem**: ZIP wird nicht erstellt
- **Lösung**: Stellen Sie sicher, dass der Build erfolgreich war
- Prüfen Sie, ob das Ausgabeverzeichnis existiert

**Problem**: "Zugriff verweigert" beim Erstellen des ZIP
- **Lösung**: Schließen Sie alle Programme, die Dateien im Ausgabeverzeichnis verwenden
- Löschen Sie alte ZIP-Dateien manuell, falls nötig

**Problem**: Version wird nicht korrekt angezeigt
- **Lösung**: Stellen Sie sicher, dass `version.txt` existiert und korrekt befüllt ist
- Der Pre-Build-Event versucht, die Version aus Git zu lesen
