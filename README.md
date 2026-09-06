# 🎵 Honktast.Test.InputAudio - Musik-Noten Erkenner

Eine C# .NET 8.0 Konsolenanwendung, die Mikrofoneingaben analysiert und in Echtzeit als Musiknoten interpretiert.

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com)
[![License: Proprietary](https://img.shields.io/badge/License-Proprietary-red.svg)](LICENSE)
[![Status: Active](https://img.shields.io/badge/Status-Active-brightgreen.svg)]()

---

### 🤖 Claude Development Session
**Letzte Sitzung:** https://claude.ai/code/session_01J269EB9hrYSkyMRLCBRKYo  
**Status:** Alle 44 Tasten des Casio SA-76 funktionieren korrekt ✅  
**Hardware:** Casio SA-76 Keyboard (C3-G6, 3.5 Oktaven)

Siehe `.claude/project.json` für Projektdetails und Quick-Commands.

---

## 📋 Inhaltsverzeichnis

- [Features](#features)
- [Technologie](#technologie)
- [Installation](#installation)
- [Verwendung](#verwendung)
- [Architektur](#architektur)
- [Bekannte Limitierungen](#bekannte-limitierungen)
- [Zukünftige Verbesserungen](#zukünftige-verbesserungen)

## ✨ Features

- 🎤 **Live-Noten-Erkennung**: Erkennt Musiknoten in Echtzeit vom Mikrofon
- 🎹 **Tonhöhen-Analyse**: Wandelt Frequenzen in Musiknoten um (z.B. 440 Hz → A4)
- 🔊 **Mikrofon-Auswahl**: Unterstützt mehrere Eingabegeräte
- 📊 **Cent-Abweichung**: Zeigt Abweichung von perfekten Noten an
- 📈 **Signal-Visualisierung**: Bar-Graph für Signalstärke
- 🎚️ **Frequenz-Stabilisierung**: Mittelwert der letzten Messungen
- 🌐 **Benutzerfreundliches Menü**: Interaktive Schnittstelle

## 🔬 Technologie

### Pitch Detection (Tonhöhenerkennung)

Die Anwendung verwendet den **YIN-Algorithmus** für robuste Tonhöhenerkennung:

```
YIN-Algorithmus:
1. AMDF (Average Magnitude Difference Function) berechnen
2. Normalisierte Differenzfunktion
3. Schwellwert-Pruning (YIN-Schwelle = 0.1)
4. Parabel-Interpolation für Sub-Sample-Genauigkeit
5. Frequenz-Konvertierung und Stabilisierung
```

**Vorteile gegenüber FFT:**
- Robuster gegen Rauschen
- Bessere Grundton-Erkennung
- Weniger anfällig für Harmonische
- Standard in der Spracherkennung

### Note-Konvertierung

- **A4 = 440 Hz**: Standard-Konzert-Stimmton
- **12-Ton-Gleichstufung**: Mathematische Grundlage
- **Formel**: `f = A4 × 2^((n - 57) / 12)`
- **Cent-Berechnung**: 1200 × log₂(f₁/f₂)

## 📦 Installation

### Voraussetzungen

- **.NET 8.0 SDK** oder später
- **Windows/Linux/macOS** mit Mikrofon-Unterstützung
- Visual Studio oder VS Code (optional)

### Setup

```bash
# Repository klonen
git clone https://github.com/YOUR_USERNAME/Honktast.Test.InputAudio.git
cd Honktast.Test.InputAudio

# Abhängigkeiten wiederherstellen
dotnet restore

# Projekt bauen
dotnet build

# Projekt ausführen
dotnet run
```

## 🚀 Verwendung

### Programm starten

```bash
cd Honktast.Test.InputAudio
dotnet run
```

### Menü-Optionen

```
╔════════════════════════════════════════╗
║    🎵 Musik-Noten Erkenner 🎵         ║
║                                        ║
║  Mikrofon-Eingabe in Noten umwandeln   ║
╚════════════════════════════════════════╝

Wählen Sie einen Modus:
1. 🎤 Live-Noten erkennen
2. 🎹 Tonhöhe prüfen (Note → Frequenz)
3. 🔊 Verfügbare Eingabegeräte
4. 📊 Info & Hilfe
0. ❌ Beenden
```

### Beispiel: Live-Noten Erkennung

1. Wählen Sie **Option 1**
2. Wählen Sie Ihr Mikrofon aus der Liste
3. System testet Gerät automatisch
4. Singen Sie eine Note
5. Ausgabe:
   ```
   Note    Frequenz        Cents   Signal
   --------------------------------------------------
   C4      261.63 Hz       +0.00¢  ████░░░░░░
   C4      261.63 Hz       -0.05¢  ████░░░░░░
   ```
6. Drücke **'q'** zum Beenden

### Beispiel: Tonhöhe testen

1. Wählen Sie **Option 2**
2. Geben Sie eine Note ein (z.B. **A4**, **C5**, **G3**)
3. System zeigt Frequenz:
   ```
   ✓ Note: A4
   Frequenz: 440.00 Hz
   ```

### Beispiel: Verfügbare Geräte

1. Wählen Sie **Option 3**
2. System listet alle Eingabegeräte auf:
   ```
   🔊 VERFÜGBARE EINGABEGERÄTE
   
   1. Microphone (USB Audio Device)
   2. Headset Microphone
   3. Line In
   ```

## 🏗️ Architektur

### Projektstruktur

```
Honktast.Test.InputAudio/
├── Program.cs                    # Hauptprogramm, Menü-System
├── AudioCapture.cs              # Mikrofon-Eingabe, Audio-Verarbeitung
├── PitchDetector.cs             # YIN-Algorithmus, Tonhöhenerkennung
├── NoteConverter.cs             # Frequenz ↔ Note Konvertierung
├── AudioDeviceManager.cs        # Geräte-Verwaltung
├── Honktast.Test.InputAudio.csproj
└── README.md
```

### Klassen

#### `Program.cs`
- Benutzerinterface und Menü-System
- Orchestrierung aller Komponenten
- Error-Handling

#### `AudioCapture.cs`
- Mikrofon-Eingabe via NAudio WaveInEvent
- Thread-sichere Audio-Puffer (Queue)
- Real-time Datenverarbeitung
- Signal-Stärke-Berechnung (RMS)

#### `PitchDetector.cs`
- **YIN-Algorithmus** für Tonhöhenerkennung
- AMDF (Average Magnitude Difference Function)
- Hann-Fenster für Spektralanalyse
- Parabel-Interpolation für Sub-Sample-Genauigkeit
- Frequenz-Stabilisierung (Queue-basierter Durchschnitt)

#### `NoteConverter.cs`
- Konvertierung: Frequenz → Note (z.B. 440 Hz → A4)
- Konvertierung: Note → Frequenz (z.B. A4 → 440 Hz)
- Cent-Abweichung-Berechnung
- Standard: A4 = 440 Hz

#### `AudioDeviceManager.cs`
- Enumerierung verfügbarer Eingabegeräte
- Interaktive Geräteauswahl
- Gerät-Validierung und Testing

## 📊 Note-Referenzen

### C-Dur Skala (Oktave 4)

| Note | Frequenz | Cents (vs. A4) |
|------|----------|----------------|
| C4   | 261.63 Hz | -4800¢ |
| D4   | 293.66 Hz | -4300¢ |
| E4   | 329.63 Hz | -3800¢ |
| F4   | 349.23 Hz | -3500¢ |
| G4   | 392.00 Hz | -2900¢ |
| A4   | 440.00 Hz | 0¢ (Referenz) |
| B4   | 493.88 Hz | +1100¢ |
| C5   | 523.25 Hz | +1200¢ |

## ⚙️ Parameter und Kalibrierung

### YIN-Algorithmus Parameter

```csharp
// In PitchDetector.cs
const float ThresholdYin = 0.1f;           // YIN-Schwelle (0.0-1.0)
private const int _bufferSize = 4096;      // FFT-Größe
private const int _historySize = 8;        // Stabilisierungs-Fenster
private const int _sampleRate = 44100;     // Sample-Rate (Hz)
```

### Frequenzbereich

- **Minimum**: 40 Hz (sehr tiefe männliche Stimme)
- **Maximum**: 500 Hz (optimiert für Gesang)
- **Standard A4**: 440 Hz (Konzert-Stimmton)

### Anpassung

Um den Standard-Stimmton zu ändern (z.B. A4 = 432 Hz):

```csharp
// In NoteConverter.cs
private const float A4_FREQUENCY = 432f;  // Statt 440f
```

## 🔊 Mikrofon-Empfehlungen

Beste Ergebnisse mit:
- USB-Mikrofone (bessere Qualität)
- Externe Audio-Interfaces
- Headset-Mikrofone mit Noise-Cancellation

Zu vermeiden:
- Eingebaute Laptop-Mikrofone (Rauschen)
- Bluetooth-Mikrofone (Latenzverzögerung)
- Stark verrauschte Umgebungen

## 📈 Beispiel-Ausgaben

### Perfekte Note (C4)
```
Note    Frequenz        Cents   Signal
--------------------------------------------------
C4      261.63 Hz       +0.00¢  ████░░░░░░
C4      261.62 Hz       -0.04¢  ████░░░░░░
C4      261.64 Hz       +0.02¢  ████░░░░░░
```

### Note mit Vibrato (A4)
```
Note    Frequenz        Cents   Signal
--------------------------------------------------
A4      440.00 Hz       +0.00¢  █████░░░░░
A4      442.50 Hz       +8.16¢  █████░░░░░
A4      439.20 Hz       -2.82¢  █████░░░░░
A4      440.85 Hz       +3.36¢  █████░░░░░
```

### Tiefe Note (C2)
```
Note    Frequenz        Cents   Signal
--------------------------------------------------
C2      65.41 Hz        +0.00¢  ██░░░░░░░░
C2      65.40 Hz        -0.02¢  ██░░░░░░░░
C2      65.43 Hz        +0.05¢  ██░░░░░░░░
```

## ⚠️ Bekannte Limitierungen

1. **Harmonische-Anfälligkeit**
   - Sehr hohe Stimmen können Harmonische statt Grundton erkennen
   - Frequenzbereich auf 40-500 Hz begrenzt (optimiert für Stimme)

2. **Akkord-Erkennung**
   - Erkennt nur einzelne Noten
   - Mehrere simultane Frequenzen werden durchschnittlich erkannt

3. **Latenz**
   - ~100-150ms Verzögerung durch Buffer-Größe
   - YIN-Algorithmus benötigt Zeit für genaue Erkennung

4. **Rauschen-Empfindlichkeit**
   - Funktioniert am besten in ruhigen Umgebungen
   - Hintergrundgeräusche können Erkennung beeinflussen

5. **Vibrato und Ornamente**
   - Schnelle Tonhöhen-Schwankungen können zu Noise führen
   - Langsames Vibrato wird erkannt (±20 Hz tolerant)

## 🚀 Zukünftige Verbesserungen

- [ ] **Harmonische-Filterung**: Bessere Reduktion von Harmonischen
- [ ] **Akkord-Erkennung**: Mehrere simultane Noten
- [ ] **Spektrogramm-Visualisierung**: Grafische Darstellung
- [ ] **Aufnahme & Playback**: WAV-Dateien speichern
- [ ] **MIDI-Export**: MusicXML oder Standard MIDI
- [ ] **Datenbank**: Noten-Sequenzen speichern
- [ ] **Mehrsprachige UI**: Englisch, Deutsch, etc.
- [ ] **WPF-UI**: Graphische Benutzeroberfläche
- [ ] **Real-time Feedback**: Visuelle Stimm-Intonations-Anzeige
- [ ] **Performance-Optimierung**: Native FFT-Bibliotheken

## 🔧 Troubleshooting

### Problem: "Keine Eingabegeräte gefunden"
```
Lösung: 
- Stellen Sie sicher, dass Mikrofon mit USB verbunden ist
- Testen Sie in Windows-Audioeinstellungen
- Starten Sie die Anwendung neu
```

### Problem: Falsche Noten werden erkannt
```
Lösungen:
1. Stelle sicher, dass du in den 40-500 Hz Bereich singst
2. Verwende ein besseres Mikrofon
3. Singe reinere Töne ohne Vibrato
4. Reduziere Hintergrundgeräusche
```

### Problem: Sehr instabile Erkennung
```
Lösungen:
1. Erhöhe Mikrofon-Lautstärke
2. Verwende Externes USB-Mikrofon
3. Teste mit Option 3 ob Gerät funktioniert
4. Überprüfe Audioeinstellungen des Systems
```

## 📄 Lizenz

Dieses Projekt ist **Proprietary** - alle Rechte vorbehalten.
Siehe [LICENSE](LICENSE) für Details.

⚠️ **Nur zur persönlichen Nutzung durch Paul Heinz Korsig.**

## 👤 Autor

Paul Heinz Korsig
- Email: paul-heinz.korsig@alice.de

## 📞 Support

Bei Fragen oder Problemen:
1. Überprüfe die [Known Issues](#bekannte-limitierungen)
2. Lese das [Troubleshooting](#troubleshooting)-Kapitel
3. Erstelle ein Issue im Repository

## 🙏 Danksagungen

- NAudio-Bibliothek für Audio-Verarbeitung
- YIN-Algorithmus (Cheveigne & Kawahara, 2002)
- .NET Foundation für C# und .NET

## 📚 Referenzen

- [YIN: A Fundamental Frequency Estimator for Speech and Music](https://librosa.org/doc/main/_modules/librosa/yin.html)
- [NAudio Documentation](https://github.com/naudio/NAudio)
- [Equal Temperament Tuning](https://en.wikipedia.org/wiki/Equal_temperament)

---

**Status**: Aktive Entwicklung 🚧
**Letzte Aktualisierung**: September 5, 2026
**Version**: 1.0.0
