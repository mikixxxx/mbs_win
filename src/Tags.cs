using ATL;

namespace mbs;

public static class Tags
{
    public static MediaFile Read(string path)
    {
        var file = new MediaFile(path);
        try
        {
            var track = new Track(path);
            if (!string.IsNullOrWhiteSpace(track.Title))
                file.Name = track.Title;
            file.IsVBR = track.IsVBR;
        }
        catch
        {
            // Even missing/corrupt metadata should not block the playlist.
        }
        return file;
    }
}
