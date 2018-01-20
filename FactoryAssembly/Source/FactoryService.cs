using UnityEngine;

namespace FactoryAssembly
{
    public class FactoryService : MonoBehaviour
    {
        private KMGameInfo _gameInfo = null;

        private void Awake()
        {
            _gameInfo = GetComponent<KMGameInfo>();
            _gameInfo.OnStateChange += OnStateChange;
        }

        private void OnDestroy()
        {
            _gameInfo.OnStateChange -= OnStateChange;
        }

        private void OnStateChange(KMGameInfo.State state)
        {
            switch (state)
            {
                case KMGameInfo.State.Setup:
                    FactoryGameModePicker.UpdateCompatibleMissions();
                    break;

                case KMGameInfo.State.Gameplay:
                    break;

                default:
                    break;
            }
        }
    }
}
