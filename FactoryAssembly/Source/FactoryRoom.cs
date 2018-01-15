using Assets.Scripts.Pacing;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FactoryAssembly
{
    public class FactoryRoom : MonoBehaviour
    {
        /// <remarks>
        /// Due to issues using custom assembly in Unity not exposing fields, have to discover things by name instead. Not the greatest option, but it works.
        /// </remarks>
        private static readonly string[] CONVEYOR_BELT_NODE_NAMES = { "ConveyorBeltNodeA", "ConveyorBeltNodeB" };
        private static readonly string LEFT_DOOR_NAME = "LeftDoor";
        private static readonly string RIGHT_DOOR_NAME = "RightDoor";
        private static readonly string CONVEYOR_TOP_NAME = "ConveyorTop";
        private static readonly string DOOR_LONG_AUDIO_NAME = "DoorLong";
        private static readonly string DOOR_SHORT_AUDIO_NAME = "DoorShort";
        private static readonly string CONVEYOR_AUDIO_NAME = "Conveyor";

        private Animator _conveyorBeltAnimator = null;
        private KMGameplayRoom _room = null;
        private KMAudio _audio = null;

        private Queue<FactoryBomb> _bombs = new Queue<FactoryBomb>();
        private FactoryBomb _currentBomb = null;
        private FactoryBomb _oldBomb = null;

        private Transform[] _conveyorBeltNodes = null;
        private int _nextBeltNodeIndex = 0;

        private Transform _leftDoor = null;
        private Transform _rightDoor = null;
        private Transform _conveyorTop = null;

        #region Unity Lifecycle
        /// <summary>
        /// Unity event.
        /// </summary>
        private void Awake()
        {
            _conveyorBeltAnimator = GetComponent<Animator>();
            _room = GetComponent<KMGameplayRoom>();
            _audio = GetComponent<KMAudio>();

            _room.OnLightChange += OnLightChange;
        }

        /// <summary>
        /// Unity event.
        /// </summary>
        private void Start()
        {
            //Get the conveyor belt nodes
            _conveyorBeltNodes = new Transform[CONVEYOR_BELT_NODE_NAMES.Length];
            for (int nodeIndex = 0; nodeIndex < CONVEYOR_BELT_NODE_NAMES.Length; ++nodeIndex)
            {
                _conveyorBeltNodes[nodeIndex] = transform.Find(CONVEYOR_BELT_NODE_NAMES[nodeIndex]);
            }

            _leftDoor = transform.Find(LEFT_DOOR_NAME);
            _rightDoor = transform.Find(RIGHT_DOOR_NAME);
            _conveyorTop = transform.Find(CONVEYOR_TOP_NAME);

            StartCoroutine(FindBombs());
            StartCoroutine(StartGameplay());
        }

        /// <summary>
        /// Unity event.
        /// </summary>
        private void Update()
        {
            if (_currentBomb != null && _currentBomb.IsReadyToShip)
            {
                _currentBomb.DisableBomb();
                GetNextBomb();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Called by KMGameplayRoom on lighting change pacing events.
        /// </summary>
        /// <param name="on">If true, lights should be on; if false, lights should be off.</param>
        private void OnLightChange(bool on)
        {
            //TODO: Control light intensity in the room
        }

        /// <summary>
        /// Coroutine to find spawned bombs in the gameplay room.
        /// </summary>
        private IEnumerator FindBombs()
        {
            yield return null;
            yield return null;

            Bomb[] bombs = FindObjectsOfType<Bomb>();

            foreach (Bomb bomb in bombs)
            {
                FactoryBomb factoryBomb = bomb.gameObject.AddComponent<FactoryBomb>();
                factoryBomb.SetupStartPosition(_conveyorBeltNodes[0]);
                _bombs.Enqueue(factoryBomb);
            }
        }

        /// <summary>
        /// Coroutine to replace the standard GameplayState start round coroutine.
        /// </summary>
        private IEnumerator StartGameplay()
        {
            GameplayState gameplayState = SceneManager.Instance.GameplayState;

            gameplayState.StopAllCoroutines();

            yield return new WaitForSeconds(2.0f);

            gameplayState.Room.ActivateCeilingLights();
            if (GameplayState.OnLightsOnEvent != null)
            {
                GameplayState.OnLightsOnEvent();
            }

            if ((KTInputManager.Instance.GetCurrentSelectable() == null || KTInputManager.Instance.GetCurrentSelectable().Parent == KTInputManager.Instance.RootSelectable) && !KTInputManager.Instance.IsMotionControlMode())
            {
                KTInputManager.Instance.SelectRootDefault();
            }

            if (KTInputManager.Instance.IsMotionControlMode())
            {
                KTInputManager.Instance.SelectableManager.EnableMotionControls();
            }

            yield return new WaitForSeconds(1.0f);

            GetNextBomb();

            PropertyInfo roundStartedProperty = typeof(GameplayState).GetProperty("RoundStarted", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            roundStartedProperty.SetValue(gameplayState, true, null);

            FieldInfo paceMakerField = typeof(GameplayState).GetField("paceMaker", BindingFlags.Instance | BindingFlags.NonPublic);
            ((PaceMaker)paceMakerField.GetValue(gameplayState)).StartRound(gameplayState.Mission);
        }

        /// <summary>
        /// Requests the next bomb to show up.
        /// </summary>
        private void GetNextBomb()
        {
            _oldBomb = _currentBomb;

            if (_bombs.Count != 0)
            {
                _currentBomb = _bombs.Dequeue();
                _currentBomb.AttachToConveyor(_conveyorBeltNodes[_nextBeltNodeIndex]);
                _nextBeltNodeIndex = (_nextBeltNodeIndex + 1) % _conveyorBeltNodes.Length;
            }
            else
            {
                _currentBomb = null;
            }

            _conveyorBeltAnimator.SetTrigger("NextBomb");
            _audio.PlaySoundAtTransform(CONVEYOR_AUDIO_NAME, _conveyorTop);
        }

        #region Animation Methods
        /// <summary>
        /// Starts the 'current' bomb.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        private void StartBomb()
        {
            if (_currentBomb != null)
            {
                _currentBomb.StartBomb();
            }
        }

        /// <summary>
        /// Ends the 'old' bomb.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        private void EndBomb()
        {
            if (_oldBomb != null)
            {
                _oldBomb.EndBomb();
                _oldBomb = null;
            }
        }

        /// <summary>
        /// The left door opens.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        private void DoorLeftOpen()
        {
            _audio.PlaySoundAtTransform(DOOR_LONG_AUDIO_NAME, _leftDoor);
        }

        /// <summary>
        /// The left door closes.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        private void DoorLeftClose()
        {
            _audio.PlaySoundAtTransform(DOOR_SHORT_AUDIO_NAME, _leftDoor);
        }

        /// <summary>
        /// The right door opens.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        private void DoorRightOpen()
        {
            _audio.PlaySoundAtTransform(DOOR_SHORT_AUDIO_NAME, _rightDoor);
        }

        /// <summary>
        /// The right door closes.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        private void DoorRightClose()
        {
            _audio.PlaySoundAtTransform(DOOR_LONG_AUDIO_NAME, _rightDoor);
        }
        #endregion
        #endregion
    }
}
