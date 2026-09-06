using NAudio.Wave;

public class AudioCapture : IDisposable
{
    private readonly IWaveIn _waveIn;
    private readonly PitchDetector _pitchDetector;
    private bool _isRunning;
    private readonly int _deviceIndex;
    private readonly Queue<float> _audioBuffer;
    private readonly object _bufferLock = new object();
    private int _sampleCount;
    private readonly SessionRecorder? _recorder;

    public AudioCapture(int deviceIndex = 0, SessionRecorder? recorder = null)
    {
        _deviceIndex = deviceIndex;
        _waveIn = new WaveInEvent
        {
            DeviceNumber = deviceIndex,
            WaveFormat = new WaveFormat(44100, 16, 1),
            BufferMilliseconds = 10
        };

        _pitchDetector = new PitchDetector(44100, 1024);
        _isRunning = false;
        _audioBuffer = new Queue<float>();
        _sampleCount = 0;
        _recorder = recorder;

        _waveIn.DataAvailable += OnDataAvailable;
    }

    public async Task StartAsync()
    {
        _isRunning = true;
        _waveIn.StartRecording();

        Console.WriteLine("🎤 Mikrofon aktiv - Singen Sie eine Note! (Drücken Sie 'q' zum Beenden)");
        Console.WriteLine("Note\tFrequenz\tCents\tSignal");
        Console.WriteLine(new string('-', 50));

        while (_isRunning)
        {
            await Task.Delay(50);

            float[] samples;
            lock (_bufferLock)
            {
                samples = _audioBuffer.ToArray();
                _audioBuffer.Clear();
            }

            if (samples.Length > 0)
            {
                bool detectionReady = false;
                for (int i = 0; i < samples.Length; i++)
                {
                    if (_pitchDetector.AddSamples(new[] { samples[i] }))
                    {
                        detectionReady = true;
                    }
                }

                string signal = GetSignalBar(samples);

                if (detectionReady)
                {
                    var pitch = _pitchDetector.DetectPitch();

                    if (pitch.HasValue && pitch > 0)
                    {
                        string note = NoteConverter.FrequencyToNote(pitch.Value);
                        float cents = NoteConverter.GetCentsDifference(pitch.Value, note);
                        Console.WriteLine($"{note}\t{pitch:F1} Hz\t{cents:+0.00;-0.00}¢\t{signal}");
                        _recorder?.RecordNote(note, pitch.Value);
                    }
                    else
                    {
                        Console.WriteLine($"--- \t--- Hz\t--- ¢\t{signal}\t(kein Signal erkannt)");
                    }
                }
            }

            if (Console.KeyAvailable && Console.ReadKey(true).KeyChar == 'q')
            {
                _isRunning = false;
            }
        }

        Stop();
    }

    private string GetSignalBar(float[] samples)
    {
        float rms = 0;
        foreach (var sample in samples)
        {
            rms += sample * sample;
        }
        rms = MathF.Sqrt(rms / samples.Length);

        int bars = (int)(rms * 20);
        bars = Math.Min(10, Math.Max(0, bars));
        return new string('█', bars) + new string('░', 10 - bars);
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (!_isRunning) return;

        lock (_bufferLock)
        {
            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                short sample = BitConverter.ToInt16(e.Buffer, i);
                float normalized = sample / 32768f;
                float amplified = normalized * 4f;
                _audioBuffer.Enqueue(amplified);
                _sampleCount++;
            }
        }
    }

    public void Stop()
    {
        _waveIn.StopRecording();
        _recorder?.Close();
        Console.WriteLine("\n✓ Aufnahme beendet");
    }

    public void Dispose()
    {
        _waveIn?.Dispose();
    }
}
