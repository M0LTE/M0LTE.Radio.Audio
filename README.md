# M0LTE.Radio.Audio

Minimal, dependency-free **audio I/O + PTT abstractions** for driving a radio or sound device
from a .NET modem or DSP. It's the shared seam of the `M0LTE.*` radio packages: implement
these interfaces and any sound card, SDR, IQ stream, or file plugs into a modem the same way.

| Type | Contract |
| --- | --- |
| `IAudioInput` | `int SampleRate`, `int Read(Span<float>)`: blocking, source-paced capture of normalised floats (-1..1). |
| `IAudioOutput` | `int SampleRate`, `void Write(ReadOnlySpan<float>)`, `void Drain()`: blocking, sink-paced playback. |
| `IIqSource` | `int SampleRate`, `double CentreFrequencyHz`, `long TuningGeneration`, `long SamplesLost`, `int Read(Span<float>)`: blocking, source-paced capture of interleaved I,Q floats (-1..1), with a retune or a gap reported on the block it precedes. |
| `IPttControl` | `void Key()`, `void Unkey()`: transmitter keying. |
| `NullPtt` | A no-op `IPttControl` for VOX interfaces and tests. |

### The IQ source contract

`IIqSource.Read` returns a count of floats, always even, and 0 only when the source is closing. The four properties describe the block most recently returned, and a source changes them only inside `Read`, so a consumer checks them after each read:

- A retune (centre frequency or sample rate changed under the source) ends a block. The next block carries the new `SampleRate` and `CentreFrequencyHz`, and `TuningGeneration` has gone up.
- A gap (an overrun, a dropped network packet) ends a block too. On the next block, `SamplesLost` has gone up by the number of complex samples missing just before it, or by the source's best estimate when it cannot know exactly.

A source that never retunes and never loses samples returns 0 for both.

- **Targets** `net10.0`. No third-party dependencies.
- Public API is **locked by a build-time test** and the package follows
  [Semantic Versioning](https://semver.org/); see [`docs/versioning.md`](docs/versioning.md).

## Install

```sh
dotnet add package M0LTE.Radio.Audio
```

## Example

```csharp
using M0LTE.Radio.Audio;

// A trivial file-backed capture source at 48 kHz.
public sealed class WavFileInput(float[] samples) : IAudioInput
{
    private int _pos;
    public int SampleRate => 48_000;
    public int Read(Span<float> destination)
    {
        int n = Math.Min(destination.Length, samples.Length - _pos);
        samples.AsSpan(_pos, n).CopyTo(destination);
        _pos += n;
        return n;
    }
}
```

## Licence

AGPL-3.0-or-later (see [`LICENSE`](LICENSE)). These are original interface definitions.
