# 🤝 Beitragen - Honktast.Test.InputAudio

Danke, dass du zu diesem Projekt beitragen möchtest! 🎵

## Setup für Entwicklung

### Voraussetzungen
- **.NET 8.0 SDK** oder später
- **Visual Studio** / **VS Code** (optional)
- **Git**

### Projekt klonen & starten
```bash
git clone https://github.com/Honktast/Honktast.Test.InputAudio.git
cd Honktast.Test.InputAudio

# Abhängigkeiten wiederherstellen
dotnet restore

# Projekt bauen
dotnet build

# Projekt ausführen
dotnet run
```

## Development Workflow

### 1. Feature/Bug-Branch erstellen
```bash
git checkout -b feature/deine-feature
# oder
git checkout -b fix/dein-bug
```

### 2. Code schreiben
- Folge den bestehenden Code-Style (C# Konventionen)
- Dokumentiere komplexe Funktionen
- Teste gründlich

### 3. Lokales Testen
```bash
dotnet build
dotnet run
```

### 4. Commit & Push
```bash
git add .
git commit -m "Feature/Fix: Kurze Beschreibung"
git push origin dein-branch
```

### 5. Pull Request erstellen
- Aussagekräftigen Titel verwenden
- Beschreibe das Problem und die Lösung
- Testen bestätigen

## Code-Style Guidelines

### Naming
```csharp
// Klassen & Methoden: PascalCase
public class AudioCapture { }
public void StartRecording() { }

// Private Felder: _camelCase
private float _lastFrequency;

// Konstanten: UPPER_CASE
private const float ThresholdYin = 0.35f;
```

### Dokumentation
```csharp
// Kurz halten - nur für nicht-offensichtliche Logik
private float? StabilizeFrequency(float frequency)
{
    // Reset history if frequency jump > 25% (allow note changes)
    if (percentDiff > 25)
    {
        _frequencyHistory.Clear();
    }
}
```

### Audio-Verarbeitung
- Immer `float` für Samples verwenden (nicht `double`)
- Sample-Rate = 44100 Hz
- Buffer-Größe = 2048 Samples (Standard)
- YIN-Schwelle = 0.35 (nicht verändern ohne Testing)

## Häufige Änderungen

### Frequenzbereich anpassen
**Datei:** `PitchDetector.cs`, Zeile 72-73, 146
```csharp
int minPeriod = Math.Max(1, _sampleRate / 2000); // Max-Frequenz (Hz)
int maxPeriod = Math.Min(signal.Length / 2, _sampleRate / 40); // Min-Frequenz

if (frequency < 40 || frequency > 2000)  // Gültiger Bereich
    return null;
```

### YIN-Schwelle optimieren
**Datei:** `PitchDetector.cs`, Zeile 11
```csharp
private const float ThresholdYin = 0.35f;  // ← Anpassen
// Höher = lockerer (mehr falsche Positive)
// Niedriger = strenger (mehr Misses)
```

### Stabilisierungs-Parameter
**Datei:** `PitchDetector.cs`, Zeile 10, 162
```csharp
private readonly int _historySize = 8;      // Glättungs-Fenster
if (percentDiff > 25)  // Schwellenwert für Neustart (%)
```

## Testing

Manuelles Testing mit Casio SA-76 Keyboard:
1. App starten
2. Option 1: "Live-Noten erkennen"
3. Alle 44 Tasten (C3-G6) spielen
4. Ausgabe überprüfen: Richtige Noten + stabile Frequenzen?

## Bug Reports

Verwende GitHub Issues mit:
- **Titel:** Kurze Beschreibung
- **Beschreibung:** 
  - Was ist schiefgegangen?
  - Wie kann man es reproduzieren?
  - Welches Mikrofon/Keyboard?
- **Logs:** Debug-Ausgabe falls verfügbar

## Performance-Verbesserungen

Vor einer Optimierung:
1. Profiling durchführen (Visual Studio)
2. Bottleneck identifizieren
3. Änderungen testen
4. Benchmarks dokumentieren

## Release-Prozess

Nur Projekt-Maintainer:
```bash
# Version in CHANGELOG.md aktualisieren
# Commit & Tag erstellen
git tag v1.1.0
git push origin v1.1.0
```

## Fragen?

- **Issues:** GitHub Issues für Bugs/Features
- **Discussions:** GitHub Discussions für Fragen
- **Code Review:** PR-Comments für Feedback

Danke für deine Hilfe! 🎉
