using mbs;
using Terminal.Gui;

internal static class Program
{
    private static void Main(string[] args)
    {
        Application.Init();
        try
        {
            RunPlayer();
        }
        finally
        {
            Application.Shutdown();
        }
    }

    private static void RunPlayer()
    {
        var top = Application.Top;
        Application.Driver.SetCursorVisibility(CursorVisibility.Invisible);

        using var player = new AudioPlayer();
        var playlist = new MyList<MediaFile>();
        var random = new Random();
        var state = PlayerState.Stopped;
        MediaFile? currentTrack = null;
        int nextIndex = 0;

        string? browserDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        if (string.IsNullOrWhiteSpace(browserDirectory) || !Directory.Exists(browserDirectory))
            browserDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrWhiteSpace(browserDirectory) || !Directory.Exists(browserDirectory))
            browserDirectory = null; // Show drive list.
        List<string?> browserPaths = new();

        // Three-pane layout, retained from the original Terminal.Gui design.
        var browserPane = new FrameView("Files")
        {
            X = 0, Y = 0, Width = Dim.Percent(34), Height = Dim.Fill() - 1
        };
        var browserView = new ListView()
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(),
            AllowsMarking = false, AllowsMultipleSelection = false
        };
        browserPane.Add(browserView);

        var playlistPane = new FrameView("Playlist")
        {
            X = Pos.Right(browserPane), Y = 0,
            Width = Dim.Percent(30), Height = Dim.Fill() - 1
        };
        var playlistView = new ListView()
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(),
            AllowsMarking = false, AllowsMultipleSelection = false,
            CanFocus = true
        };
        playlistPane.Add(playlistView);

        var playerPane = new FrameView("Player / EQ")
        {
            X = Pos.Right(playlistPane), Y = 0,
            Width = Dim.Fill(), Height = Dim.Fill() - 1
        };
        var songName = new Label("Stopped") { X = Pos.Center(), Y = 1 };
        var positionBar = new ProgressBar()
        {
            X = Pos.Center(), Y = Pos.Bottom(songName) + 1, Width = Dim.Percent(85),
            ProgressBarStyle = ProgressBarStyle.Continuous,
            ProgressBarFormat = ProgressBarFormat.Simple,
            Fraction = 0f
        };
        var songTime = new Label("00:00 / 00:00")
        {
            X = Pos.Center(), Y = Pos.Bottom(positionBar) + 1
        };
        var shuffle = new CheckBox("Shuffle", false) { X = 1, Y = Pos.Bottom(songTime) + 2 };
        var repeat = new CheckBox("Repeat list", false) { X = 1, Y = Pos.Bottom(shuffle) };

        var volumeBar = new ProgressBar()
        {
            X = Pos.Center(), Y = Pos.Bottom(playerPane) - 4, Width = Dim.Percent(70),
            ProgressBarStyle = ProgressBarStyle.Continuous,
            ProgressBarFormat = ProgressBarFormat.SimplePlusPercentage,
            Fraction = 0.8f
        };
        var volumeLabel = new Label("Volume")
        {
            X = Pos.Center(), Y = Pos.Top(volumeBar) - 1
        };
        var highBar = new ProgressBar()
        {
            X = Pos.Center(), Y = Pos.Top(volumeLabel) - 3, Width = Dim.Percent(70),
            ProgressBarStyle = ProgressBarStyle.Continuous,
            ProgressBarFormat = ProgressBarFormat.SimplePlusPercentage,
            Fraction = 0.5f
        };
        var highLabel = new Label("Treble") { X = Pos.Center(), Y = Pos.Top(highBar) - 1 };
        var midBar = new ProgressBar()
        {
            X = Pos.Center(), Y = Pos.Top(highLabel) - 3, Width = Dim.Percent(70),
            ProgressBarStyle = ProgressBarStyle.Continuous,
            ProgressBarFormat = ProgressBarFormat.SimplePlusPercentage,
            Fraction = 0.5f
        };
        var midLabel = new Label("Mid") { X = Pos.Center(), Y = Pos.Top(midBar) - 1 };
        var lowBar = new ProgressBar()
        {
            X = Pos.Center(), Y = Pos.Top(midLabel) - 3, Width = Dim.Percent(70),
            ProgressBarStyle = ProgressBarStyle.Continuous,
            ProgressBarFormat = ProgressBarFormat.SimplePlusPercentage,
            Fraction = 0.5f
        };
        var lowLabel = new Label("Bass") { X = Pos.Center(), Y = Pos.Top(lowBar) - 1 };

        playerPane.Add(songName, positionBar, songTime, shuffle, repeat,
            volumeBar, volumeLabel, highBar, highLabel, midBar, midLabel, lowBar, lowLabel);

        var help = new Label("Enter: open  Space: add/play  D: delete  N: next  P: pause  S: stop  C/V: seek  U/J: volume  R/F T/G Z/H: EQ  Esc: quit")
        {
            X = 0, Y = Pos.Bottom(browserPane), Width = Dim.Fill()
        };
        top.Add(browserPane, playlistPane, playerPane, help);

        static string DisplayName(string path)
        {
            string name = Path.GetFileName(Path.TrimEndingDirectorySeparator(path));
            return string.IsNullOrEmpty(name) ? path : name;
        }

        void RefreshBrowser()
        {
            try
            {
                var paths = new List<string?>();
                if (browserDirectory is null)
                {
                    paths.AddRange(Directory.GetLogicalDrives());
                }
                else
                {
                    paths.Add(null); // Parent directory / drives.
                    paths.AddRange(Directory.EnumerateFileSystemEntries(browserDirectory)
                        .OrderBy(path => File.Exists(path))
                        .ThenBy(path => DisplayName(path), StringComparer.OrdinalIgnoreCase));
                }
                browserPaths = paths;
                browserView.SetSource(paths.Select(path => path is null ? ".." : DisplayName(path)).ToList());
                if (paths.Count > 0) browserView.SelectedItem = 0;
                browserPane.Title = browserDirectory ?? "Drives";
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                MessageBox.ErrorQuery("Cannot open folder", ex.Message, "OK");
            }
        }

        void UpdatePlaylist()
        {
            int selection = playlistView.SelectedItem;
            playlistView.SetSource(playlist.Select(track => track.Name.LimitLength(35)).ToList());
            if (playlist.Count > 0)
                playlistView.SelectedItem = Math.Clamp(selection, 0, playlist.Count - 1);
        }

        void AddFiles(string path)
        {
            try
            {
                var tracks = FileHelpers.EnumerateAudioFiles(path).Select(Tags.Read).ToList();
                playlist.AddRange(tracks);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                MessageBox.ErrorQuery("Cannot add files", ex.Message, "OK");
            }
        }

        void OpenSelectedEntry()
        {
            int index = browserView.SelectedItem;
            if (index < 0 || index >= browserPaths.Count) return;
            var path = browserPaths[index];
            if (path is null)
            {
                browserDirectory = browserDirectory is null ? null : Directory.GetParent(browserDirectory)?.FullName;
                RefreshBrowser();
            }
            else if (Directory.Exists(path))
            {
                browserDirectory = path;
                RefreshBrowser();
            }
            else if (FileHelpers.IsAudio(path))
            {
                AddFiles(path);
            }
        }

        void StopPlayback()
        {
            state = PlayerState.Stopped;
            currentTrack = null;
            player.Stop();
            songName.Text = "Stopped";
            songTime.Text = "00:00 / 00:00";
            positionBar.Fraction = 0f;
        }

        void PlayAt(int index)
        {
            if (index < 0 || index >= playlist.Count) return;
            try
            {
                var track = playlist[index];
                player.Play(track, volumeBar.Fraction, lowBar.Fraction, midBar.Fraction, highBar.Fraction);
                currentTrack = track;
                nextIndex = index + 1;
                state = PlayerState.Playing;
                songName.Text = track.Name.LimitLength(65);
                playlistView.SelectedItem = index;
            }
            catch (Exception ex)
            {
                StopPlayback();
                MessageBox.ErrorQuery("Playback error", ex.Message, "OK");
            }
        }

        void PlayRandom()
        {
            var candidates = playlist.Select((track, index) => (track, index))
                .Where(item => !item.track.AlreadyPlayed).ToList();
            if (candidates.Count == 0 && repeat.Checked)
            {
                foreach (var track in playlist) track.AlreadyPlayed = false;
                candidates = playlist.Select((track, index) => (track, index)).ToList();
            }
            if (candidates.Count == 0)
            {
                StopPlayback();
                return;
            }
            PlayAt(candidates[random.Next(candidates.Count)].index);
        }

        void PlayNext()
        {
            if (playlist.Count == 0)
            {
                StopPlayback();
                return;
            }
            if (nextIndex >= playlist.Count)
            {
                if (!repeat.Checked)
                {
                    StopPlayback();
                    return;
                }
                nextIndex = 0;
            }
            PlayAt(nextIndex);
        }

        void ChangeVolume(float delta)
        {
            volumeBar.Fraction = Math.Clamp(volumeBar.Fraction + delta, 0f, 1f);
            player.SetVolume(volumeBar.Fraction);
        }

        void ChangeEq(ProgressBar bar, float delta)
        {
            bar.Fraction = Math.Clamp(bar.Fraction + delta, 0f, 1f);
            player.SetEq(lowBar.Fraction, midBar.Fraction, highBar.Fraction);
        }

        void DeleteSelected()
        {
            int index = playlistView.SelectedItem;
            if (index < 0 || index >= playlist.Count) return;
            var track = playlist[index];
            if (ReferenceEquals(track, currentTrack)) StopPlayback();
            if (index < nextIndex) nextIndex--;
            playlist.Remove(track);
            if (playlist.Count == 0) nextIndex = 0;
        }

        browserView.OpenSelectedItem += ev => OpenSelectedEntry();
        browserView.KeyUp += ev =>
        {
            if (ev.KeyEvent.Key != Key.Space) return;
            int index = browserView.SelectedItem;
            if (index >= 0 && index < browserPaths.Count && browserPaths[index] is string path)
                AddFiles(path);
        };
        playlistView.KeyUp += ev =>
        {
            switch (ev.KeyEvent.Key)
            {
                case Key.Space: PlayAt(playlistView.SelectedItem); break;
                case Key.d: DeleteSelected(); break;
            }
        };
        playlist.OnAdd += (_, _) => UpdatePlaylist();
        playlist.OnRemove += (_, _) => UpdatePlaylist();
        player.PlaybackEnded += (_, _) =>
        {
            if (state == PlayerState.Playing)
            {
                if (shuffle.Checked) PlayRandom();
                else PlayNext();
            }
        };
        top.KeyUp += ev =>
        {
            switch (ev.KeyEvent.Key)
            {
                case Key.Esc: Application.RequestStop(); break;
                case Key.s: StopPlayback(); break;
                case Key.n:
                    if (playlist.Count > 0)
                    {
                        if (shuffle.Checked) PlayRandom();
                        else PlayNext();
                    }
                    break;
                case Key.p: player.TogglePause(); break;
                case Key.c: player.SeekSeconds(-10); break;
                case Key.v: player.SeekSeconds(+10); break;
                case Key.u: ChangeVolume(+0.02f); break;
                case Key.j: ChangeVolume(-0.02f); break;
                case Key.r: ChangeEq(lowBar, +0.02f); break;
                case Key.f: ChangeEq(lowBar, -0.02f); break;
                case Key.t: ChangeEq(midBar, +0.02f); break;
                case Key.g: ChangeEq(midBar, -0.02f); break;
                case Key.z: ChangeEq(highBar, +0.02f); break;
                case Key.h: ChangeEq(highBar, -0.02f); break;
            }
        };

        Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(100), _ =>
        {
            player.Update();
            if (player.IsLoaded)
            {
                var position = player.Position;
                var length = player.Length;
                songTime.Text = position.ToString(@"mm\:ss") + " / " + length.ToString(@"mm\:ss");
                positionBar.Fraction = length.TotalMilliseconds > 0
                    ? Math.Clamp((float)(position.TotalMilliseconds / length.TotalMilliseconds), 0f, 1f)
                    : 0f;
            }
            return true;
        });

        RefreshBrowser();
        Application.Run(top);
    }
}
