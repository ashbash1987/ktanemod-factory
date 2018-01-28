using System;
using System.Collections.Generic;
using System.Linq;

namespace FactoryAssembly
{
    public static partial class FactoryGameModePicker
    {
        public enum GameMode
        {
            [GameModeType(typeof(StaticMode), "Static")]
            Static,

            [GameModeType(typeof(FiniteSequenceMode), "Finite")]
            FiniteSequence,

            [GameModeType(typeof(FiniteSequenceMode), "Finite + Global Time")]
            [GameModeAdaptation(typeof(GlobalTimerAdaptation))]
            FiniteSequenceGlobalTime,

            [GameModeType(typeof(FiniteSequenceMode), "Finite + Global Strikes")]
            [GameModeAdaptation(typeof(GlobalStrikesAdaptation))]
            FiniteSequenceGlobalStrikes,

            [GameModeType(typeof(FiniteSequenceMode), "Finite + Global Time & Strikes")]
            [GameModeAdaptation(typeof(GlobalTimerAdaptation))]
            [GameModeAdaptation(typeof(GlobalStrikesAdaptation))]
            FiniteSequenceGlobalTimeStrikes,

            [GameModeType(typeof(InfiniteSequenceMode), "∞")]
            [RequireMultipleBombs()]
            InfiniteSequence,

            [GameModeType(typeof(InfiniteSequenceMode), "∞ + Global Time")]
            [GameModeAdaptation(typeof(GlobalTimerAdaptation))]
            [RequireMultipleBombs()]
            InfiniteSequenceGlobalTime,

            [GameModeType(typeof(InfiniteSequenceMode), "∞ + Global Strikes")]
            [GameModeAdaptation(typeof(GlobalStrikesAdaptation))]
            [RequireMultipleBombs()]
            InfiniteSequenceGlobalStrikes,

            [GameModeType(typeof(InfiniteSequenceMode), "∞ + Global Time & Strikes")]
            [GameModeAdaptation(typeof(GlobalTimerAdaptation))]
            [GameModeAdaptation(typeof(GlobalStrikesAdaptation))]
            [RequireMultipleBombs()]
            InfiniteSequenceGlobalTimeStrikes,
        }

        public static string GetFriendlyName(this GameMode gameMode)
        {
            return gameMode.GetAttributeOfType<GameModeTypeAttribute>().FriendlyName;
        }

        public static Type[] GetGameModeAdapations(this GameMode gameMode)
        {
            GameModeAdaptationAttribute[] adaptations = gameMode.GetAttributesOfType<GameModeAdaptationAttribute>();
            if (adaptations != null)
            {
                return adaptations.Select((x) => x.AdapatationType).ToArray();
            }

            return null;
        }

        public static bool RequiresMultipleBombs(this GameMode gameMode)
        {
            return gameMode.GetAttributeOfType<RequireMultipleBombsAttribute>() != null;
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
