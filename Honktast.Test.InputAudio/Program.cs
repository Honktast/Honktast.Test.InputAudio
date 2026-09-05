using NAudio.Wave;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("╔════════════════════════════════════════╗");
Console.WriteLine("║    🎵🎵 Musik-Noten Erkenner🎵🎵       ║");
Console.WriteLine("║                                        ║");
Console.WriteLine("║  Mikrofon-Eingabe in Noten umwandeln   ║");
Console.WriteLine("╚════════════════════════════════════════╝\n");

while (true)
{
    Console.WriteLine("\nWählen Sie einen Modus:");
    Console.WriteLine("1. 🎤 Live-Noten erkennen");
    Console.WriteLine("2. 🎹 Tonhöhe prüfen (Note → Frequenz)");
    Console.WriteLine("3. 🔊 Verfügbare Eingabegeräte");
    Console.WriteLine("4. 📊 Info & Hilfe");
    Console.WriteLine("0. ❌ Beenden");
    Console.Write("\nWahl: ");

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await RunLiveNoteDetection();
            break;
        case "2":
            TestFrequency();
            break;
        case "3":
            ShowAvailableDevices();
            break;
        case "4":
            ShowInfo();
            break;
        case "0":
            Console.WriteLine("\nAuf Wiedersehen!");
            return;
        default:
            Console.WriteLine("⚠️  Ungültige Eingabe!");
            break;
    }
}

async Task RunLiveNoteDetection()
{
    int deviceIndex = AudioDeviceManager.SelectInputDevice();

    if (deviceIndex < 0)
        return;

    if (!AudioDeviceManager.TestDevice(deviceIndex))
        return;

    Console.WriteLine("\n🎤 Live-Noten-Erkennung wird gestartet...\n");

    try
    {
        using var audioCapture = new AudioCapture(deviceIndex);
        await audioCapture.StartAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Fehler: {ex.Message}");
    }
}

void TestFrequency()
{
    Console.Write("\nGeben Sie eine Note ein (z.B. A4, C5, G3): ");
    string? note = Console.ReadLine()?.ToUpperInvariant().Trim();

    if (string.IsNullOrEmpty(note))
    {
        Console.WriteLine("⚠️  Ungültige Eingabe!");
        return;
    }

    float frequency = NoteConverter.NoteToFrequency(note);
    if (frequency > 0)
    {
        Console.WriteLine($"\n✓ Note: {note}");
        Console.WriteLine($"  Frequenz: {frequency:F2} Hz");
    }
    else
    {
        Console.WriteLine("⚠️  Ungültige Note!");
    }
}

void ShowInfo()
{
    Console.WriteLine("\n╔════════════════════════════════════════╗");
    Console.WriteLine("║            ℹ️  INFORMATIONEN            ║");
    Console.WriteLine("╚════════════════════════════════════════╝\n");

    Console.WriteLine("Diese Anwendung erkennt Musiknoten durch:");
    Console.WriteLine("  • Mikrofon-Eingabe aufnehmen");
    Console.WriteLine("  • FFT (Fast Fourier Transform) für Spektralanalyse");
    Console.WriteLine("  • Tonhöhen-Detektion (Pitch Detection)");
    Console.WriteLine("  • Umwandlung in Musiknoten\n");

    Console.WriteLine("Unterstützte Noten: C bis B mit Vorzeichen (#)");
    Console.WriteLine("Oktavbereich: 0-8 (z.B. C4, A4, B7)\n");

    Console.WriteLine("Beispiel-Noten mit Frequenzen:");
    Console.WriteLine($"  C4:  {NoteConverter.NoteToFrequency("C4"):F2} Hz");
    Console.WriteLine($"  A4:  {NoteConverter.NoteToFrequency("A4"):F2} Hz (Konzert-Stimmton)");
    Console.WriteLine($"  C5:  {NoteConverter.NoteToFrequency("C5"):F2} Hz\n");
}

void ShowAvailableDevices()
{
    Console.WriteLine("\n╔════════════════════════════════════════╗");
    Console.WriteLine("║      🔊 VERFÜGBARE EINGABEGERÄTE       ║");
    Console.WriteLine("╚════════════════════════════════════════╝\n");

    var devices = AudioDeviceManager.GetAvailableInputDevices();

    if (devices.Count == 0)
    {
        Console.WriteLine("❌ Keine Eingabegeräte gefunden!");
        return;
    }

    for (int i = 0; i < devices.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {devices[i].Name}");
    }
    Console.WriteLine();
}
