using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LiveSplit.AHatInTime
{
    public enum GameState : int
    {
        Inactive = 0,
        Running = 1,
        Credits = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GameData
    {
        public GameState State;
        public double UnpauseTime;
        public bool IsPaused;
        public double CurrentGameSessionTime;
        public int TimePieceCount;
    }
}
