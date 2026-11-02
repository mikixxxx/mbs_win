using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Vorbis;

namespace mbs;

public sealed class AudioPlayer : IDisposable
{
    private WaveOutEvent? output;
    private WaveStream? reader;
    private VolumeSampleProvider? volumeControl;
    private ThreeBandEq? eq;
    private bool waitingForEnd;

    public event EventHandler? PlaybackEnded;
    public bool IsLoaded => output != null && reader != null;
    public bool IsPaused => output?.PlaybackState == PlaybackState.Paused;
    public TimeSpan Position => reader?.CurrentTime ?? TimeSpan.Zero;
    public TimeSpan Length => reader?.TotalTime ?? TimeSpan.Zero;

    public void Play(MediaFile track, float volume, float low, float mid, float high)
    {
        Stop();
        try
        {
            reader = Path.GetExtension(track.FileFullPath).Equals(".ogg", StringComparison.OrdinalIgnoreCase)
                ? new VorbisWaveReader(track.FileFullPath)
                : new AudioFileReader(track.FileFullPath);

            eq = new ThreeBandEq(reader.ToSampleProvider());
            eq.SetBands(low, mid, high);
            volumeControl = new VolumeSampleProvider(eq) { Volume = Math.Clamp(volume, 0f, 1f) };
            output = new WaveOutEvent { DesiredLatency = 150 };
            output.Init(volumeControl);
            output.Play();
            waitingForEnd = true;
            track.AlreadyPlayed = true;
        }
        catch
        {
            Stop();
            throw;
        }
    }

    public void Update()
    {
        // Called from the Terminal.Gui main loop; never raises UI events on audio threads.
        if (!waitingForEnd || output == null) return;
        if (output.PlaybackState != PlaybackState.Stopped) return;
        waitingForEnd = false;
        PlaybackEnded?.Invoke(this, EventArgs.Empty);
    }

    public void Stop()
    {
        waitingForEnd = false;
        output?.Stop();
        output?.Dispose();
        reader?.Dispose();
        output = null;
        reader = null;
        eq = null;
        volumeControl = null;
    }

    public void TogglePause()
    {
        if (output?.PlaybackState == PlaybackState.Playing) output.Pause();
        else if (output?.PlaybackState == PlaybackState.Paused) output.Play();
    }

    public void SeekSeconds(int seconds)
    {
        if (reader == null) return;
        double target = Math.Clamp(reader.CurrentTime.TotalSeconds + seconds, 0.0, reader.TotalTime.TotalSeconds);
        reader.CurrentTime = TimeSpan.FromSeconds(target);
    }

    public void SetVolume(float value)
    {
        if (volumeControl != null) volumeControl.Volume = Math.Clamp(value, 0f, 1f);
    }

    public void SetEq(float low, float mid, float high) => eq?.SetBands(low, mid, high);
    public void Dispose() => Stop();
}
