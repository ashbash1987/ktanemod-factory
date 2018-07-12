﻿using Assets.Scripts.Missions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryAssembly
{
    internal static partial class FactoryGameModePicker
    {
        private const string FACTORY_MODE_POOL_ID = "Factory Mode";
        private const string MULTIPLE_BOMBS_POOL_ID = "Multiple Bombs";
        private static Dictionary<string, GameMode> _discoveredMissions = new Dictionary<string, GameMode>();

        internal static void UpdateCompatibleMissions()
        {
            foreach (ModMission mission in ModManager.Instance.ModMissions)
            {
                UpdateMission(mission);
            }
        }

        internal static void UpdateMission(Mission mission, bool force = false, bool tidyOtherComponents = false, bool mustReturnValue = false)
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
                    //Don't want to re-add component pools into the custom mission though, otherwise the bomb generator won't be happy!
                    if (_discoveredMissions.Remove(mission.ID) && mission.ID != ModMission.CUSTOM_MISSION_ID)
                    {
                        AddComponentPoolToMission(mission, FACTORY_MODE_POOL_ID, (int)previousDiscoveredGameMode.Value);

                        if (previousDiscoveredGameMode.HasValue && previousDiscoveredGameMode.Value.RequiresMultipleBombs() && MultipleBombsInterface.AccessVersion == MultipleBombsInterface.AccessAPIVersion.None)
                        {
                            AddComponentPoolToMission(mission, MULTIPLE_BOMBS_POOL_ID, 0);
                        }                        
                    }
                }
            }
            //Otherwise, there was a previous change and not forcing it, and it requires MultipleBombs, but MultipleBombs cannot be accessed, then remove from the previously discovered dictionary
            else if (previousDiscoveredGameMode.HasValue && previousDiscoveredGameMode.Value.RequiresMultipleBombs() && MultipleBombsInterface.AccessVersion == MultipleBombsInterface.AccessAPIVersion.None)
            {
                _discoveredMissions.Remove(mission.ID);

                AddComponentPoolToMission(mission, FACTORY_MODE_POOL_ID, (int)previousDiscoveredGameMode.Value);

                if (previousDiscoveredGameMode.HasValue && previousDiscoveredGameMode.Value.RequiresMultipleBombs() && MultipleBombsInterface.AccessVersion == MultipleBombsInterface.AccessAPIVersion.None)
                {
                    AddComponentPoolToMission(mission, MULTIPLE_BOMBS_POOL_ID, 0);
                }
            }
        }

        internal static FactoryGameMode CreateGameMode(string missionID, GameObject gameObject, out FactoryGameModePicker.GameMode gameMode)
        {
            if (missionID.Equals(FreeplayMissionGenerator.FREEPLAY_MISSION_ID))
            {
                gameMode = GameMode.FiniteSequence;
                return new FiniteSequenceMode();
            }
            else
            {
                GameMode? discoveredGameMode = GetGameModeForMission(missionID);
                if (discoveredGameMode.HasValue)
                {
                    gameMode = discoveredGameMode.Value;
                    return CreateGameMode(gameMode, gameObject);
                }
                else
                {
                    gameMode = GameMode.Static;
                    return CreateGameMode(gameMode, gameObject);
                }
            }
        }

        internal static GameMode? GetGameModeForMission(Mission mission)
        {
            return GetGameModeForMission(mission.ID);
        }

        internal static GameMode? GetGameModeForMission(string missionID)
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
                FactoryGameMode factoryGameMode = Activator.CreateInstance(attribute.Type) as FactoryGameMode;

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
                return new StaticMode();
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

            Logging.Log($"Discovering component pools for mission {mission.name}...");
            for (int componentPoolIndex = 0; componentPoolIndex < mission.GeneratorSetting.ComponentPools.Count; componentPoolIndex++)
            {
                ComponentPool pool = mission.GeneratorSetting.ComponentPools[componentPoolIndex];

                if (pool.ModTypes != null)
                {
                    Logging.Log($"  {componentPoolIndex + 1}. Mod Types: {{{string.Join(", ", pool.ModTypes.ToArray())}}} ({pool.Count})");
                }
                else
                {
                    Logging.Log($"  {componentPoolIndex + 1}. No Mod Types.");
                }                
            }

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
                            if (!gameMode.Value.RequiresMultipleBombs() || MultipleBombsInterface.AccessVersion != MultipleBombsInterface.AccessAPIVersion.None)
                            {
                                Logging.Log($"Mission {mission.ID} has component pool configuration for '{gameMode.Value.GetFriendlyName()}'; removing from component pools.");

                                mission.GeneratorSetting.ComponentPools.RemoveAt(componentPoolIndex);
                            }
                            //Else the game mode is not safe to run, but we still need to return a valid gamemode, so still remove the component pool, but force into 'static' mode
                            else if (mustReturnValid)
                            {
                                Logging.Log($"Mission {mission.ID} should be configured for '{gameMode.Value.GetFriendlyName()}', but MultipleBombs is required and cannot be accessed.");

                                gameMode = GameMode.Static;
                                mission.GeneratorSetting.ComponentPools.RemoveAt(componentPoolIndex);
                            }
                            //Else the game mode is not safe to run at all, so return null and re-add a multiple bombs component pool to drive the point home in the bomb binder
                            else
                            {
                                Logging.Log($"Mission {mission.ID} should be configured for '{gameMode.Value.GetFriendlyName()}', but MultipleBombs is required and cannot be accessed; re-adding MultipleBombs component pool.");

                                gameMode = null;
                                AddComponentPoolToMission(mission, MULTIPLE_BOMBS_POOL_ID, 0);
                            }
                            break;

                        case MULTIPLE_BOMBS_POOL_ID:
                            //This is here to "fix" custom missions with multiple-bombs pools also added to them when trying to manually generate additional bombs for 'infinite' modes
                            if (tidyOtherComponents)
                            {
                                Logging.Log($"Mission {mission.ID} has component pool for 'Multiple Bombs', and component needs tidying away for reasons.");
                                mission.GeneratorSetting.ComponentPools.RemoveAt(componentPoolIndex);
                            }
                            else
                            {
                                Logging.Log($"Mission {mission.ID} has component pool for 'Multiple Bombs', but doesn't need tidying away.");
                            }
                            break;

                        default:
                            break;
                    }
                    
                }
            }

            if (gameMode.HasValue)
            {
                Logging.Log($"Mission {mission.ID} configured for '{gameMode.Value.GetFriendlyName()}'.");
            }
            else
            {
                Logging.Log($"Mission {mission.ID} not configured for any special gamemode.");
            }

            return gameMode;
        }

        private static void AddComponentPoolToMission(Mission mission, string componentName, int count)
        {
            mission.GeneratorSetting.ComponentPools.Add(new ComponentPool() { Count = count, AllowedSources = ComponentPool.ComponentSource.Mods, SpecialComponentType = SpecialComponentTypeEnum.None, ComponentTypes = new List<ComponentTypeEnum>(), AllSolvableVetoComponentTypes = new List<ComponentTypeEnum>(), ModTypes = new List<string>() { componentName } });
        }
    }
}
