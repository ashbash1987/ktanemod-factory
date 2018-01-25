using Assets.Scripts.Missions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryAssembly
{
    public static class FactoryGameModePicker
    {
        public enum GameMode
        {
            [GameModeType(typeof(StaticMode), "Static Mode")]
            Static,

            [GameModeType(typeof(FiniteSequenceMode), "Finite Sequence Mode")]
            FiniteSequence,

            [GameModeType(typeof(InfiniteSequenceMode), "Infinite Sequence Mode")]
            InfiniteSequence
        }

        public static string[] GetModeNames
        {
            get
            {
                List<string> modeNames = new List<string>();
                foreach (GameMode gameMode in Enum.GetValues(typeof(GameMode)))
                {
                    modeNames.Add(gameMode.GetAttributeOfType<GameModeTypeAttribute>().FriendlyName);
                }

                return modeNames.ToArray();
            }
        }

        private const string FACTORY_MODE_POOL_ID = "Factory Mode";
        private const string MULTIPLE_BOMBS_POOL_ID = "Multiple Bombs";
        private static Dictionary<string, GameMode> _discoveredMissions = new Dictionary<string, GameMode>();

        public static void UpdateCompatibleMissions()
        {
            foreach (ModMission mission in ModManager.Instance.ModMissions)
            {
                UpdateMission(mission);
            }
        }

        public static void UpdateMission(Mission mission, bool force = false, bool tidyOtherComponents = false)
        {
            if (!_discoveredMissions.ContainsKey(mission.ID) || force)
            {
                _discoveredMissions[mission.ID] = GetGameModeForMission(mission, tidyOtherComponents);
            }
        }

        public static FactoryGameMode CreateGameMode(string missionID, GameObject gameObject)
        {
            if (missionID.Equals(FreeplayMissionGenerator.FREEPLAY_MISSION_ID))
            {
                return gameObject.AddComponent<FiniteSequenceMode>();
            }
            else
            {
                GameMode gameMode = GameMode.Static;
                if (_discoveredMissions.TryGetValue(missionID, out gameMode))
                {
                    return CreateGameMode(gameMode, gameObject);
                }

                return gameObject.AddComponent<FiniteSequenceMode>();
            }
        }

        private static FactoryGameMode CreateGameMode(GameMode gameMode, GameObject gameObject)
        {
            GameModeTypeAttribute attribute = gameMode.GetAttributeOfType<GameModeTypeAttribute>();

            if (attribute != null)
            {
                return gameObject.AddComponent(attribute.Type) as FactoryGameMode;
            }
            else
            {
                return gameObject.AddComponent<StaticMode>();
            }
        }
    
        private static GameMode GetGameModeForMission(Mission mission, bool tidyOtherComponents)
        {
            GameMode gameMode = GameMode.Static;

            for (int componentPoolIndex = mission.GeneratorSetting.ComponentPools.Count - 1; componentPoolIndex >= 0; componentPoolIndex--)
            {
                ComponentPool pool = mission.GeneratorSetting.ComponentPools[componentPoolIndex];
                if (pool.ModTypes != null && pool.ModTypes.Count == 1)
                {
                    switch (pool.ModTypes[0])
                    {
                        case FACTORY_MODE_POOL_ID:
                            mission.GeneratorSetting.ComponentPools.RemoveAt(componentPoolIndex);

                            int factoryModeIndex = pool.Count;
                            gameMode = (GameMode)factoryModeIndex;
                            break;

                        case MULTIPLE_BOMBS_POOL_ID:
                            //This is here to "fix" custom missions with multiple-bombs pools also added to them when trying to manually generate additional bombs for 'infinite' modes
                            if (tidyOtherComponents)
                            {
                                mission.GeneratorSetting.ComponentPools.RemoveAt(componentPoolIndex);
                            }
                            break;

                        default:
                            break;
                    }
                    
                }
            }

            return gameMode;
        }
    }
}
