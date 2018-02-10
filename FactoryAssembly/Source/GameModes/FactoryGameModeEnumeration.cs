using System;
using System.Collections.Generic;
using System.Linq;

namespace FactoryAssembly
{
    internal static partial class FactoryGameModePicker
    {
        internal enum GameMode
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

        internal static Type GetGameModeType(this GameMode gameMode)
        {
            return gameMode.GetAttributeOfType<GameModeTypeAttribute>().Type;
        }

        internal static string GetFriendlyName(this GameMode gameMode)
        {
            return gameMode.GetAttributeOfType<GameModeTypeAttribute>().FriendlyName;
        }

        internal static Type[] GetGameModeAdapations(this GameMode gameMode)
        {
            GameModeAdaptationAttribute[] adaptations = gameMode.GetAttributesOfType<GameModeAdaptationAttribute>();
            if (adaptations != null)
            {
                return adaptations.Select((x) => x.AdapatationType).ToArray();
            }

            return null;
        }

        internal static bool RequiresMultipleBombs(this GameMode gameMode)
        {
            return gameMode.GetAttributeOfType<RequireMultipleBombsAttribute>() != null;
        }

        internal static string[] GetModeNames
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

        internal static bool[] GetModeSupport
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
