using System;
using System.Collections.Generic;

namespace FactoryAssembly
{
    public static partial class FactoryGameModePicker
    {
        public enum GameMode
        {
            [GameModeType(typeof(StaticMode), "Static", false)]
            Static,

            [GameModeType(typeof(FiniteSequenceMode), "Finite", false)]
            FiniteSequence,

            [GameModeType(typeof(FiniteSequenceMode), "Finite + Global Time", false, typeof(GlobalTimerAdaptation))]
            FiniteSequenceGlobalTime,

            [GameModeType(typeof(FiniteSequenceMode), "Finite + Global Strikes", false, typeof(GlobalStrikesAdaptation))]
            FiniteSequenceGlobalStrikes,

            [GameModeType(typeof(FiniteSequenceMode), "Finite + Global Time & Strikes", false, typeof(GlobalTimerAdaptation), typeof(GlobalStrikesAdaptation))]
            FiniteSequenceGlobalTimeStrikes,

            [GameModeType(typeof(InfiniteSequenceMode), "∞", true)]
            InfiniteSequence,

            [GameModeType(typeof(InfiniteSequenceMode), "∞ + Global Time", true, typeof(GlobalTimerAdaptation))]
            InfiniteSequenceGlobalTime,

            [GameModeType(typeof(InfiniteSequenceMode), "∞ + Global Strikes", true, typeof(GlobalStrikesAdaptation))]
            InfiniteSequenceGlobalStrikes,

            [GameModeType(typeof(InfiniteSequenceMode), "∞ + Global Time & Strikes", true, typeof(GlobalTimerAdaptation), typeof(GlobalStrikesAdaptation))]
            InfiniteSequenceGlobalTimeStrikes,
        }

        public static string GetFriendlyName(this GameMode gameMode)
        {
            return gameMode.GetAttributeOfType<GameModeTypeAttribute>().FriendlyName;
        }

        public static bool RequiresMultipleBombs(this GameMode gameMode)
        {
            return gameMode.GetAttributeOfType<GameModeTypeAttribute>().RequireMultipleBombs;
        }

        public static string[] GetModeNames
        {
            get
            {
                List<string> modeNames = new List<string>();
                foreach (GameMode gameMode in Enum.GetValues(typeof(GameMode)))
                {
                    modeNames.Add(gameMode.GetFriendlyName());
                }

                return modeNames.ToArray();
            }
        }

        public static bool[] GetModeSupport
        {
            get
            {
                List<bool> modeSupport = new List<bool>();
                foreach (GameMode gameMode in Enum.GetValues(typeof(GameMode)))
                {
                    bool requireMultipleBombs = gameMode.RequiresMultipleBombs();
                    modeSupport.Add(!requireMultipleBombs || MultipleBombsInterface.CanAccess);
                }

                return modeSupport.ToArray();
            }
        }
    }
}
