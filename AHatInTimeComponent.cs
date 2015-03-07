using LiveSplit.ASL;
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

        protected Process Game { get; set; }

        protected TimerModel Model { get; set; }
        public ASLState OldState { get; set; }
        public ASLState State { get; set; }

        protected DeepPointer<byte> Hourglasses { get; set; }
        protected DeepPointer<byte> GameTimer { get; set; }

        public override string ComponentName
        {
            get { return "A Hat In Time Auto Splitter"; }
        }

        public AHatInTimeComponent(LiveSplitState state)
        {
            Settings = new AHatInTimeSettings();
            State = new ASLState();
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

        private void Rebuild()
        {
            State.ValueDefinitions.Clear();

            var gameVersion = GameVersion.BETA;

            switch (gameVersion)
            {
                case GameVersion.BETA: RebuildBeta(); break;
                default: Game = null; break;
            }
        }

        private void RebuildBeta()
        {
            State.ValueDefinitions.Add(new ASLValueDefinition()
            {
                Identifier = "hourglasses",
                Pointer = new DeepPointer<byte>(1, Game, "HatinTimeGame.exe", 0x022265F0, 0x4, 0x428, 0x670, 0x40, 0x48)
            });
            State.ValueDefinitions.Add(new ASLValueDefinition()
            {
                Identifier = "gameTime",
                Pointer = new DeepPointer<float>(1, Game, "HatinTimeGame.exe", 0x002A4FF4, 0x0)
            });
        }

        protected void TryConnect()
        {
            if (Game == null || Game.HasExited)
            {
                Game = Process.GetProcessesByName("HatinTimeGame").FirstOrDefault();
                if (Game != null)
                {
                    Rebuild();
                    State.RefreshValues();
                    OldState = State;
                }
            }
        }

        public override void Update(UI.IInvalidator invalidator, LiveSplitState lsState, float width, float height, UI.LayoutMode mode)
        {
            if (Game != null && !Game.HasExited)
            {
                OldState = State.RefreshValues();

                if (lsState.CurrentPhase == TimerPhase.NotRunning)
                {
                    if (Start(lsState, OldState.Data, State.Data))
                    {
                        Model.Start();
                    }
                }
                else if (lsState.CurrentPhase == TimerPhase.Running || lsState.CurrentPhase == TimerPhase.Paused)
                {
                    if (Reset(lsState, OldState.Data, State.Data))
                    {
                        Model.Reset();
                        return;
                    }
                    else if (Split(lsState, OldState.Data, State.Data))
                    {
                        Model.Split();
                    }

                    var isPaused = IsPaused(lsState, OldState.Data, State.Data);
                    if (isPaused != null)
                        lsState.IsGameTimePaused = isPaused;

                    var gameTime = GameTime(lsState, OldState.Data, State.Data);
                    if (gameTime != null)
                        lsState.SetGameTime(gameTime);
                }
            }
            else
            {
                if (Model == null)
                {
                    Model = new TimerModel() { CurrentState = lsState };
                }
                TryConnect();
            }
        }

        public override void Dispose()
        {
            //TODO Probably dispose the filesystem watcher
        }

        public bool Start(LiveSplitState timer, dynamic old, dynamic current)
        {
            return false;
        }

        public bool Split(LiveSplitState timer, dynamic old, dynamic current)
        {
            return current.hourglasses > old.hourglasses;
        }

        public bool Reset(LiveSplitState timer, dynamic old, dynamic current)
        {
            return false;
        }

        public bool IsPaused(LiveSplitState timer, dynamic old, dynamic current)
        {
            return current.gameTime == old.gameTime;
        }

        public TimeSpan? GameTime(LiveSplitState timer, dynamic old, dynamic current)
        {
            return null;
        }
    }
}
