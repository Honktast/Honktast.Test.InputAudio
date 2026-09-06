using NAudio.Wave;

public class AudioLoopback : IDisposable
{
    private readonly IWaveIn _waveIn;
    private readonly IWavePlayer _wavePlayer;
    private readonly WaveFormat _waveFormat;
    private readonly Queue<byte> _ringBuffer;
    private readonly object _bufferLock = new();
    private bool _isRunning;
    private const int RingBufferSize = 4410;

    public AudioLoopback(int inputDeviceIndex, int outputDeviceIndex)
    {
        _waveFormat = new WaveFormat(44100, 16, 1);
        _ringBuffer = new Queue<byte>(RingBufferSize);

        _waveIn = new WaveInEvent
        {
            DeviceNumber = inputDeviceIndex,
            WaveFormat = _waveFormat,
            BufferMilliseconds = 5
        };

        _wavePlayer = new WaveOutEvent { DeviceNumber = outputDeviceIndex };
        var provider = new RingBufferWaveProvider(_waveFormat, _ringBuffer, _bufferLock);
        _wavePlayer.Init(provider);

        _waveIn.DataAvailable += OnDataAvailable;
        _isRunning = false;
    }

    public void Start()
    {
        _isRunning = true;
        _wavePlayer.Play();
        _waveIn.StartRecording();
        Console.WriteLine("🔄 Loopback aktiv - Ultra Low Latency Mode");
    }

    public void Stop()
    {
        _isRunning = false;
        _waveIn.StopRecording();
        _wavePlayer.Stop();
        Console.WriteLine("✓ Loopback beendet");
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (!_isRunning) return;

        lock (_bufferLock)
        {
            for (int i = 0; i < e.BytesRecorded; i++)
            {
                if (_ringBuffer.Count >= RingBufferSize)
                    _ringBuffer.Dequeue();
                _ringBuffer.Enqueue(e.Buffer[i]);
            }
        }
    }

    public void Dispose()
    {
        Stop();
        _waveIn?.Dispose();
        _wavePlayer?.Dispose();
    }
}

public class RingBufferWaveProvider : IWaveProvider
{
    private readonly Queue<byte> _buffer;
    private readonly object _bufferLock;
    public WaveFormat WaveFormat { get; }

    public RingBufferWaveProvider(WaveFormat waveFormat, Queue<byte> buffer, object bufferLock)
    {
        WaveFormat = waveFormat;
        _buffer = buffer;
        _bufferLock = bufferLock;
    }

    public int Read(byte[] buffer, int offset, int count)
    {
        lock (_bufferLock)
        {
            int bytesRead = 0;
            while (bytesRead < count && _buffer.Count > 0)
            {
                buffer[offset + bytesRead++] = _buffer.Dequeue();
            }
            return bytesRead;
        }
    }
}
