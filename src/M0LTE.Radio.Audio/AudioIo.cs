namespace M0LTE.Radio.Audio;

/// <summary>
/// A blocking, source-paced audio input — the capture side of a radio or sound device.
/// A modem/DSP reads normalised float samples (−1..1) from it at <see cref="SampleRate"/>,
/// so a sound card, an SDR's audio stream, or a file can all be driven the same way.
/// </summary>
public interface IAudioInput
{
    /// <summary>Source sample rate (Hz) of the samples returned by <see cref="Read"/>.</summary>
    int SampleRate { get; }

    /// <summary>Reads up to <paramref name="destination"/>.Length samples as normalised
    /// floats (−1..1). Blocks until at least one sample is available; returns the count
    /// written (0 only when the source is closing).</summary>
    int Read(Span<float> destination);
}

/// <summary>
/// A blocking, sink-paced audio output — the playback side of a radio or sound device.
/// A modem/DSP writes normalised float samples (−1..1) at <see cref="SampleRate"/>.
/// </summary>
public interface IAudioOutput
{
    /// <summary>Sink sample rate (Hz); the samples written must be at this rate.</summary>
    int SampleRate { get; }

    /// <summary>Writes samples; blocks while the sink consumes them.</summary>
    void Write(ReadOnlySpan<float> samples);

    /// <summary>Blocks until everything written has actually left the sink — the
    /// sample-domain part of releasing the transmitter.</summary>
    void Drain();
}

/// <summary>Keys and unkeys the transmitter (push-to-talk). Implementations range from a
/// serial RTS/DTR line or CM108 HID pin to an SDR API command, or a no-op for VOX.</summary>
public interface IPttControl
{
    /// <summary>Asserts PTT.</summary>
    void Key();

    /// <summary>Releases PTT.</summary>
    void Unkey();
}

/// <summary>No-op PTT for VOX-operated interfaces and tests.</summary>
public sealed class NullPtt : IPttControl
{
    /// <inheritdoc />
    public void Key()
    {
    }

    /// <inheritdoc />
    public void Unkey()
    {
    }
}
