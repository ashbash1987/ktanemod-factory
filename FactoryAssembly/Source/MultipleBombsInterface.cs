using System;
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
        private const string MULTIPLE_BOMBS_TYPE_NAME = "MultipleBombsAssembly.MultipleBombs";
        private const string CREATE_BOMB_METHOD_NAME = "createBomb";

        internal static bool CanAccess
        {
            get
            {
                return _createBombMethod != null;
            }
        }

        private static Type _multipleBombsType = null;
        private static object _multipleBombsObject = null;
        private static MethodInfo _createBombMethod = null;

        /// <summary>
        /// Called from FactoryService.OnStateChange upon entering 'Setup' state. Re-ensures that we have access to MultipleBombs via reflection.
        /// </summary>
        internal static void RediscoverMultipleBombs()
        {
            Logging.Log("Rechecking for MultipleBombs...");

            //Reset all the reflected fields first
            _multipleBombsType = null;
            _multipleBombsObject = null;
            _createBombMethod = null;

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

            //Try to find the create bomb method
            _createBombMethod = _multipleBombsType.GetMethod(CREATE_BOMB_METHOD_NAME, BindingFlags.NonPublic | BindingFlags.Instance);
            if (_createBombMethod == null)
            {
                Logging.Log("Cannot find the MultipleBombs create bomb method - MultipleBombs gamemodes are disabled.");
                return;
            }

            Logging.Log("MultipleBombs gamemodes are enabled.");
        }

        internal static Bomb CreateBomb(string missionID, Transform targetTransform)
        {
            if (!CanAccess)
            {
                Logging.Log("Tried to create a bomb, but MultipleBombs cannot be accessed, so cannot create a bomb.");
                return null;
            }

            Vector3 position = targetTransform.position;
            Vector3 eulerAngles = targetTransform.eulerAngles;

            //Current expected method signature is:
            //MultipleBombsAssembly.MultipleBombs.createBomb(string missionId, Vector3 position, Vector3 eulerAngles, List<KMBombInfo> knownBombInfos)
            return (Bomb)_createBombMethod.Invoke(_multipleBombsObject, new object[] { missionID, position, eulerAngles, null });
        }
    }
}
