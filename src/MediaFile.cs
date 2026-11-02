namespace mbs;

public sealed class MediaFile
{
    public MediaFile(string path)
    {
        FileFullPath = path;
        Name = Path.GetFileNameWithoutExtension(path);
    }

    public string FileFullPath { get; }
    public string Name { get; set; }
    public bool IsVBR { get; set; }
    public bool AlreadyPlayed { get; set; }
}

public static class FileHelpers
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".ogg", ".wav"
    };

    public static bool IsAudio(string path) => Extensions.Contains(Path.GetExtension(path));

    // Skip inaccessible subdirectories and Windows junctions to avoid cycles.
    public static IEnumerable<string> EnumerateAudioFiles(string root)
    {
        if (File.Exists(root))
        {
            if (IsAudio(root)) yield return root;
            yield break;
        }

        var pending = new Stack<string>();
        pending.Push(root);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (pending.Count != 0)
        {
            var dir = pending.Pop();
            if (!visited.Add(Path.GetFullPath(dir))) continue;
            string[] files;
            string[] subdirs;
            try
            {
                files = Directory.GetFiles(dir);
                subdirs = Directory.GetDirectories(dir);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                continue;
            }
            foreach (var file in files)
                if (IsAudio(file)) yield return file;
            foreach (var subdir in subdirs)
            {
                try
                {
                    if ((File.GetAttributes(subdir) & FileAttributes.ReparsePoint) == 0)
                        pending.Push(subdir);
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
                {
                    // Skip inaccessible entries.
                }
            }
        }
    }
}
