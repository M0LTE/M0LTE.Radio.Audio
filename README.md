# M0LTE.Radio.Audio

Minimal, dependency-free **audio I/O + PTT abstractions** for driving a radio or sound device
from a .NET modem or DSP. It's the shared seam of the `M0LTE.*` radio packages — implement
these three interfaces and any sound card, SDR audio stream, or file plugs into a modem the
same way.

| Type | Contract |
| --- | --- |
| `IAudioInput` | `int SampleRate`, `int Read(Span<float>)` — blocking, source-paced capture of normalised floats (−1..1). |
| `IAudioOutput` | `int SampleRate`, `void Write(ReadOnlySpan<float>)`, `void Drain()` — blocking, sink-paced playback. |
| `IPttControl` | `void Key()`, `void Unkey()` — transmitter keying. |
| `NullPtt` | A no-op `IPttControl` for VOX interfaces and tests. |

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
