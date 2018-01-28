using Assets.Scripts.Missions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryAssembly
{
    public static partial class FactoryGameModePicker
    {
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

        public static void UpdateMission(Mission mission, bool force = false, bool tidyOtherComponents = false, bool mustReturnValue = false)
        {
            //Get the previously discovered gamemode to see what the mission was defined as
            GameMode? previousDiscoveredGameMode = GetGameModeForMission(mission);

            //If there's no previous discovered gamemode, or we're forcing the change, look into the mission directly
            if (!previousDiscoveredGameMode.HasValue || force)
            {
                GameMode? gameMode = GenerateGameModeForMission(mission, tidyOtherComponents, mustReturnValue);

                //If there's a non-null value, then add to the dictionary
                if (gameMode.HasValue)
                {
                    _discoveredMissions[mission.ID] = gameMode.Value;
                }
                //Otherwise, remove from the previously discovered dictionary
                else
                {
                    if (_discoveredMissions.Remove(mission.ID))
                    {
                        AddComponentPoolToMission(mission, FACTORY_MODE_POOL_ID, (int)previousDiscoveredGameMode.Value);

                        if (previousDiscoveredGameMode.HasValue && previousDiscoveredGameMode.Value.RequiresMultipleBombs() && !MultipleBombsInterface.CanAccess)
                        {
                            AddComponentPoolToMission(mission, MULTIPLE_BOMBS_POOL_ID, 0);
                        }                        
                    }
                }
            }
            //Otherwise, there was a previous change and not forcing it, and it requires MultipleBombs, but MultipleBombs cannot be accessed, then remove from the previously discovered dictionary
            else if (previousDiscoveredGameMode.HasValue && previousDiscoveredGameMode.Value.RequiresMultipleBombs() && !MultipleBombsInterface.CanAccess)
            {
                _discoveredMissions.Remove(mission.ID);

                AddComponentPoolToMission(mission, FACTORY_MODE_POOL_ID, (int)previousDiscoveredGameMode.Value);

                if (previousDiscoveredGameMode.HasValue && previousDiscoveredGameMode.Value.RequiresMultipleBombs() && !MultipleBombsInterface.CanAccess)
                {
                    AddComponentPoolToMission(mission, MULTIPLE_BOMBS_POOL_ID, 0);
                }
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
                GameMode? gameMode = GetGameModeForMission(missionID);
                if (gameMode.HasValue)
                {
                    return CreateGameMode(gameMode.Value, gameObject);
                }
                else
                {
                    return CreateGameMode(GameMode.Static, gameObject);
                }
            }
        }

        private static GameMode? GetGameModeForMission(Mission mission)
        {
            return GetGameModeForMission(mission.ID);
        }

        private static GameMode? GetGameModeForMission(string missionID)
        {
            GameMode gameMode;
            if (_discoveredMissions.TryGetValue(missionID, out gameMode))
            {
                return gameMode;
            }

            return null;
        }

        private static FactoryGameMode CreateGameMode(GameMode gameMode, GameObject gameObject)
        {
            GameModeTypeAttribute attribute = gameMode.GetAttributeOfType<GameModeTypeAttribute>();

            if (attribute != null)
            {
                Logging.Log("Creating gamemode '{0}'.", attribute.FriendlyName);
                FactoryGameMode factoryGameMode = gameObject.AddComponent(attribute.Type) as FactoryGameMode;

                Type[] adapations = gameMode.GetGameModeAdapations();
                if (adapations != null)
                {
                    foreach (Type adaptationType in adapations)
                    {
                        factoryGameMode.AddAdaptation(adaptationType);
                    }
                }

                return factoryGameMode;
            }
            else
            {
                Logging.Log("Creating gamemode '{0}'.", GameMode.Static.GetFriendlyName());
                return gameObject.AddComponent<StaticMode>();
            }
        }
    
        /// <summary>
        /// Returns the defined GameMode for the given mission, or null if there is a problem with supporting the mission.
        /// This also adjusts the component pools by removing any non-standard component pools to allow missions to run correctly.
        /// </summary>
        /// <remarks>
        /// The one prime reason for a mission not being correctly supported is in the case of a gamemode that requires MultipleBombs functionality, but MultipleBombs is not present.
        /// For these cases, the missions are not 'corrected' and so should be 'locked out' in the bomb binder, unless mustReturnValid is set to true, whereby any invalid gamemode would instead just return as static.
        /// </remarks>
        private static GameMode? GenerateGameModeForMission(Mission mission, bool tidyOtherComponents, bool mustReturnValid)
        {
            //Default suitability is null
            GameMode? gameMode = null;

            for (int componentPoolIndex = mission.GeneratorSetting.ComponentPools.Count - 1; componentPoolIndex >= 0; componentPoolIndex--)
            {
                ComponentPool pool = mission.GeneratorSetting.ComponentPools[componentPoolIndex];
                if (pool.ModTypes != null && pool.ModTypes.Count == 1)
                {
                    switch (pool.ModTypes[0])
                    {
                        case FACTORY_MODE_POOL_ID:
                            int factoryModeIndex = pool.Count;
                            gameMode = (GameMode)factoryModeIndex;

                            //If the game mode is safe to run, then safely remove the component pool
                            if (!gameMode.Value.RequiresMultipleBombs() || MultipleBombsInterface.CanAccess)
                            {
                                mission.GeneratorSetting.ComponentPools.RemoveAt(componentPoolIndex);
                            }
                            //Else the game mode is not safe to run, but we still need to return a valid gamemode, so still remove the component pool, but force into 'static' mode
                            else if (mustReturnValid)
                            {
                                Logging.Log("Mission {0} should be configured for '{1}', but MultipleBombs is required and cannot be accessed.", mission.ID, gameMode.Value.GetFriendlyName());

                                gameMode = GameMode.Static;
                                mission.GeneratorSetting.ComponentPools.RemoveAt(componentPoolIndex);
                            }
                            //Else the game mode is not safe to run at all, so return null and re-add a multiple bombs component pool to drive the point home in the bomb binder
                            else
                            {
                                Logging.Log("Mission {0} should be configured for '{1}', but MultipleBombs is required and cannot be accessed.", mission.ID, gameMode.Value.GetFriendlyName());

                                gameMode = null;
                                AddComponentPoolToMission(mission, MULTIPLE_BOMBS_POOL_ID, 0);
                            }
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

            if (gameMode.HasValue)
            {
                Logging.Log("Mission {0} configured for '{1}'.", mission.ID, gameMode.Value.GetFriendlyName());
            }
            else
            {
                Logging.Log("Mission {0} not configured for any special gamemode.", mission.ID);
            }

            return gameMode;
        }

        private static void AddComponentPoolToMission(Mission mission, string componentName, int count)
        {
            mission.GeneratorSetting.ComponentPools.Add(new ComponentPool() { Count = count, AllowedSources = ComponentPool.ComponentSource.Mods, SpecialComponentType = SpecialComponentTypeEnum.None, ModTypes = new List<string>() { componentName } });
        }
    }
}
