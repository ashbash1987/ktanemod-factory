using UnityEngine;

namespace FactoryAssembly
{
    public class FactoryService : MonoBehaviour
    {
        private KMGameInfo _gameInfo = null;
        private APIProperties _properties = null;
        private ResultBinderConverter _binderConverter = null;

        private static bool _fromSetupRoom = false;

        private void Awake()
        {
            _gameInfo = GetComponent<KMGameInfo>();

            _properties = GetComponentInChildren<APIProperties>();
            _properties.Add("SupportedModes", () => FactoryGameModePicker.GetModeNames, null);
            _properties.Add("EnabledModes", () => FactoryGameModePicker.GetModeSupport, null);

            _binderConverter = GetComponentInChildren<ResultBinderConverter>(true);
        }

        private void OnEnable()
        {
            InvoiceData.Enabled = false;
            InvoiceData.ClearData();

            _gameInfo.OnStateChange += OnStateChange;
        }

        private void OnDisable()
        {
            InvoiceData.Enabled = false;
            InvoiceData.ClearData();

            _gameInfo.OnStateChange -= OnStateChange;
        }

        private void OnStateChange(KMGameInfo.State state)
        {
            switch (state)
            {
                case KMGameInfo.State.Setup:
                    Logging.Log("State Change: Setup");

                    InvoiceData.Enabled = false;
                    InvoiceData.ClearData();

                    MultipleBombsInterface.RediscoverMultipleBombs();
                    FactoryGameModePicker.UpdateCompatibleMissions();
                    _fromSetupRoom = true;

                    _binderConverter.Revert();
                    break;

                case KMGameInfo.State.Gameplay:
                    Logging.Log("State Change: Gameplay");

                    if (GameplayState.MissionToLoad == ModMission.CUSTOM_MISSION_ID && _fromSetupRoom)
                    {
                        FactoryGameModePicker.UpdateMission(GameplayState.CustomMission, true, false, true);
                    }

                    _fromSetupRoom = false;

                    _binderConverter.Revert();
                    break;

                case KMGameInfo.State.PostGame:
                    Logging.Log("Stage Change: PostGame");

                    if (InvoiceData.Enabled)
                    {
                        _binderConverter.Convert();
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
