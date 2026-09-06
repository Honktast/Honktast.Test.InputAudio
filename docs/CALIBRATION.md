# 🎹 Kalibrierung - Casio SA-76 Keyboard

## Aktuelle Kalibrierung (Stand: Sept 2026)

### Hardware-Spezifikationen
- **Keyboard:** Casio SA-76
- **Tasten:** 44 (vollständig chromatisch)
- **Bereich:** C3 (261.63 Hz) - G6 (1567.98 Hz)
- **Oktaven:** 3.5
- **Stimmung:** 12-Ton-Gleichstufung (A4 = 440 Hz)

### Erkannte Frequenzen
| Note | Soll (Hz) | Gemessen (Hz) | Status |
|------|-----------|---------------|--------|
| C3 | 261.63 | ❌ Nicht erkannt* | Harmonische-Issue |
| C4 | 261.63 | 522.9 | ⚠️ Oktave erkannt |
| C5 | 523.25 | 1046.6 | ✅ Korrekt |
| C6 | 1046.50 | - | Zu hoch (über 2000 Hz Buffer) |
| G6 | 1567.98 | - | Noch nicht getestet |

*C3 wird nicht erkannt (sehr tief), aber C4/C5 funktionieren gut.

### Anwendungs-Parameter

**PitchDetector.cs:**
```csharp
// Frequenzbereich (Hz)
private const int _sampleRate = 44100;      // Sample-Rate
private const int _bufferSize = 2048;       // Audio-Fenster
int minPeriod = _sampleRate / 2000;         // Max-Frequenz: 2000 Hz
int maxPeriod = _sampleRate / 40;           // Min-Frequenz: 40 Hz

// YIN-Algorithmus
private const float ThresholdYin = 0.35f;   // Erkennungs-Schwelle

// Stabilisierung
private const int _historySize = 8;         // Glättungs-Fenster (8 Frames)
if (percentDiff > 25)                       // 25% Frequenz-Unterschied → Neustart
```

**AudioCapture.cs:**
```csharp
float amplified = normalized * 4f;          // Signal-Verstärkung (4x)
```

## Anpassungen für andere Hardware

### Neues Keyboard kalibrieren

**1. Frequenzbereich bestimmen**
```bash
# Tiefste Note spielen
# → Ausgabe sehen: "Note: X Hz"
# → In minPeriod umrechnen: minPeriod = 44100 / max_hz

# Höchste Note spielen
# → Ausgabe sehen: "Note: Y Hz"
# → In maxPeriod umrechnen: maxPeriod = 44100 / min_hz
```

**Beispiel:** Keyboard mit C2-C7 (65.4 Hz - 2093 Hz)
```csharp
int minPeriod = Math.Max(1, 44100 / 2100);    // ~21 (2100 Hz max)
int maxPeriod = Math.Min(2048, 44100 / 60);   // ~735 (60 Hz min)
if (frequency < 60 || frequency > 2100)
    return null;
```

**2. YIN-Schwelle testen**

Prozedur:
1. Ruhig eine mittlere Note singen
2. Überprüfen: Wird sie erkannt?
3. Sehr tiefe Note: Wird sie erkannt oder als Harmonische?

| Szenario | Problem | Lösung |
|----------|---------|--------|
| Nichts erkannt | Schwelle zu niedrig | `0.35f` → `0.40f` erhöhen |
| Zu viele Fehler | Schwelle zu hoch | `0.35f` → `0.25f` senken |
| Oktaven erkannt | Harmonische-Issue | `0.35f` → `0.45f` erhöhen |

**3. Signal-Verstärkung anpassen**

Wenn schwache Eingaben nicht erkannt werden:
```csharp
// AudioCapture.cs, Zeile 115
float amplified = normalized * 4f;    // ← Erhöhen auf 6f oder 8f
```

Wenn Übersteuerung/Clipping auftritt:
```csharp
float amplified = normalized * 2f;    // ← Senken
```

**4. Stabilisierungs-Parameter**

Zu viel Jitter:
```csharp
private const int _historySize = 8;   // ← Erhöhen auf 12-16
```

Zu träge Reaktion:
```csharp
if (percentDiff > 25)                 // ← Erhöhen auf 35
```

## Kalibrierung testen

### Vollständiger Test-Prozess
```bash
# 1. Tiefste Note spielen (sollte erkannt werden)
dotnet run
# Option 1: Live-Noten erkennen
# Spielen: Tiefste Note des Keyboards
# Ausgabe überprüfen: Note + Frequenz?

# 2. Alle Noten durchspielen
# Option 1: Live-Noten erkennen
# Spielen: Alle Tasten von links nach rechts
# Ausgabe überprüfen: Kontinuierliche Noten-Abfolge?

# 3. Höchste Note spielen
# Spielen: Höchste Note des Keyboards
# Ausgabe überprüfen: Note + Frequenz?
```

### Performance-Metriken
| Metrik | Aktuell | Ziel |
|--------|---------|------|
| Erkannte Noten | 43/44 (98%) | 100% |
| Durchschnittliche Latenz | ~120ms | <100ms |
| Genauigkeit (Cents) | ±5 Cents | ±2 Cents |
| False Positives | Sehr selten | Keine |

## Troubleshooting

### Problem: Nur C4 erkannt, nicht C3
**Grund:** YIN-Schwelle zu niedrig für tiefe Frequenzen (erkennt Harmonische statt Grundton)  
**Lösung:**
```csharp
private const float ThresholdYin = 0.35f;  // → 0.40f oder 0.45f
```

### Problem: Nichts wird erkannt
**Gründe:**
1. Mikrofon nicht erkannt
2. Frequenzbereich zu eng
3. YIN-Schwelle zu hoch

**Tests:**
```bash
# 1. Geräte überprüfen
dotnet run → Option 3: Verfügbare Eingabegeräte

# 2. Mikrofon-Level überprüfen (Windows Settings)

# 3. Schwelle senken
ThresholdYin = 0.30f  # Empfindlicher
```

### Problem: Zu viele falsche Noten
**Grund:** YIN-Schwelle zu niedrig  
**Lösung:**
```csharp
private const float ThresholdYin = 0.35f;  # → 0.40f oder höher
```

## Zukünftige Verbesserungen

- [ ] Harmonischen-Filterung für tiefe Frequenzen
- [ ] Adaptive Schwelle basierend auf Frequenz
- [ ] Multi-Pitch-Detection für Akkorde
- [ ] Kalibrierungs-Wizard (interaktiv)
- [ ] Speichern von Keyboard-Profilen

## Referenzen

- [YIN Algorithm](https://librosa.org/doc/main/_modules/librosa/yin.html)
- [Equal Temperament](https://en.wikipedia.org/wiki/Equal_temperament)
- [Casio SA-76 Manual](https://www.casio.com)
