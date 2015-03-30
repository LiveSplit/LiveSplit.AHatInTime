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

    [StructLayout(LayoutKind.Explicit)]
    public struct GameData
    {
        [FieldOffset(0x0)]
        public GameState State;
        [FieldOffset(0x4)]
        public double UnpauseTime;
        [FieldOffset(0xC)]
        public bool IsPaused;
        [FieldOffset(0x10)]
        public double CurrentGameSessionTime;
        [FieldOffset(0x18)]
        public int TimePieceCount;
    }
}
