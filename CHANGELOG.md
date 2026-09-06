# 📋 Changelog - Honktast.Test.InputAudio

Alle bemerkenswerten Änderungen in diesem Projekt sind in dieser Datei dokumentiert.

## [1.0.0] - 2026-09-06

### 🎉 Initial Release

Die erste vollständig funktionsfähige Version der Musik-Noten Erkennungs-Anwendung.

### ✨ Features

- 🎤 **Live-Noten-Erkennung**: Echtzeitnoten-Erkennung vom Mikrofon
- 🎹 **YIN-Algorithmus**: Robuste Tonhöhenerkennung (Standard in der Spracherkennung)
- 📊 **Noten-Anzeige**: Noten, Frequenz, Cents-Abweichung + Signal-Visualisierung
- 🔊 **Mehrere Eingabegeräte**: Unterstützung für verschiedene Mikrofone
- 🎚️ **Frequenz-Stabilisierung**: Queue-basierte Glättung für stabile Erkennung
- 🌐 **Benutzerfreundliches Menü**: Interaktive deutsche Benutzeroberfläche

### 🔧 Technische Details

**Unterstützte Hardware:**
- Casio SA-76 Keyboard (44 Tasten, C3-G6, 3.5 Oktaven)
- Beliebige Mikrofone über NAudio

**YIN-Parameter:**
- Schwelle: 0.35
- Buffer-Größe: 2048 Samples
- Sample-Rate: 44100 Hz
- Frequenzbereich: 40-2000 Hz
- Stabilisierungs-Fenster: 8 Frames

**Performance:**
- Latenz: ~100-150ms
- Genauigkeit: ±5 Cents bei stabilen Tönen
- CPU: Niedrig (~5-10%)

### 🐛 Bugs Behoben

#### Build 5dfc41f (Dokumentation)
- Projektmetadaten hinzugefügt
- Session-Tracking eingerichtet

#### Build e9f5b00 (YIN-Schwelle)
- Schwelle von 0.25 auf 0.35 erhöht
- Bessere Erkennung von Grundtönen (verhindert Oktaven-Verwechslung)

#### Build 14a6407 (Frequenzbereich-Kalibrierung)
- Maximale Frequenz von 500 Hz auf 2000 Hz erweitert
- Unterstützung für höhere Noten (bis G6)

#### Build b723e2c (Stabilisierungs-Lock)
- **CRITICAL FIX:** Stabilisierung sperrt sich nicht mehr auf erste Frequenz fest
- History wird jetzt zurückgesetzt bei >25% Frequenz-Unterschied
- Ermöglicht kontinuierliche Noten-Erkennung über breiten Bereich

#### Build 75e8eca (Debug-Cleanup)
- Debug-Ausgabe entfernt
- Saubere Produktions-Ausgabe

#### Build 7341da2 (Debug-Ausgabe)
- Diagnostik hinzugefügt (Raw vs. Stabilized Frequency)

#### Build 560d7b4 (YIN-Normalisierung) ⭐
- **CRITICAL FIX:** YIN-Normalisierungsformel korrigiert
- Alt: `df[lag] = df[lag] / (cumulativeSum / (lag + 1))`
- Neu: `df[lag] = df[lag] * lag / cumulativeSum` (Standard YIN)
- Dies war der Hauptgrund für fehlende Tonhöhenerkennung

#### Build ee1fe22 (Signal-Verstärkung)
- Audio-Signal 4x verstärkt (für schwache Eingaben)
- YIN-Schwelle von 0.1 auf 0.15 erhöht

#### Build 3970832 (Buffer-Architektur) ⭐
- **MAJOR REFACTOR:** Zirkulärer Buffer durch Queue ersetzt
- Alt: Komplexer Index-basierter zirkulärer Buffer
- Neu: Einfache FIFO-Queue (robuster, wartbarer)
- Bessere Signalerkennung, weniger Fehler

#### Build 4fca7e7 (Buffer-Logik)
- PitchDetector.AddSamples() gibt jetzt bool zurück (Buffer vollständig?)
- AudioCapture ruft DetectPitch() nur auf, wenn Buffer voll ist
- Behebt: Tonhöhenerkennung vor ausreichender Datenmenge

### 📝 Dokumentation

- ✅ README.md mit Projekt-Übersicht
- ✅ ARCHITECTURE.md mit System-Design
- ✅ CONTRIBUTING.md für Entwickler
- ✅ CALIBRATION.md für Hardware-Kalibrierung
- ✅ .claude/project.json mit Projekt-Metadaten
- ✅ .claude/session_summary.md für Session-Fortsetzen

### ⚠️ Bekannte Limitierungen

1. **C3 wird nicht erkannt** (sehr tiefe Note)
   - Grund: YIN erkennt Harmonische statt Grundton
   - Workaround: C4 funktioniert gut
   - Fix geplant: Harmonischen-Filterung

2. **Akkorde nicht unterstützt**
   - Erkennt nur einzelne Noten
   - Mehrere simultane Frequenzen werden gemittelt

3. **Vibrato erzeugt Jitter**
   - Schnelle Tonhöhen-Schwankungen
   - Langsames Vibrato wird gut erkannt

4. **Rauschen-Empfindlichkeit**
   - Funktioniert am besten in ruhigen Umgebungen

### 📦 Abhängigkeiten

- NAudio 2.2.1 (Audio-Verarbeitung)
- .NET 8.0 Runtime

### 🚀 Nächste Schritte

- [ ] Harmonischen-Filterung für tiefe Frequenzen
- [ ] Akkord-Erkennung
- [ ] Spektrogramm-Visualisierung
- [ ] WAV-Aufnahme & Playback
- [ ] MIDI-Export
- [ ] Graphische Benutzeroberfläche (WPF)

---

## Format

Dieses Projekt folgt [Semantic Versioning](https://semver.org/):
- **MAJOR:** Inkompatible API-Änderungen
- **MINOR:** Neue Features (abwärtskompatibel)
- **PATCH:** Bug-Fixes

**Typen von Änderungen:**
- ✨ Features - Neue Funktionalität
- 🐛 Bugs - Bug-Fixes
- 🔧 Änderungen - Verbesserungen ohne neue Features
- 📝 Docs - Dokumentations-Änderungen
- ⚡ Performance - Performance-Verbesserungen
- 🔐 Security - Sicherheits-Fixes
