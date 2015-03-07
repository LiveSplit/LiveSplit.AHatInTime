using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using LiveSplit.Web;
using LiveSplit.Web.Share;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace LiveSplit.AHatInTime
{
    class AHatInTimeComponent : LogicComponent
    {
        public AHatInTimeSettings Settings { get; set; }

        public int LastLogIndex { get; set; }

        LiveSplitState state;

        public override string ComponentName
        {
            get { return "A Hat In Time - Time Without Loads"; }
        }

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

        public AHatInTimeComponent(LiveSplitState state)
        {
            Settings = new AHatInTimeSettings();

            LastLogIndex = 0; //TODO Should reset when the game restarts
            RefreshTimer = new System.Timers.Timer(5000);
            RefreshTimer.Elapsed += RefreshTimer_Elapsed;

            this.state = state;

            state.OnStart += (s, e) => OnStart();
            state.OnReset += (s, e) => OnReset();
        }

        void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ParseNewLines(ReadAllLines());
        }

        private void ParseLine(String line)
        {
            try
            {
                if (state.CurrentPhase == TimerPhase.Ended || state.CurrentPhase == TimerPhase.NotRunning)
                    return;

                if (line.StartsWith(" Log: ########### Finished loading level: "))
                {
                    var cutOff = line.Substring(" Log: ########### Finished loading level: ".Length);
                    var timeStr = cutOff.Substring(0, cutOff.IndexOf(" seconds"));
                    double seconds;
                    if (Double.TryParse(timeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out seconds))
                    {
                        state.LoadingTimes += TimeSpan.FromSeconds(seconds);
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

            var path = Path.GetDirectoryName(LogPath);
            var watcher = new FileSystemWatcher(path) { EnableRaisingEvents = true };
            watcher.Changed += watcher_Changed;
            watcher.NotifyFilter |= NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size;
            ParseNewLines(ReadAllLines());
            RefreshTimer.Enabled = true;
        }

        void OnReset()
        {
            RefreshTimer.Enabled = false;
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (state.CurrentPhase == TimerPhase.Ended || state.CurrentPhase == TimerPhase.NotRunning)
            {
                ((FileSystemWatcher)sender).Dispose();
                return;
            }

            if (e.Name == "Launch.log")
            {
                ParseNewLines(ReadAllLines());
            }
        }

        public override System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public override System.Windows.Forms.Control GetSettingsControl(UI.LayoutMode mode)
        {
            return Settings;
        }

        public override void SetSettings(System.Xml.XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public override void Update(UI.IInvalidator invalidator, LiveSplitState state, float width, float height, UI.LayoutMode mode)
        {
        }

        public override void Dispose()
        {
            //TODO Probably dispose the filesystem watcher
        }
    }
}
