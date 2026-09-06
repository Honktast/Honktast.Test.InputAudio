using NAudio.Wave;

public class AudioLoopback : IDisposable
{
    private readonly IWaveIn _waveIn;
    private readonly IWavePlayer _waveOutPlayer;
    private readonly WaveFormat _waveFormat;
    private BufferedWaveProvider? _waveProvider;
    private bool _isRunning;

    public AudioLoopback(int inputDeviceIndex, int outputDeviceIndex)
    {
        _waveFormat = new WaveFormat(44100, 16, 1);

        _waveIn = new WaveInEvent
        {
            DeviceNumber = inputDeviceIndex,
            WaveFormat = _waveFormat
        };

        _waveOutPlayer = new WaveOutEvent
        {
            DeviceNumber = outputDeviceIndex
        };

        _waveProvider = new BufferedWaveProvider(_waveFormat);
        _waveOutPlayer.Init(_waveProvider);
        _isRunning = false;

        _waveIn.DataAvailable += OnDataAvailable;
    }

    public void Start()
    {
        _isRunning = true;
        _waveOutPlayer.Play();
        _waveIn.StartRecording();
        Console.WriteLine("🔄 Loopback aktiv - Mikrofon wird auf Kopfhörer durchgeschleift");
    }

    public void Stop()
    {
        _isRunning = false;
        _waveIn.StopRecording();
        _waveOutPlayer.Stop();
        Console.WriteLine("✓ Loopback beendet");
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (!_isRunning || _waveProvider == null) return;

        _waveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
    }

    public void Dispose()
    {
        Stop();
        _waveIn?.Dispose();
        _waveOutPlayer?.Dispose();
    }
}
