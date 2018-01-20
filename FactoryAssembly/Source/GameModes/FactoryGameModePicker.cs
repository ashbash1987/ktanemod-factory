using Assets.Scripts.Missions;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryAssembly
{
    public static class FactoryGameModePicker
    {
        public enum GameMode
        {
            Static,
            FiniteSequence
        }

        private const string FACTORY_MODE_POOL_ID = "Factory Mode";
        private static Dictionary<string, GameMode> _discoveredMissions = new Dictionary<string, GameMode>();

        public static void UpdateCompatibleMissions()
        {
            foreach (ModMission mission in ModManager.Instance.ModMissions)
            {
                if (!_discoveredMissions.ContainsKey(mission.ID))
                {
                    _discoveredMissions[mission.ID] = GetGameModeForMission(mission);
                }
            }
        }

        public static FactoryGameMode CreateGameMode(string missionID, GameObject gameObject)
        {
            if (missionID.Equals(FreeplayMissionGenerator.FREEPLAY_MISSION_ID))
            {
                return gameObject.AddComponent<FiniteSequenceMode>();
            }
            else if (missionID.Equals(ModMission.CUSTOM_MISSION_ID))
            {
                GameMode gameMode = GetGameModeForMission(GameplayState.CustomMission);
                return CreateGameMode(gameMode, gameObject);
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
            switch (gameMode)
            {
                case GameMode.Static:
                    return gameObject.AddComponent<StaticMode>();

                case GameMode.FiniteSequence:
                    return gameObject.AddComponent<FiniteSequenceMode>();

                default:
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
