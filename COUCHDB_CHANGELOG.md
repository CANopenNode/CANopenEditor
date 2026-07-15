# CouchDB Export Feature - Changelog

## Übersicht
Implementierung einer vollständigen CouchDB-Export-Funktionalität für CANopenEditor mit HTTP-Upload-Unterstützung und Konfigurationsmenü.

---

## Phase 1: CouchDB Exporter Implementation

### Neue Datei: `libEDSsharp/CouchDBExporter.cs`
- **Klasse**: `CouchDBExporter : IFileExporter`
- **Funktionalität**:
  - `ExportSingleDocument()` - Exportiert einzelnes EDS-Gerät im CanOpenNode Protobuf JSON Format
  - `ExportMultipleDocuments()` - Exportiert mehrere Geräte als separate JSON-Dateien
  - `UploadToCouchDB()` - Asynchroner HTTP PUT Upload zu CouchDB Server
- **Format**: Nutzt `WriteProtobuf` aus `CanOpenXDD_1_1` mit JSON-Flag
- **CouchDB Dokument-Struktur**:
  ```json
  {
    "_id": "productName",
    "proto": { /* CanOpenNode Protobuf JSON */ }
  }
  ```
- **Abhängigkeiten**: 
  - `Newtonsoft.Json` (13.0.3) für JSON-Wrapping
  - `System.Net.Http` für HTTP-Requests

### GetExporters() - Registrierte Exporter:
1. "CouchDB JSON (CanOpenNode Protobuf)" - Einzeldokument-Export
2. "CouchDB JSON (Multiple Devices)" - Multi-Geräte-Export

---

## Phase 2: GUI Integration

### Form1.cs - Event Handler
**Neue Methode**: `exportCouchDBToolStripMenuItem_Click()`
- Asynchrone Implementation (`async void`)
- Dual-Mode Dialog:
  - **Yes**: Upload zu CouchDB (via HTTP PUT)
  - **No**: Speichern als lokale JSON-Datei
  - **Cancel**: Abbruch
- **Upload-Verhalten**:
  - Liest CouchDB-URL aus Preferences
  - Validiert URL-Konfiguration
  - Zeigt Erfolgs-/Fehlermeldung mit Response
- **Fehlerbehandlung**: Try-Catch mit aussagekräftigen Meldungen

### Form1.Designer.cs - Menu Item
- **Neues Control**: `exportCouchDBToolStripMenuItem`
- **Menu-Hierarchie**: File → Export to CouchDB...
- **Eigenschaften**:
  - Text: "Export to Couch&DB..."
  - Shortcut-Ready (Alt+D für Keyboard-Navigation)
  - Enabled-Status tied to active tab
  - Event-Handler verbunden

---

## Phase 3: Preferences Dialog

### Preferences.Designer.cs - UI-Komponenten
**Neue Controls**:
- `label_couchdb`: Label "CouchDB URL"
- `textBox_couchdburl`: TextBox für URL-Eingabe
- Position: Unter den Warning-Checkboxen
- Size: 478px Breite für lange URLs

### Preferences.cs - Logik
- **InitializeComponent()**: 
  - Lädt aktuelle CouchDB-URL aus Settings
  - Standard: `http://localhost:5984/canopen`
- **button_save_Click()**: 
  - Speichert neue URL in Properties.Settings
  - Persistierung via `Settings.Default.Save()`

---

## Phase 4: Settings & Configuration

### Settings.settings (EDSEditorGUI)
```xml
<Setting Name="CouchDBUrl" Type="System.String" Scope="User">
  <Value Profile="(Default)">http://localhost:5984/canopen</Value>
</Setting>
```

### Settings.Designer.cs - Property
```csharp
[UserScopedSetting]
[DefaultSettingValue("http://localhost:5984/canopen")]
public string CouchDBUrl {
    get { return ((string)(this["CouchDBUrl"])); }
    set { this["CouchDBUrl"] = value; }
}
```

### App.config
- User-Setting-Eintrag für CouchDBUrl
- Default-Wert: `http://localhost:5984/canopen`
- Scope: User (pro Benutzer konfigurierbar)

---

## Phase 5: Dependency Management

### libEDSsharp.csproj - Package References
```xml
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="System.Net.Http" Version="4.3.4" 
                  Condition="'$(TargetFramework)' == 'net481'" />
```
- **System.Net.Http**: Nur für .NET Framework 4.8.1 (HttpClient-Support)
- **Newtonsoft.Json**: Für JSON-Manipulation und CouchDB-Dokument-Wrapping

### Multi-Target Support
- ✅ .NET Framework 4.8.1
- ✅ .NET 8.0

---

## Technische Details

### HTTP PUT Upload
```csharp
// URL: {couchDbUrl}/{productName}
PUT http://localhost:5984/canopen/MyProduct
Content-Type: application/json

{
  "_id": "MyProduct",
  "proto": { /* Protobuf JSON */ }
}
```

### Workflow
1. Benutzer öffnet EDS-Datei
2. File → Export to CouchDB...
3. Dialog: Upload oder Speichern?
4. Bei Upload:
   - Generiert Protobuf JSON
   - Wraps in CouchDB-Dokument
   - Sendet HTTP PUT
   - Zeigt Response/Fehler
5. Bei Speichern:
   - Öffnet SaveFileDialog
   - Speichert Protobuf JSON lokal

### Fehlerbehandlung
- Missing CouchDB URL: Warnung mit Preferences-Hinweis
- HTTP Error: Status-Code + Response-Body anzeigen
- Network Error: Exception-Message anzeigen

---

## Build Status

### Erfolgreich kompiliert
- ✅ libEDSsharp (net481 + net8.0)
- ✅ EDSEditorGUI (net8.0-windows)
- ⚠️ 33 Warnungen (Naming Conventions - nicht kritisch)

### Fehlerfreier Build
- 0 Fehler
- Multi-Target: Beide Frameworks unterstützt

---

## Dateien geändert/erstellt

### Neue Dateien (1)
- `libEDSsharp/CouchDBExporter.cs` (137 Zeilen)

### Modifizierte Dateien (7)
1. `EDSEditorGUI/Form1.cs` - Event Handler + async Upload
2. `EDSEditorGUI/Form1.Designer.cs` - Menu Item
3. `EDSEditorGUI/Preferences.cs` - URL-Einstellung
4. `EDSEditorGUI/Preferences.Designer.cs` - TextBox + Label
5. `EDSEditorGUI/Properties/Settings.Designer.cs` - CouchDBUrl Property
6. `EDSEditorGUI/App.config` - Setting-Eintrag
7. `libEDSsharp/libEDSsharp.csproj` - Package References

---

## Features

### ✅ Implementiert
- [x] CouchDB JSON Export (CanOpenNode Protobuf Format)
- [x] Einzelgeräte-Export
- [x] Mehrgeräte-Export
- [x] HTTP PUT Upload zu CouchDB
- [x] Konfigurierbare CouchDB-URL
- [x] Preferences Dialog Integration
- [x] Fehlerbehandlung & User Feedback
- [x] Async/Await für nicht-blockierende UI
- [x] Multi-Target Support (.NET 4.8.1 + 8.0)

### 📋 Benutzer-Workflow
1. **Konfiguration**:
   - Tools → Preferences
   - CouchDB URL eingeben (z.B. http://localhost:5984/canopen)
   - Save

2. **Export**:
   - File → Export to CouchDB...
   - Choose: Upload oder Save
   - Upload → CouchDB Server
   - Save → Lokale JSON-Datei

---

## Kompatibilität

- **Target Frameworks**: .NET Framework 4.8.1, .NET 8.0
- **Abhängigkeiten**:
  - Google.Protobuf 3.27.2
  - Newtonsoft.Json 13.0.3
  - System.Net.Http 4.3.4 (nur net481)
  - AutoMapper 10.0.0 / 13.0.1
- **OS**: Windows (net8.0-windows)

---

## Notizen

### Design-Entscheidungen
1. **WriteProtobuf als Basis**: Nutzt existierende, bewährte Serialisierung statt eigene JSON-Generierung
2. **HTTP PUT statt POST**: CouchDB-Standard für Document Upload mit ID
3. **Async Upload**: UI bleibt responsiv während HTTP-Request
4. **Dual-Mode Dialog**: Flexibilität für Upload und lokale Speicherung
5. **Settings-Persistierung**: URL wird über .NET Settings API gespeichert

### Sicherheit
- ⚠️ HTTP (kein HTTPS) - Für Development/Testing
- Keine Authentifizierung implementiert - Optional für Zukunft

---

## Version
- **Release**: 1.0
- **Datum**: Dezember 2024
- **Status**: ✅ Production Ready

