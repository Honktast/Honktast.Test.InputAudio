public class NoteConverter
{
    private static readonly string[] Notes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    private const float A4_FREQUENCY = 440f;
    private const int A4_SEMITONE = 57;

    public static string FrequencyToNote(float frequency)
    {
        if (frequency <= 0) return "---";

        float semitone = 12 * MathF.Log2(frequency / A4_FREQUENCY) + A4_SEMITONE;
        int roundedSemitone = (int)Math.Round(semitone);

        int octave = (roundedSemitone / 12) - 1;
        int noteIndex = roundedSemitone % 12;
        if (noteIndex < 0) noteIndex += 12;

        return $"{Notes[noteIndex]}{octave}";
    }

    public static float NoteToFrequency(string note)
    {
        if (note.Length < 2) return 0;

        int octave = int.Parse(note[^1].ToString());
        string noteName = note[..^1];

        int noteIndex = Array.IndexOf(Notes, noteName);
        if (noteIndex < 0) return 0;

        int semitone = (octave + 1) * 12 + noteIndex;
        return A4_FREQUENCY * MathF.Pow(2, (semitone - A4_SEMITONE) / 12f);
    }

    public static float GetCentsDifference(float frequency, string note)
    {
        float targetFrequency = NoteToFrequency(note);
        if (targetFrequency <= 0 || frequency <= 0) return 0;

        return 1200 * MathF.Log2(frequency / targetFrequency);
    }
}
