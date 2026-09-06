using NAudio.Wave;

public class AudioDeviceManager
{
    public static List<(int Index, string Name)> GetAvailableInputDevices()
    {
        var devices = new List<(int, string)>();

        for (int i = 0; i < WaveInEvent.DeviceCount; i++)
        {
            var capabilities = WaveInEvent.GetCapabilities(i);
            devices.Add((i, capabilities.ProductName));
        }

        return devices;
    }

    public static int SelectInputDevice()
    {
        var devices = GetAvailableInputDevices();

        if (devices.Count == 0)
        {
            Console.WriteLine("❌ Keine Eingabegeräte gefunden!");
            return -1;
        }

        Console.WriteLine("\n🎤 Verfügbare Eingabegeräte:\n");
        for (int i = 0; i < devices.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {devices[i].Name}");
        }

        while (true)
        {
            Console.Write("\nWählen Sie ein Gerät (Nummer eingeben): ");
            if (int.TryParse(Console.ReadLine(), out int choice) &&
                choice > 0 && choice <= devices.Count)
            {
                return devices[choice - 1].Index;
            }

            Console.WriteLine("⚠️  Ungültige Eingabe!");
        }
    }

    public static int SelectOutputDevice()
    {
        Console.WriteLine("\n🔊 Standardausgabegerät wird verwendet (Kopfhörer/Lautsprecher)");
        return -1;
    }

    public static bool TestDevice(int deviceIndex)
    {
        try
        {
            using (var waveIn = new WaveInEvent { DeviceNumber = deviceIndex })
            {
                Console.WriteLine($"✓ Gerät '{WaveInEvent.GetCapabilities(deviceIndex).ProductName}' verfügbar");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fehler beim Testen des Geräts: {ex.Message}");
            return false;
        }
    }
}
