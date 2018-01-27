using System.Collections;
using UnityEngine;

namespace FactoryAssembly
{
    public class WarningLight : MonoBehaviour
    {
        [Range(0.0f, 360.0f)]
        public float RotationSpeed = 160.0f;

        [Range(0.0f, 5.0f)]
        public float AlarmSoundPeriod = 3.0f;

        private KMAudio _audio = null;
        private Coroutine _soundLoop = null;

        private void Awake()
        {
            _audio = GetComponent<KMAudio>();
        }

        private void Update()
        {
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.y += RotationSpeed * Time.deltaTime;
            transform.eulerAngles = eulerAngles;
        }

        private void OnEnable()
        {
            _soundLoop = StartCoroutine(WarningSound());
        }

        private void OnDisable()
        {
            if (_soundLoop != null)
            {
                StopCoroutine(_soundLoop);
                _soundLoop = null;
            }
        }

        private IEnumerator WarningSound()
        {
            while (true)
            {
                yield return null;
                _audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.EmergencyAlarm, transform);
                yield return new WaitForSeconds(AlarmSoundPeriod);
            }
        }
    }
}
