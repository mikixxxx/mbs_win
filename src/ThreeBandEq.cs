using System.Threading;
using NAudio.Dsp;
using NAudio.Wave;

namespace mbs;

public enum EqBand { Low, Mid, High }

// Audio thread reads a stable filter bank; UI thread swaps it atomically.
public sealed class ThreeBandEq : ISampleProvider
{
    private readonly ISampleProvider source;
    private BiQuadFilter[][] filters;

    public WaveFormat WaveFormat => source.WaveFormat;

    public ThreeBandEq(ISampleProvider input)
    {
        source = input;
        filters = MakeFilters(0, 0, 0);
    }

    public void SetBands(float low, float mid, float high)
    {
        // 0.5 on the UI is neutral (0 dB), 0 is -12 dB, 1 is +12 dB.
        var bank = MakeFilters(ToDb(low), ToDb(mid), ToDb(high));
        Interlocked.Exchange(ref filters, bank);
    }

    private static float ToDb(float slider) => (Math.Clamp(slider, 0f, 1f) - 0.5f) * 24f;

    private BiQuadFilter[][] MakeFilters(float lowDb, float midDb, float highDb)
    {
        int channels = WaveFormat.Channels;
        float sampleRate = WaveFormat.SampleRate;
        var result = new BiQuadFilter[channels][];
        for (int channel = 0; channel < channels; channel++)
        {
            result[channel] = new[]
            {
                BiQuadFilter.LowShelf(sampleRate, Math.Min(200f, sampleRate * 0.10f), 1f, lowDb),
                BiQuadFilter.PeakingEQ(sampleRate, Math.Min(1200f, sampleRate * 0.20f), 0.8f, midDb),
                BiQuadFilter.HighShelf(sampleRate, Math.Min(6000f, sampleRate * 0.35f), 1f, highDb)
            };
        }
        return result;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int read = source.Read(buffer, offset, count);
        var bank = Volatile.Read(ref filters);
        int channels = bank.Length;
        for (int i = 0; i < read; i++)
        {
            float sample = buffer[offset + i];
            foreach (var filter in bank[i % channels])
                sample = filter.Transform(sample);
            buffer[offset + i] = sample;
        }
        return read;
    }
}
