using System.Collections;
using UnityEngine;

namespace FactoryAssembly
{
    public class WalkieTalkie : MonoBehaviour
    {
        public AudioClip AllAvailableExperts = null;
        [Range(0.0f, 300.0f)]
        public float AllAvailableExpertsTime = 120.0f;

        public AudioClip Evacuate = null;
        [Range(0.0f, 300.0f)]
        public float EvacuateTime = 60.0f;

        public AudioClip EmergencyCleared = null;

        public AudioClip Over = null;

        private KMAudio _audio = null;
        private Coroutine _currentWalkieCoroutine = null;

        private FactoryGameMode _gameMode = null;
        private float _previousTime = float.PositiveInfinity;


        private void Awake()
        {
            _audio = GetComponent<KMAudio>();
        }

        private void Update()
        {
            if (_gameMode == null)
            {
                _gameMode = FindObjectOfType<FactoryGameMode>();
                return;
            }

            float currentTime = _gameMode.RemainingTime;

            if (currentTime < _previousTime)
            {
                if (ShouldTrigger(AllAvailableExpertsTime, currentTime, _previousTime))
                {
                    PlayWalkie(AllAvailableExperts);
                }
                else if (ShouldTrigger(EvacuateTime, currentTime, _previousTime))
                {
                    PlayWalkie(Evacuate);
                }
            }
            else
            {
                if (ShouldTrigger(AllAvailableExpertsTime, _previousTime, currentTime))
                {
                    PlayWalkie(EmergencyCleared);
                }
            }

            _previousTime = currentTime;
        }

        private bool ShouldTrigger(float triggerTime, float low, float high)
        {
            return triggerTime >= low && triggerTime <= high;
        }

        private void PlayWalkie(AudioClip clip)
        {
            if (_currentWalkieCoroutine != null)
            {
                return;
            }

            _currentWalkieCoroutine = StartCoroutine(PlayWalkieCoroutine(clip));
        }

        private IEnumerator PlayWalkieCoroutine(AudioClip clip)
        {
            _audio.PlaySoundAtTransform(clip.name, transform);
            yield return new WaitForSeconds(clip.length);

            _audio.PlaySoundAtTransform(Over.name, transform);
            yield return new WaitForSeconds(Over.length);

            _currentWalkieCoroutine = null;
        }
    }
}
