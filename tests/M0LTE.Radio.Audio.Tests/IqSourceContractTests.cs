namespace M0LTE.Radio.Audio.Tests;

/// <summary>
/// Executable documentation of the <see cref="IIqSource"/> contract: a scripted source that
/// retunes and loses samples, and the per-block check a consumer makes to see both.
/// </summary>
public class IqSourceContractTests
{
    [Fact]
    public void A_consumer_that_checks_after_each_read_sees_a_retune_and_a_gap_at_the_block_they_precede()
    {
        // Three runs of pairs: 4 at 48 kHz on 14.1 MHz, then a retune to 96 kHz on 7.1 MHz
        // (3 pairs), then 5 pairs lost, then 2 more pairs at the same tuning.
        var source = new ScriptedIqSource(
            new Run(4, 48_000, 14_100_000, LostBefore: 0),
            new Run(3, 96_000, 7_100_000, LostBefore: 0),
            new Run(2, 96_000, 7_100_000, LostBefore: 5));

        long generation = source.TuningGeneration;
        long lost = source.SamplesLost;
        var seen = new List<string>();
        float[] block = new float[64];

        int got;
        while ((got = source.Read(block)) > 0)
        {
            got.Should().Be(got & ~1, "the count is floats and always whole pairs");
            string note = $"{got / 2} pairs at {source.SampleRate} Hz on {source.CentreFrequencyHz} Hz";
            if (source.TuningGeneration != generation)
            {
                note += ", retuned";
                generation = source.TuningGeneration;
            }

            if (source.SamplesLost != lost)
            {
                note += $", {source.SamplesLost - lost} lost before it";
                lost = source.SamplesLost;
            }

            seen.Add(note);
        }

        seen.Should().Equal(
            "4 pairs at 48000 Hz on 14100000 Hz",
            "3 pairs at 96000 Hz on 7100000 Hz, retuned",
            "2 pairs at 96000 Hz on 7100000 Hz, 5 lost before it");
    }

    private readonly record struct Run(int Pairs, int SampleRate, double CentreHz, long LostBefore);

    /// <summary>Returns each run as one block, updating its description inside Read as the
    /// contract requires.</summary>
    private sealed class ScriptedIqSource(params Run[] runs) : IIqSource
    {
        private int _next;

        public int SampleRate { get; private set; } = runs[0].SampleRate;

        public double CentreFrequencyHz { get; private set; } = runs[0].CentreHz;

        public long TuningGeneration { get; private set; }

        public long SamplesLost { get; private set; }

        public int Read(Span<float> interleaved)
        {
            if (_next == runs.Length)
            {
                return 0;
            }

            Run run = runs[_next++];
            if (run.SampleRate != SampleRate || run.CentreHz != CentreFrequencyHz)
            {
                SampleRate = run.SampleRate;
                CentreFrequencyHz = run.CentreHz;
                TuningGeneration++;
            }

            SamplesLost += run.LostBefore;
            int floats = run.Pairs * 2;
            interleaved[..floats].Fill(0.5f);
            return floats;
        }
    }
}
