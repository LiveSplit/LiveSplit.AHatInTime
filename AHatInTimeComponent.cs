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
            return null;
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
                Pointer = new DeepPointer<byte>(1, Game, "HatinTimeGame.exe", 0x022225F0, 0x4, 0x428, 0x670, 0x40, 0x48)
            });
            State.ValueDefinitions.Add(new ASLValueDefinition()
            {
                Identifier = "hourglasses2",
                Pointer = new DeepPointer<byte>(1, Game, "HatinTimeGame.exe", 0x021D5344, 0xe0, 0x4cc, 0x3dc, 0x3c, 0x48)
            });
            State.ValueDefinitions.Add(new ASLValueDefinition()
            {
                Identifier = "gameTime",
                Pointer = new DeepPointer<float>(1, Game, "HatinTimeGame.exe", 0x220D604)
            });
            State.ValueDefinitions.Add(new ASLValueDefinition()
            {
                Identifier = "map",
                Pointer = new DeepPointer<string>(32, Game, "HatinTimeGame.exe", 0x02208E38, 0x670, 0x0)
            });
            ((dynamic)State.Data).pointerdelta = 0;
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
            var offset = TimeSpan.FromSeconds(1.2);
            if (timer.Run.Offset != offset)
                timer.Run.Offset = offset;

            if (current.map == "hub_spaceship" && old.map == "hat_startup" && (current.hourglasses == 0 || current.hourglasses2 == 0))
            {
                current.counter = 0;
                return true;
            }

            return false;
        }

        public bool Split(LiveSplitState timer, dynamic old, dynamic current)
        {
            return current.hourglasses == (old.hourglasses + 1) || current.hourglasses2 == (old.hourglasses2 + 1);
        }

        public bool Reset(LiveSplitState timer, dynamic old, dynamic current)
        {
            return false;
        }

        public bool IsPaused(LiveSplitState timer, dynamic old, dynamic current)
        {
            if (current.gameTime == old.gameTime)
            {
                if (!timer.IsGameTimePaused)
                    current.counter++;
            }
            else if (timer.IsGameTimePaused)
                current.counter--;
            else
                current.counter = 0;

            if (current.counter > 4)
                current.counter = 4;
            else if (current.counter < 0)
                current.counter = 0;

            if (timer.IsGameTimePaused)
                return current.counter > 0;
            else
                return current.counter == 4;
        }

        public TimeSpan? GameTime(LiveSplitState timer, dynamic old, dynamic current)
        {
            return null;
        }
    }
}
