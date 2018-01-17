using Assets.Scripts.Pacing;
using System;
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

        private static readonly Color AMBIENT_OFF_COLOR = new Color(0.01f, 0.01f, 0.01f);
        private static readonly Color AMBIENT_ON_COLOR = new Color(0.4f, 0.4f, 0.4f);

        private const float LIGHT_OFF_INTENSITY = 0.02f;
        private const float LIGHT_ON_INTENSITY = 0.8f;

        private Animator _conveyorBeltAnimator = null;
        private Light[] _lights = null;
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

        private Selectable _roomSelectable = null;
        private Selectable[] _roomChildren = null;
        private int _bombSelectableIndex = 0;
        private bool _initialSwitchOn = true;

        #region Unity Lifecycle
        /// <summary>
        /// Unity event.
        /// </summary>
        private void Awake()
        {
            _conveyorBeltAnimator = GetComponent<Animator>();
            _lights = GetComponentsInChildren<Light>();
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
            StartCoroutine(AdjustSelectableGrid());

            OnLightChange(false);
        }

        /// <summary>
        /// Unity event.
        /// </summary>
        private void Update()
        {
            if (_currentBomb != null && _currentBomb.IsReadyToShip)
            {
                SetSelectableBomb(null);
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
            if (_initialSwitchOn)
            {
                if (on)
                {
                    StartCoroutine(ChangeLightIntensity(KMSoundOverride.SoundEffect.Switch, 0.0f, LIGHT_ON_INTENSITY, AMBIENT_ON_COLOR));
                    _initialSwitchOn = false;
                }
                else
                {
                    StartCoroutine(ChangeLightIntensity(null, 0.0f, LIGHT_OFF_INTENSITY, AMBIENT_OFF_COLOR));
                }
            }
            else
            {
                if (on)
                {
                    StartCoroutine(ChangeLightIntensity(KMSoundOverride.SoundEffect.LightBuzzShort, 0.5f, LIGHT_ON_INTENSITY, AMBIENT_ON_COLOR));
                }
                else
                {
                    StartCoroutine(ChangeLightIntensity(KMSoundOverride.SoundEffect.LightBuzz, 1.0f, LIGHT_OFF_INTENSITY, AMBIENT_OFF_COLOR));
                }
            }
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
        /// Coroutine to adjust the selectable grid, which is manipulated by Multiple Bombs initially to normally accomodate multiple bombs in the selectable grid.
        /// </summary>
        private IEnumerator AdjustSelectableGrid()
        {
            GameplayState gameplayState = SceneManager.Instance.GameplayState;
            _roomSelectable = gameplayState.Room.GetComponent<Selectable>();

            _roomChildren = new Selectable[_roomSelectable.Children.Length];
            Array.Copy(_roomSelectable.Children, _roomChildren, _roomChildren.Length);

            int roomChildRowLength = _roomSelectable.ChildRowLength;
            int roomDefaultSelectableIndex = _roomSelectable.DefaultSelectableIndex;

            _bombSelectableIndex = Array.FindIndex(_roomChildren, (x) => x != null && x.GetComponent<Bomb>() != null);

            yield return null;
            yield return null;

            _roomSelectable.Children = _roomChildren;
            _roomSelectable.ChildRowLength = roomChildRowLength;
            _roomSelectable.DefaultSelectableIndex = roomDefaultSelectableIndex;

            SetSelectableBomb(null);
        }

        private IEnumerator ChangeLightIntensity(KMSoundOverride.SoundEffect? sound, float wait, float lightIntensity, Color ambientColor)
        {
            if (sound.HasValue)
            {
                _audio.PlayGameSoundAtTransform(sound.Value, transform);
            }

            if (wait > 0.0f)
            {
                yield return new WaitForSeconds(wait);
            }

            foreach (Light light in _lights)
            {
                light.intensity = lightIntensity;
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.ambientIntensity = 0.0f;
            DynamicGI.UpdateEnvironment();
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

        private void SetSelectableBomb(FactoryBomb bomb)
        {
            KTInputManager.Instance.ClearSelection();

            Selectable selectable = bomb != null ? bomb.Selectable : null;

            _roomChildren[_bombSelectableIndex] = selectable;

            KTInputManager.Instance.RootSelectable = _roomSelectable;
            KTInputManager.Instance.SelectableManager.ConfigureSelectableAreas(KTInputManager.Instance.RootSelectable);
            KTInputManager.Instance.SelectRootDefault();

            _roomSelectable.Init();
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
                SetSelectableBomb(_currentBomb);
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
