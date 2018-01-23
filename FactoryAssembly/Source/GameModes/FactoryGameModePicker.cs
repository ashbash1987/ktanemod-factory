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
            FiniteSequence
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
        private static Dictionary<string, GameMode> _discoveredMissions = new Dictionary<string, GameMode>();

        public static void UpdateCompatibleMissions()
        {
            foreach (ModMission mission in ModManager.Instance.ModMissions)
            {
                UpdateMission(mission);
            }
        }

        public static void UpdateMission(Mission mission, bool force = false)
        {
            if (!_discoveredMissions.ContainsKey(mission.ID) || force)
            {
                _discoveredMissions[mission.ID] = GetGameModeForMission(mission);
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
    
        private static GameMode GetGameModeForMission(Mission mission)
        {
            GameMode gameMode = GameMode.Static;

            for (int componentPoolIndex = mission.GeneratorSetting.ComponentPools.Count - 1; componentPoolIndex >= 0; componentPoolIndex--)
            {
                ComponentPool pool = mission.GeneratorSetting.ComponentPools[componentPoolIndex];
                if (pool.ModTypes != null && pool.ModTypes.Count == 1 && pool.ModTypes[0] == FACTORY_MODE_POOL_ID)
                {
                    mission.GeneratorSetting.ComponentPools.RemoveAt(componentPoolIndex);

                    int factoryModeIndex = pool.Count;
                    gameMode = (GameMode)factoryModeIndex;
                }
            }

            return gameMode;
        }
    }
}
