using System.Text;

public class SessionRecorder : IDisposable
{
    private readonly string _filePath;
    private readonly StreamWriter _writer;
    private DateTime _sessionStart;
    private string? _lastNote;
    private DateTime _lastNoteTime;
    private bool _isDisposed;

    public SessionRecorder()
    {
        _sessionStart = DateTime.Now;
        string sessionId = Guid.NewGuid().ToString().Substring(0, 8);
        string fileName = _sessionStart.ToString("dd.MM.yyyy-HH-mm-ss") + $"-{sessionId}.txt";

        string musicDir = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        string recordingsDir = Path.Combine(musicDir, "Eigene Musik", "Sessions");
        if (!Directory.Exists(recordingsDir))
        {
            Directory.CreateDirectory(recordingsDir);
        }

        _filePath = Path.Combine(recordingsDir, fileName);
        _writer = new StreamWriter(_filePath, append: false, encoding: Encoding.UTF8);

        WriteHeader();
    }

    private void WriteHeader()
    {
        _writer.WriteLine("╔════════════════════════════════════════╗");
        _writer.WriteLine("║       KEYBOARD SESSION RECORDING        ║");
        _writer.WriteLine("╚════════════════════════════════════════╝");
        _writer.WriteLine($"Sitzung gestartet: {_sessionStart:dd.MM.yyyy HH:mm:ss}");
        _writer.WriteLine($"Speichert in: {Path.GetDirectoryName(_filePath)}");
        _writer.WriteLine(new string('─', 50));
        _writer.WriteLine("Zeit\t\tNote\t\tDauer (ms)\tFrequenz");
        _writer.WriteLine(new string('─', 50));
        _writer.Flush();
    }

    public void RecordNote(string note, float frequency)
    {
        if (_isDisposed) return;

        DateTime now = DateTime.Now;

        if (_lastNote == note)
        {
            return;
        }

        if (_lastNote != null)
        {
            int duration = (int)(now - _lastNoteTime).TotalMilliseconds;
            float lastFreq = NoteConverter.NoteToFrequency(_lastNote);
            _writer.WriteLine($"{_lastNoteTime:HH:mm:ss.fff}\t{_lastNote}\t\t{duration}\t\t{lastFreq:F1} Hz");
        }

        _lastNote = note;
        _lastNoteTime = now;
        _writer.Flush();
    }

    public void EndNote()
    {
        if (_isDisposed || _lastNote == null) return;

        DateTime now = DateTime.Now;
        int duration = (int)(now - _lastNoteTime).TotalMilliseconds;
        float frequency = NoteConverter.NoteToFrequency(_lastNote);

        _writer.WriteLine($"{_lastNoteTime:HH:mm:ss.fff}\t{_lastNote}\t\t{duration}\t\t{frequency:F1} Hz");
        _writer.Flush();

        _lastNote = null;
    }

    public void Close()
    {
        if (_isDisposed) return;

        EndNote();
        _writer.WriteLine(new string('─', 50));
        _writer.WriteLine($"Sitzung beendet: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        _writer.WriteLine($"Datei gespeichert: {_filePath}");
        _writer.Close();
        _isDisposed = true;
    }

    public void Dispose()
    {
        Close();
        _writer?.Dispose();
    }
}
