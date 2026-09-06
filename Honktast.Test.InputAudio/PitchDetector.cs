public class PitchDetector
{
    private readonly int _bufferSize;
    private readonly int _sampleRate;
    private readonly float[] _window;
    private readonly Queue<float> _sampleQueue;
    private float? _lastFrequency;
    private readonly Queue<float> _frequencyHistory;
    private readonly int _historySize = 8;
    private const float ThresholdYin = 0.35f;

    public PitchDetector(int sampleRate = 44100, int bufferSize = 2048)
    {
        _sampleRate = sampleRate;
        _bufferSize = bufferSize;
        _sampleQueue = new Queue<float>(bufferSize);
        _window = HannWindow(bufferSize);
        _frequencyHistory = new Queue<float>(_historySize);
    }

    public bool AddSamples(float[] samples)
    {
        foreach (var sample in samples)
        {
            _sampleQueue.Enqueue(sample);
            if (_sampleQueue.Count > _bufferSize)
            {
                _sampleQueue.Dequeue();
            }
        }

        bool isFull = _sampleQueue.Count == _bufferSize;
        return isFull;
    }

    public float? DetectPitch()
    {
        try
        {
            if (_sampleQueue.Count < _bufferSize)
                return null;

            float[] buffer = _sampleQueue.ToArray();

            // Anwenden des Fensters
            float[] windowed = new float[_bufferSize];
            for (int i = 0; i < _bufferSize; i++)
            {
                windowed[i] = buffer[i] * _window[i];
            }

            // YIN-Algorithmus für Tonhöhenerkennung
            float? frequency = DetectPitchYIN(windowed);

            if (frequency.HasValue)
            {
                // Stabilisierung mit History
                frequency = StabilizeFrequency(frequency.Value);
            }

            return frequency;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Pitch detection error: {ex.Message}");
            return null;
        }
    }

    private float? DetectPitchYIN(float[] signal)
    {
        int minPeriod = Math.Max(1, _sampleRate / 2000); // Max 2000 Hz (G6+)
        int maxPeriod = Math.Min(signal.Length / 2, _sampleRate / 40); // Min 40 Hz

        // Schritt 1: Autocorrelation
        float[] autocorr = new float[maxPeriod + 1];
        for (int lag = 0; lag <= maxPeriod; lag++)
        {
            float sum = 0;
            for (int i = 0; i < signal.Length - lag; i++)
            {
                sum += signal[i] * signal[i + lag];
            }
            autocorr[lag] = sum;
        }

        // Schritt 2: Differenzfunktion (AMDF - Average Magnitude Difference Function)
        float[] df = new float[maxPeriod + 1];
        float cumulativeSum = 0;

        df[0] = 0;
        for (int lag = 1; lag <= maxPeriod; lag++)
        {
            float sum = 0;
            for (int i = 0; i < signal.Length - lag; i++)
            {
                float diff = signal[i] - signal[i + lag];
                sum += diff * diff;
            }
            df[lag] = sum;
            cumulativeSum += sum;

            // Normalisierte Differenzfunktion (YIN normalization)
            if (cumulativeSum > 0)
            {
                df[lag] = df[lag] * lag / cumulativeSum;
            }
        }

        // Schritt 3: Finde absolutes Minimum
        float minValue = float.MaxValue;
        int minLag = minPeriod;

        for (int lag = minPeriod; lag <= maxPeriod; lag++)
        {
            if (df[lag] < minValue)
            {
                minValue = df[lag];
                minLag = lag;
            }
        }

        // Schritt 4: Prüfe ob unter Schwelle
        if (minValue > ThresholdYin)
            return null;

        // Schritt 5: Verfeinere mit Parabel-Interpolation
        float refinedLag = minLag;
        if (minLag > minPeriod && minLag < maxPeriod)
        {
            float y1 = df[minLag - 1];
            float y2 = df[minLag];
            float y3 = df[minLag + 1];

            float a = (y3 - 2 * y2 + y1) / 2;
            if (Math.Abs(a) > 0.0001f)
            {
                refinedLag = minLag + (y1 - y3) / (4 * a);
            }
        }

        // Konvertiere Lag zu Frequenz
        float frequency = _sampleRate / refinedLag;

        // Gültigkeit prüfen (Casio SA-76: C3-G6 = 262-1568 Hz, aber 40-2000 Hz erlaubt)
        if (frequency < 40 || frequency > 2000)
            return null;

        return frequency;
    }

    private float? StabilizeFrequency(float frequency)
    {
        // Wenn wir eine letzte Frequenz haben, prüfe ob diese ähnlich ist
        if (_lastFrequency.HasValue)
        {
            float diff = Math.Abs(frequency - _lastFrequency.Value);
            float percentDiff = diff / _lastFrequency.Value * 100;

            // Wenn die Differenz zu groß ist, starte neu mit neuer Frequenz
            if (percentDiff > 25)
            {
                _frequencyHistory.Clear();
                _frequencyHistory.Enqueue(frequency);
                _lastFrequency = frequency;
                return frequency;
            }
        }

        // Füge zur History hinzu
        _frequencyHistory.Enqueue(frequency);
        if (_frequencyHistory.Count > _historySize)
        {
            _frequencyHistory.Dequeue();
        }

        // Berechne Durchschnitt der letzten Frequenzen
        float average = 0;
        foreach (var f in _frequencyHistory)
        {
            average += f;
        }
        average /= _frequencyHistory.Count;

        _lastFrequency = average;
        return average;
    }

    private float[] HannWindow(int size)
    {
        var window = new float[size];
        for (int i = 0; i < size; i++)
        {
            window[i] = (float)(0.5 * (1 - Math.Cos(2 * Math.PI * i / (size - 1))));
        }
        return window;
    }
}
