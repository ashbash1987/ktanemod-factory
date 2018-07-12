using Assets.Scripts.Missions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FactoryAssembly
{
    public class FactoryRoom : MonoBehaviour
    {
        internal Selectable RoomSelectable
        {
            get;
            private set;
        }

        internal Transform InitialSpawn
        {
            get
            {
                return _data.InitialSpawn;
            }
        }

        internal Transform VanillaBombSpawn
        {
            get
            {
                return _data.VanillaBombSpawn;
            }
        }

        internal FactoryGameMode GameMode
        {
            get;
            private set;
        }

        private FactoryRoomData _data = null;

        private Animator _conveyorBeltAnimator = null;
        private KMGameplayRoom _room = null;
        private KMAudio _audio = null;
        private KMGameCommands _gameCommands = null;

        private int _nextBeltNodeIndex = 0;

        private bool _initialSwitchOn = true;
        private bool _lightsOn = false;

        #region Unity Lifecycle
        /// <summary>
        /// Unity event.
        /// </summary>
        private void Awake()
        {
            _data = GetComponent<FactoryRoomData>();

            _conveyorBeltAnimator = GetComponent<Animator>();
            _room = GetComponent<KMGameplayRoom>();
            _audio = GetComponent<KMAudio>();
            _gameCommands = GetComponent<KMGameCommands>();

            _room.OnLightChange += OnLightChange;
        }

        /// <summary>
        /// Unity event.
        /// </summary>
        private void Start()
        {
            GameplayState gameplayState = SceneManager.Instance.GameplayState;
            RoomSelectable = gameplayState.Room.GetComponent<Selectable>();

            InvoiceData.ClearData();

            Mission mission = null;
            if (GameplayState.MissionToLoad == ModMission.CUSTOM_MISSION_ID)
            {
                mission = GameplayState.CustomMission;
            }
            else
            {
                mission = MissionManager.Instance.GetMission(GameplayState.MissionToLoad);
            }
            
            InvoiceData.MissionName = mission?.DisplayName;

            if (string.IsNullOrEmpty(InvoiceData.MissionName))
            {
                InvoiceData.MissionName = "Free Play";
            }

            FactoryGameModePicker.GameMode gameModeEnum;
            GameMode = FactoryGameModePicker.CreateGameMode(GameplayState.MissionToLoad, gameObject, out gameModeEnum);
            InvoiceData.GameMode = gameModeEnum;

            QuickDelay(() => GameMode.Setup(this));

            OnLightChange(false);
        }

        /// <summary>
        /// Unity event.
        /// </summary>
        private void Update()
        {
            GameMode?.Update();

            UpdateLighting();
        }
        #endregion

        #region Public (Internal) Methods
        internal Transform GetNextConveyorNode()
        {
            Transform nextNode = _data.ConveyorBeltNodes[_nextBeltNodeIndex];
            _nextBeltNodeIndex = (_nextBeltNodeIndex + 1) % _data.ConveyorBeltNodes.Length;

            return nextNode;
        }

        internal void GetNextBomb()
        {
            _conveyorBeltAnimator.SetTrigger("NextBomb");
            _audio.PlaySoundAtTransform(_data.ConveyorAudio.name, _data.ConveyorTop);
        }

        internal Bomb CreateBombWithCurrentMission(int bombIndex)
        {
            return CreateBomb(GameplayState.MissionToLoad, bombIndex);
        }

        internal Bomb CreateBomb(string missionID, int bombIndex)
        {
            //Protection in case MultipleBombs cannot be accessed
            if (MultipleBombsInterface.AccessVersion == MultipleBombsInterface.AccessAPIVersion.None)
            {
                return null;
            }

            Logging.Log("Creating bomb {0}@{1}", missionID, bombIndex);

            //Need to 'undo' RoundStarted to prevent the game from auto-starting the next bomb
            bool roundStarted = SceneManager.Instance.GameplayState.RoundStarted;
            PropertyInfo roundStartedProperty = typeof(GameplayState).GetProperty("RoundStarted", BindingFlags.Public | BindingFlags.Instance);
            roundStartedProperty.SetValue(SceneManager.Instance.GameplayState, false, null);

            //Interact with MultipleBombs to generate us a bomb
            Bomb bomb = MultipleBombsInterface.CreateBomb(missionID, bombIndex, _data.VanillaBombSpawn);

            //Revert the RoundStarted value back to what it was
            roundStartedProperty.SetValue(SceneManager.Instance.GameplayState, roundStarted, null);

            //Still need to do this to ensure the bomb can be selected properly later on
            bomb.GetComponent<Selectable>().Parent = RoomSelectable;
            KTInputManager.Instance.RootSelectable = RoomSelectable;
            KTInputManager.Instance.SelectRootDefault();

            return bomb;
        }

        internal void DestroyBomb(Bomb bomb)
        {
            GameplayState gameplayState = SceneManager.Instance.GameplayState;
            List<Bomb> bombs = gameplayState.Bombs;

            //Shift the bomb to be away from the front of the list (bomb at the front of the list only get deactivated, rather than destroyed)
            if (bombs.Remove(bomb))
            {
                bombs.Add(bomb);
            }

            Logging.Log("Destroying bomb...");

            //Call the remove the bomb actual
            gameplayState.RemoveBomb(bomb);
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

        #region Lighting Methods
        private void UpdateLighting()
        {
            bool warningTime = GameMode != null ? (GameMode.RemainingTime < _data.WarningTime) : false;

            float lightIntensity = _lightsOn ? (warningTime ? _data.LightWarningIntensity : _data.LightOnIntensity) : _data.LightOffIntensity;
            Color ambientColor = _lightsOn ? (warningTime ? _data.AmbientWarningColor : _data.AmbientOnColor) : _data.AmbientOffColor;

            _data.WarningLight.gameObject.SetActive(warningTime);

            foreach (Light light in _data.NormalLights)
            {
                light.intensity = lightIntensity;
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.ambientIntensity = 0.0f;
            DynamicGI.UpdateEnvironment();
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
                    StartCoroutine(ChangeLightIntensity(KMSoundOverride.SoundEffect.Switch, 0.0f, on));
                    _initialSwitchOn = false;
                }
                else
                {
                    StartCoroutine(ChangeLightIntensity(null, 0.0f, on));
                }
            }
            else
            {
                if (on)
                {
                    StartCoroutine(ChangeLightIntensity(KMSoundOverride.SoundEffect.LightBuzzShort, 0.5f, on));
                }
                else
                {
                    StartCoroutine(ChangeLightIntensity(KMSoundOverride.SoundEffect.LightBuzz, 1.5f, on));
                }
            }
        }

        private IEnumerator ChangeLightIntensity(KMSoundOverride.SoundEffect? sound, float wait, bool lightState)
        {
            if (sound.HasValue)
            {
                _audio.PlayGameSoundAtTransform(sound.Value, transform);
            }

            if (wait > 0.0f)
            {
                yield return new WaitForSeconds(wait);
            }

            _lightsOn = lightState;            
        }
        #endregion

        #region Animation Methods
        /// <summary>
        /// Starts the 'current' bomb.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        private void StartBomb()
        {
            GameMode.OnStartBomb();
        }

        /// <summary>
        /// Ends the 'old' bomb.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        private void EndBomb()
        {
            GameMode.OnEndBomb();
        }

        /// <summary>
        /// The left door opens.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        private void DoorLeftOpen()
        {
            _audio.PlaySoundAtTransform(_data.DoorLongAudio.name, _data.LeftDoor);
        }

        /// <summary>
        /// The left door closes.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        private void DoorLeftClose()
        {
            _audio.PlaySoundAtTransform(_data.DoorShortAudio.name, _data.LeftDoor);
        }

        /// <summary>
        /// The right door opens.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        private void DoorRightOpen()
        {
            _audio.PlaySoundAtTransform(_data.DoorShortAudio.name, _data.RightDoor);
        }

        /// <summary>
        /// The right door closes.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        private void DoorRightClose()
        {
            _audio.PlaySoundAtTransform(_data.DoorLongAudio.name, _data.RightDoor);
        }
        #endregion
        #endregion
    }
}
