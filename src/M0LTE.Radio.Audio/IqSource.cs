namespace M0LTE.Radio.Audio;

/// <summary>
/// A blocking, source-paced source of wideband complex baseband (IQ) samples: the receive side of
/// an SDR, an IQ sound card, a network IQ stream or a recording. A consumer (for example a front
/// end that digitally down-converts several narrowband channels from one wideband stream) pulls
/// blocks from it the way it pulls audio from an <see cref="IAudioInput"/>.
/// </summary>
/// <remarks>
/// <para><b>Sample format.</b> Interleaved single-precision <c>I, Q, I, Q, ...</c>, host-endian,
/// normalised so full scale is -1..1 on each axis. <see cref="Read"/> returns a count of
/// <b>floats</b>, which is always even (whole I,Q pairs), and 0 only when the source is
/// closing.</para>
/// <para><b>Describing the block just read.</b> <see cref="SampleRate"/>,
/// <see cref="CentreFrequencyHz"/>, <see cref="TuningGeneration"/> and <see cref="SamplesLost"/>
/// describe the samples most recently returned by <see cref="Read"/> (before the first read, the
/// samples the first read will return). A source changes them only inside <see cref="Read"/>, on
/// the reader's thread, so a consumer that reads them after each <see cref="Read"/> sees values
/// that match the block it holds. This is what lets a source whose tuning or rate is changed
/// under it (an rtl_tcp-compatible server that switches profile without telling the client, a
/// radio retuned by another client) and a source that loses samples (an overrun, a dropped
/// network packet) report it without a callback thread.</para>
/// <para><b>Retuning.</b> When the centre frequency or the sample rate changes, the source ends
/// the block at the last sample of the old tuning, and on the read that returns the first sample
/// of the new tuning it updates <see cref="SampleRate"/> and <see cref="CentreFrequencyHz"/> and
/// increments <see cref="TuningGeneration"/>. A block never spans a retune.</para>
/// <para><b>Lost samples.</b> When samples are lost, the source ends the block at the last sample
/// before the gap, and on the read that returns the first sample after it, it adds the number of
/// complex samples lost to <see cref="SamplesLost"/>. A block never spans a gap, and an increase
/// always means the gap lies immediately before the block just returned, so a consumer can mark
/// the gap instead of splicing across it.</para>
/// </remarks>
public interface IIqSource
{
    /// <summary>Complex sample rate in Hz (I,Q pairs per second) of the samples most recently
    /// returned by <see cref="Read"/>. Changes only together with
    /// <see cref="TuningGeneration"/>.</summary>
    int SampleRate { get; }

    /// <summary>The RF frequency in Hz that baseband 0 Hz corresponds to, for the samples most
    /// recently returned by <see cref="Read"/>. Channel offsets are relative to this. It is 0 when
    /// the absolute frequency is unknown or irrelevant (a synthetic test signal, a recording with
    /// no metadata). Changes only together with <see cref="TuningGeneration"/>.</summary>
    double CentreFrequencyHz { get; }

    /// <summary>Starts at 0 and increases by at least 1 each time <see cref="SampleRate"/> or
    /// <see cref="CentreFrequencyHz"/> changes. A consumer compares it after each
    /// <see cref="Read"/> with the value it last saw; a difference means the block just returned
    /// is the first at the new tuning. A source whose tuning never changes returns 0
    /// always.</summary>
    long TuningGeneration { get; }

    /// <summary>The running total of complex samples (I,Q pairs) the source knows it has lost:
    /// overruns, ring overflows, dropped packets. Starts at 0 and never decreases. An increase seen
    /// after a <see cref="Read"/> means that many samples are missing immediately before the block
    /// just returned. Where a source knows only that something was lost and not exactly how much,
    /// it adds its best estimate, never less than 1. A source that cannot lose samples returns 0
    /// always.</summary>
    long SamplesLost { get; }

    /// <summary>Reads the next block of interleaved <c>I, Q</c> samples into
    /// <paramref name="interleaved"/>, filling as many whole pairs as fit and are available.
    /// Blocks until at least one pair is available. <paramref name="interleaved"/> must hold at
    /// least one pair (two floats). Returns the number of <b>float</b> elements
    /// written (always even), or 0 only when the source is closing.</summary>
    int Read(Span<float> interleaved);
}
