using Assets.Scripts.Missions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FactoryAssembly
{
    /// <summary>
    /// This class holds all the necessary reflective implementation details in order to generate bombs using MultipleBombs instead of roll-my-own code (which would've been basically duplicated code).
    /// </summary>
    /// <remarks>
    /// Both Lupo511 and I agree that reflection is super-nasty for this, but it is really the only suitable way to do things for the time being, without having duplicated bomb creation code all over the shop.
    /// It also means now that Factory has a hard-line relationship to MultipleBombs for all gamemodes, rather than kinda required and kinda not.
    /// </remarks>
    internal static class MultipleBombsInterface
    {
        public enum AccessAPIVersion
        {
            None,
            V1,
            V2
        }

        private const string MULTIPLE_BOMBS_TYPE_NAME = "MultipleBombsAssembly.MultipleBombs";
        private const string CREATE_BOMB_METHOD_NAME = "createBomb";

        internal static AccessAPIVersion AccessVersion
        {
            get
            {
                if (_createBombMethodV1 != null)
                {
                    return AccessAPIVersion.V1;
                }
                if (_createBombMethodV2 != null)
                {
                    return AccessAPIVersion.V2;
                }

                return AccessAPIVersion.None;
            }
        }

        private static Type _multipleBombsType = null;
        private static object _multipleBombsObject = null;
        private static MethodInfo _createBombMethodV1 = null;
        private static MethodInfo _createBombMethodV2 = null;

        /// <summary>
        /// Called from FactoryService.OnStateChange upon entering 'Setup' state. Re-ensures that we have access to MultipleBombs via reflection.
        /// </summary>
        internal static void RediscoverMultipleBombs()
        {
            Logging.Log("Rechecking for MultipleBombs...");

            //Reset all the reflected fields first
            _multipleBombsType = null;
            _multipleBombsObject = null;
            _createBombMethodV1 = null;
            _createBombMethodV2 = null;

            //Try to find the type
            _multipleBombsType = ReflectionHelper.FindType(MULTIPLE_BOMBS_TYPE_NAME);
            if (_multipleBombsType == null)
            {
                Logging.Log("Cannot find the MultipleBombs type - MultipleBombs gamemodes are disabled.");
                return;
            }

            //Try to find an object with that type on it
            _multipleBombsObject = GameObject.FindObjectOfType(_multipleBombsType);
            if (_multipleBombsObject == null)
            {
                Logging.Log("Cannot find the MultipleBombs object - MultipleBombs gamemodes are disabled.");
                return;
            }

            //Try to find an appropriate create bomb method
            MethodInfo[] methods = _multipleBombsType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where((x) => x.Name == CREATE_BOMB_METHOD_NAME).ToArray();

            //V1 Signature: MultipleBombsAssembly.MultipleBombs.createBomb(string missionId, Vector3 position, Vector3 eulerAngles, List<KMBombInfo> knownBombInfos)
            _createBombMethodV1 = methods.FirstOrDefault((x) => x.GetParameters().Select(y => y.ParameterType).SequenceEqual(new Type[] { typeof(string), typeof(Vector3), typeof(Vector3), typeof(List<KMBombInfo>) }));

            //V2 Signature: MultipleBombsAssembly.MultipleBombs.createBomb(GeneratorSetting generatorSetting, Vector3 position, Vector3 eulerAngles, int seed, List<KMBombInfo> knownBombInfos)
            _createBombMethodV2 = methods.FirstOrDefault((x) => x.GetParameters().Select(y => y.ParameterType).SequenceEqual(new Type[] { typeof(int), typeof(Vector3), typeof(Vector3), typeof(int), typeof(List<KMBombInfo>) }));

            if (_createBombMethodV1 != null)
            {
                Logging.Log("MultipleBombs gamemodes are enabled (using V1 create bomb method).");
            }
            else if (_createBombMethodV2 != null)
            {
                Logging.Log("MultipleBombs gamemodes are enabled (using V2 create bomb method).");
            }
            else
            {
                Logging.Log("Cannot find a valid MultipleBombs create bomb method - MultipleBombs gamemodes are disabled.");
                return;
            }
        }

        internal static Bomb CreateBomb(string missionID, int bombIndex, Transform targetTransform)
        {
            Vector3 position = targetTransform.position;
            Vector3 eulerAngles = targetTransform.eulerAngles;

            switch (AccessVersion)
            {
                case AccessAPIVersion.V1:
                    Logging.Log("Creating bomb using MultipleBombs V1...");
                    return (Bomb)_createBombMethodV1.Invoke(_multipleBombsObject, new object[] { missionID, position, eulerAngles, null });

                case AccessAPIVersion.V2:
                    Logging.Log("Creating bomb using MultipleBombs V2...");
                    int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                    return (Bomb)_createBombMethodV2.Invoke(_multipleBombsObject, new object[] { bombIndex, position, eulerAngles, seed, null });

                default:
                    Logging.Log("Tried to create a bomb, but MultipleBombs cannot be accessed, so cannot create a bomb.");
                    return null;
            }
        }
    }
}
