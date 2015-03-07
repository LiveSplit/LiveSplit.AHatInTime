using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using LiveSplit.Web;
using LiveSplit.Web.Share;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace LiveSplit.AHatInTime
{
    class AHatInTimeComponent : LogicComponent
    {
        public AHatInTimeSettings Settings { get; set; }

        protected LiveSplitState State { get; set; }
        protected Process Game { get; set; }

        public override string ComponentName
        {
            get { return "A Hat In Time Auto Splitter"; }
        }

        public AHatInTimeComponent(LiveSplitState state)
        {
            Settings = new AHatInTimeSettings();

            this.State = state;
            ContextMenuControls.Add("Start Game", StartGame);
            ContextMenuControls.Add("Delete Save", DeleteSave);
        }

        private void DeleteSave()
        {
            if (String.IsNullOrEmpty(Settings.Path))
            {
                MessageBox.Show(State.Form, "Please set the path of the game in the settings.", "Game Path Not Set", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var savePath = Path.Combine(
                Path.GetDirectoryName(Settings.Path),
                "..", "..", "HatinTimeGame",
                "SaveData", "slot1beta.hat");
            File.Delete(savePath);
        }

        public void StartGame()
        {
            if (String.IsNullOrEmpty(Settings.Path))
            {
                MessageBox.Show(State.Form, "Please set the path of the game in the settings.", "Game Path Not Set", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var startInfo = new ProcessStartInfo(Settings.Path)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            Game = new Process
            {
                StartInfo = startInfo
            };
            var started = Game.Start();
            if (started)
            {
                Game.OutputDataReceived += Game_OutputDataReceived;
                Game.BeginOutputReadLine();
            }
            else
            {
                MessageBox.Show(State.Form, "The game could not be started.", "Game Not Started", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        void Game_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var line = e.Data;
            ParseLine(line);
        }

        private void ParseLine(String line)
        {
            try
            {
                if (State.CurrentPhase == TimerPhase.Ended || State.CurrentPhase == TimerPhase.NotRunning)
                    return;

                if (line.StartsWith(" Log: ########### Finished loading level: "))
                {
                    var cutOff = line.Substring(" Log: ########### Finished loading level: ".Length);
                    var timeStr = cutOff.Substring(0, cutOff.IndexOf(" seconds"));
                    double seconds;
                    if (Double.TryParse(timeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out seconds))
                    {
                        State.LoadingTimes += TimeSpan.FromSeconds(seconds);
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
