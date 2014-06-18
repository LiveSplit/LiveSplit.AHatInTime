using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using LiveSplit.Web;
using LiveSplit.Web.Share;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace LiveSplit.AHatInTime
{
    class AHatInTimeComponent : IComponent
    {
        public AHatInTimeSettings Settings { get; set; }

        protected TimerPhase Phase { get; set; }
        public TimeSpan? RemovedLoadTime { get; set; }
        public int LastLogIndex { get; set; }

        public string ComponentName
        {
            get { return "A Hat In Time - Time Without Loads"; }
        }

        public IDictionary<string, Action> ContextMenuControls { get; protected set; }

        protected InfoTimeComponent InternalComponent { get; set; }

        public String LogPath
        {
            get
            {
                return Path.Combine(
                    Path.GetDirectoryName(Settings.Path),
                    @"..\..\HatinTimeGame\Logs\Launch.log");
            }
        }

        protected System.Timers.Timer RefreshTimer { get; set; }

        public AHatInTimeComponent()
        {
            Settings = new AHatInTimeSettings();
            InternalComponent = new InfoTimeComponent(null, null, new RegularTimeFormatter(TimeAccuracy.Hundredths));

            ContextMenuControls = new Dictionary<String, Action>();
            RemovedLoadTime = TimeSpan.Zero;
            RefreshTimer = new System.Timers.Timer(5000);
            RefreshTimer.Elapsed += RefreshTimer_Elapsed;
        }

        void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ParseNewLines(ReadAllLines());
        }

        private void ParseLine(String line)
        {
            try
            {
                if (Phase == TimerPhase.Ended || Phase == TimerPhase.NotRunning)
                    return;

                if (line.StartsWith(" Log: ########### Finished loading level: "))
                {
                    var cutOff = line.Substring(" Log: ########### Finished loading level: ".Length);
                    var timeStr = cutOff.Substring(0, cutOff.IndexOf(" seconds"));
                    double seconds;
                    if (Double.TryParse(timeStr, out seconds))
                    {
                        RemovedLoadTime += TimeSpan.FromSeconds(seconds);
#if DEBUG
                        System.Diagnostics.Debug.WriteLine(String.Format("Removed {0} seconds of load time", seconds));
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        Object newLinesLock = new Exception("sf");

        private void ParseNewLines(IEnumerable<String> lines)
        {
            try
            {
                lock (newLinesLock)
                {
                    var skipCount = LastLogIndex;
                    LastLogIndex = lines.Count();
                    foreach (var line in lines.Skip(skipCount))
                    {
                        //System.Diagnostics.Debug.Write(line);
                        try
                        {
                            if (line.StartsWith("["))
                            {
                                var indexStr = line.Substring(1, line.IndexOf(']') - 1);
                                var lineInfo = line.Substring(line.IndexOf("]") + 1);
                                ParseLine(lineInfo);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private IEnumerable<String> ReadAllLines()
        {
            try
            {
                using (var stream = File.Open(LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var reader = new StreamReader(stream);
                    return reader.ReadToEnd().Split('\n');
                }
            } 
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return new String[0];
        }

        //Fuck, this doesn't even work in 1.2 >.>
        void OnStart()
        {
            if (!File.Exists(LogPath))
            {
                MessageBox.Show("You need to select the path to the game in the settings of the A Hat In Time Component.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LastLogIndex = 0;
            var path = Path.GetDirectoryName(LogPath);
            var watcher = new FileSystemWatcher(path) { EnableRaisingEvents = true };
            watcher.Changed += watcher_Changed;
            watcher.NotifyFilter |= NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size;
            ParseNewLines(ReadAllLines());
            RemovedLoadTime = TimeSpan.Zero;
            RefreshTimer.Enabled = true;
        }

        void OnReset()
        {
            RemovedLoadTime = TimeSpan.Zero;
            RefreshTimer.Enabled = false;
        }

        void OnRunEnd()
        {
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (Phase == TimerPhase.Ended || Phase == TimerPhase.NotRunning)
            {
                ((FileSystemWatcher)sender).Dispose();
                return;
            }

            if (e.Name == "Launch.log")
            {
                ParseNewLines(ReadAllLines());
            }
        }

        private void PrepareDraw(LiveSplitState state)
        {
            var newPhase = state.CurrentPhase;
            if (Phase != newPhase)
            {
                if (newPhase == TimerPhase.Running && Phase == TimerPhase.NotRunning)
                {
                    OnStart();
                }
                else if (newPhase == TimerPhase.NotRunning)
                {
                    OnReset();
                }
                else if (newPhase == TimerPhase.Ended)
                {
                    OnRunEnd();
                }
            }
            Phase = newPhase;

            InternalComponent.NameLabel.HasShadow 
                = InternalComponent.ValueLabel.HasShadow
                = state.LayoutSettings.DropShadows;

            InternalComponent.NameLabel.Text = "Time Without Loads";
            InternalComponent.NameLabel.ForeColor = state.LayoutSettings.TextColor;
            InternalComponent.ValueLabel.ForeColor = state.LayoutSettings.TextColor;
            InternalComponent.ValueLabel.Font = InternalComponent.NameLabel.Font;

            InternalComponent.TimeValue = state.CurrentTime - RemovedLoadTime;
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width)
        {
            PrepareDraw(state);
            InternalComponent.DrawVertical(g, state, width);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height)
        {
            PrepareDraw(state);
            InternalComponent.DrawHorizontal(g, state, height);
        }

        public float VerticalHeight
        {
            get { return InternalComponent.VerticalHeight; }
        }

        public float MinimumWidth
        {
            get { return 20; }
        }

        public float HorizontalWidth
        {
            get { return InternalComponent.HorizontalWidth; }
        }

        public float MinimumHeight
        {
            get { return InternalComponent.MinimumHeight; }
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public System.Windows.Forms.Control GetSettingsControl(UI.LayoutMode mode)
        {
            return Settings;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            Settings.SetSettings(settings);
        }
    }
}
