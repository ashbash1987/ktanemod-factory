using System;
using System.Collections;
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
        private static readonly string VANILLA_BOMB_SPAWN_NAME = "Spawns/BombSpawn";

        private static readonly Color AMBIENT_OFF_COLOR = new Color(0.01f, 0.01f, 0.01f);
        private static readonly Color AMBIENT_ON_COLOR = new Color(0.4f, 0.4f, 0.4f);

        private const float LIGHT_OFF_INTENSITY = 0.02f;
        private const float LIGHT_ON_INTENSITY = 0.8f;

        public Selectable RoomSelectable
        {
            get;
            private set;
        }

        public Transform InitialSpawn
        {
            get
            {
                return _conveyorBeltNodes[0];
            }
        }

        public Transform VanillaBombSpawn
        {
            get;
            private set;
        }

        private Animator _conveyorBeltAnimator = null;
        private Light[] _lights = null;
        private KMGameplayRoom _room = null;
        private KMAudio _audio = null;

        private Transform[] _conveyorBeltNodes = null;
        private int _nextBeltNodeIndex = 0;

        private Transform _leftDoor = null;
        private Transform _rightDoor = null;
        private Transform _conveyorTop = null;

        private bool _initialSwitchOn = true;

        private FactoryGameMode _gameMode = null;

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
            GameplayState gameplayState = SceneManager.Instance.GameplayState;
            RoomSelectable = gameplayState.Room.GetComponent<Selectable>();

            //Get the conveyor belt nodes
            _conveyorBeltNodes = new Transform[CONVEYOR_BELT_NODE_NAMES.Length];
            for (int nodeIndex = 0; nodeIndex < CONVEYOR_BELT_NODE_NAMES.Length; ++nodeIndex)
            {
                _conveyorBeltNodes[nodeIndex] = transform.Find(CONVEYOR_BELT_NODE_NAMES[nodeIndex]);
            }

            _leftDoor = transform.Find(LEFT_DOOR_NAME);
            _rightDoor = transform.Find(RIGHT_DOOR_NAME);
            _conveyorTop = transform.Find(CONVEYOR_TOP_NAME);

            VanillaBombSpawn = transform.Find(VANILLA_BOMB_SPAWN_NAME);

            //TODO: Determine gamemode
            _gameMode = gameObject.AddComponent<FiniteSequenceMode>();
            QuickDelay(() => _gameMode.Setup(this));

            OnLightChange(false);
        }
        #endregion

        #region Public Methods
        public Transform GetNextConveyorNode()
        {
            Transform nextNode = _conveyorBeltNodes[_nextBeltNodeIndex];
            _nextBeltNodeIndex = (_nextBeltNodeIndex + 1) % _conveyorBeltNodes.Length;

            return nextNode;
        }

        public void GetNextBomb()
        {
            _conveyorBeltAnimator.SetTrigger("NextBomb");
            _audio.PlaySoundAtTransform(CONVEYOR_AUDIO_NAME, _conveyorTop);
        }
        #endregion

        #region Private Methods
        private void QuickDelay(Action delayCallable)
        {
            StartCoroutine(QuickDelayCoroutine(delayCallable));
        }

        private IEnumerator QuickDelayCoroutine(Action delayCallable)
        {
            yield return null;
            yield return null;

            delayCallable();
        }

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
       
        #region Animation Methods
        /// <summary>
        /// Starts the 'current' bomb.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        private void StartBomb()
        {
            _gameMode.OnStartBomb();
        }

        /// <summary>
        /// Ends the 'old' bomb.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        private void EndBomb()
        {
            _gameMode.OnEndBomb();
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
