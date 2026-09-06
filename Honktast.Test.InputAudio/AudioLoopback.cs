using NAudio.Wave;
using NAudio.CoreAudioApi;

public class AudioLoopback : IDisposable
{
    private readonly IWaveIn _waveIn;
    private readonly WasapiOut? _wasapiOut;
    private readonly WaveOutEvent? _waveOut;
    private readonly WaveFormat _waveFormat;
    private BufferedWaveProvider? _waveProvider;
    private bool _isRunning;
    private bool _useWasapi;

    public AudioLoopback(int inputDeviceIndex, int outputDeviceIndex)
    {
        _waveFormat = new WaveFormat(44100, 16, 1);
        _useWasapi = false;

        _waveIn = new WaveInEvent
        {
            DeviceNumber = inputDeviceIndex,
            WaveFormat = _waveFormat,
            BufferMilliseconds = 20
        };

        _waveProvider = new BufferedWaveProvider(_waveFormat)
        {
            BufferDuration = TimeSpan.FromMilliseconds(200)
        };

        try
        {
            _wasapiOut = new WasapiOut(AudioClientShareMode.Exclusive, 20);
            _wasapiOut.Init(_waveProvider);
            _useWasapi = true;
            Console.WriteLine("  (WASAPI Exclusive Mode aktiviert - Minimum Latenz)");
        }
        catch
        {
            Console.WriteLine("  (WASAPI nicht verfügbar, fallback zu WaveOut)");
            _wasapiOut?.Dispose();
            _wasapiOut = null;

            _waveOut = new WaveOutEvent
            {
                DeviceNumber = outputDeviceIndex
            };
            _waveOut.Init(_waveProvider);
            _useWasapi = false;
        }

        _isRunning = false;
        _waveIn.DataAvailable += OnDataAvailable;
    }

    public void Start()
    {
        _isRunning = true;

        if (_useWasapi)
            _wasapiOut?.Play();
        else
            _waveOut?.Play();

        _waveIn.StartRecording();
        Console.WriteLine("🔄 Loopback aktiv - Mikrofon wird auf Kopfhörer durchgeschleift (Low Latency)");
    }

    public void Stop()
    {
        _isRunning = false;
        _waveIn.StopRecording();

        if (_useWasapi)
            _wasapiOut?.Stop();
        else
            _waveOut?.Stop();

        Console.WriteLine("✓ Loopback beendet");
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (!_isRunning || _waveProvider == null) return;

        try
        {
            _waveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }
        catch (InvalidOperationException)
        {
            // Buffer voll - verwerfe älteste Daten statt zu crashen
            if (_waveProvider.BufferedBytes > 0)
            {
                byte[] temp = new byte[e.BytesRecorded];
                _waveProvider.Read(temp, 0, Math.Min(temp.Length, _waveProvider.BufferedBytes));
                _waveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
            }
        }
    }

    public void Dispose()
    {
        Stop();
        _waveIn?.Dispose();
        _wasapiOut?.Dispose();
        _waveOut?.Dispose();
    }
}
