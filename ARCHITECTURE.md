# 🏗️ Architektur - Honktast.Test.InputAudio

## Übersicht

Das System besteht aus 5 Hauptkomponenten für Echtzeittonhöhenerkennung:

```
┌─────────────────┐
│   Mikrofon      │
│   (Audio-In)    │
└────────┬────────┘
         │
    ┌────▼────────────────────┐
    │   AudioCapture.cs       │
    │ (NAudio WaveInEvent)    │
    │ - Samples empfangen     │
    │ - Queue-Puffer          │
    └────┬─────────────────────┘
         │ Normalized Samples
    ┌────▼────────────────────┐
    │  PitchDetector.cs       │
    │  (YIN Algorithm)        │
    │ - 2048-Sample Buffer    │
    │ - Frequenz erkennen     │
    │ - Stabilisierung        │
    └────┬─────────────────────┘
         │ Frequency (Hz)
    ┌────▼────────────────────┐
    │ NoteConverter.cs        │
    │ - Freq → Note (C4)      │
    │ - Cents-Abweichung      │
    └────┬─────────────────────┘
         │
    ┌────▼────────────────────┐
    │    Program.cs           │
    │  (Benutzer-Interface)   │
    │ - Menü & Ausgabe        │
    └────────────────────────┘
```

## Komponenten

### 1. AudioCapture.cs
**Zweck:** Mikrofon-Eingabe & Audio-Pufferung

**Funktionsweise:**
```csharp
// NAudio WaveInEvent läuft asynchron
WaveInEvent → OnDataAvailable() → Queue<float>
```

**Wichtig:**
- 44.1 kHz Sample-Rate
- 16-bit PCM Audio
- 4x Signal-Verstärkung (für schwache Eingaben)
- Thread-sichere Queue mit Lock

### 2. PitchDetector.cs
**Zweck:** YIN-Algorithmus für Tonhöhenerkennung

**Architektur:**
```
Samples (Queue)
    ↓
[2048 samples genau?]
    ↓ JA
Hann-Fenster anwenden
    ↓
YIN-Algorithmus
    ├─ AMDF berechnen
    ├─ Normalisierte Differenzfunktion
    ├─ Minimum unter Schwelle?
    └─ Parabel-Interpolation für Präzision
    ↓
Frequenz validieren (40-2000 Hz)
    ↓
Stabilisierung (History-Queue)
    ↓
Finale Frequenz
```

**YIN-Parameter:**
- **Schwelle:** 0.35 (balanciert Sensitivität vs. Fehlerquote)
- **Buffer-Größe:** 2048 Samples
- **History-Größe:** 8 Samples
- **Stabilisierungs-Schwelle:** 25% Frequenz-Unterschied

**Queue-basierter Buffer:**
- Alte: Zirkulärer Index (fehleranfällig)
- Neu: Queue mit FIFO (einfacher, robuster)
- Automatische Entfernung ältester Samples

### 3. NoteConverter.cs
**Zweck:** Frequenz ↔ Noten-Konvertierung

**Formeln:**
```
Frequenz → Note:
  semitone = 12 × log₂(f / 440) + 57
  note = Notes[semitone % 12] + (semitone / 12 - 1)

Note → Frequenz:
  f = 440 × 2^((semitone - 57) / 12)

Cents-Differenz:
  cents = 1200 × log₂(f₁ / f₂)
```

**Standard:** A4 = 440 Hz (Konzert-Stimmton)

### 4. AudioDeviceManager.cs
**Zweck:** Mikrofon-Verwaltung

- Aufzählung verfügbarer Eingabegeräte
- Benutzerauswahl & Validierung
- Geräte-Testing

### 5. Program.cs
**Zweck:** Benutzer-Interface & Orchestrierung

**Menü:**
1. Live-Noten erkennen
2. Tonhöhe prüfen (Note → Frequenz)
3. Verfügbare Eingabegeräte
4. Info & Hilfe

## Datenfluss bei Live-Erkennung

```
1. Benutzer wählt Option 1 → Gerät wählen
2. AudioCapture startet Aufnahme
3. OnDataAvailable() wird aufgerufen (asynchron)
   - Samples → _audioBuffer (Queue)
4. StartAsync() Loop (50ms Intervalle)
   - Queue leeren
   - Samples zu PitchDetector.AddSamples()
   - Wenn Buffer voll (2048): DetectPitch()
5. PitchDetector gibt Frequenz zurück
6. NoteConverter konvertiert zu Note + Cents
7. Ausgabe auf Console
8. Benutzer drückt 'q' → Stop
```

## Performance

| Metrik | Wert |
|--------|------|
| Latenz | ~100-150ms (Buffer-Größe) |
| CPU-Last | Niedrig (~5-10%) |
| Speicher | ~10 MB |
| Genauigkeit | ±5 Cents bei stabilen Tönen |

## Fehlerbehandlung

- **Ungültige Frequenz:** YIN-Schwelle nicht erfüllt → null
- **Zu großer Frequenz-Sprung:** History leeren, neu starten
- **Keine Eingabegeräte:** Fehler anzeigen, beenden
- **Audio-Fehler:** Try-Catch in DetectPitch()

## Bekannte Limitierungen

1. **Oktaven-Verwechslung:** Sehr tiefe Noten können als Harmonische erkannt werden
2. **Akkorde:** Nur einzelne Frequenzen erkannt
3. **Vibrato:** Schnelle Tonhöhen-Schwankungen erzeugen Jitter
4. **Rauschen:** Hintergrundgeräusche beeinflussen Erkennung

## Kalibrierung für neue Hardware

Siehe `docs/CALIBRATION.md` für Anpassungen an andere Keyboards/Instrumente.
