using System.Collections;
using UnityEngine;

namespace FactoryAssembly
{
    public class Bump : MonoBehaviour
    {
        public AudioClip RumbleAudio = null;

        public float MinimumBump = 0.0f;
        public float MaximumBump = 0.0f;

        public float MinimumPunchDuration = 0.4f;
        public float MaximumPunchDuration = 0.7f;

        public float MinimumPunchOscillationPeriod = 0.3f;
        public float MaximumPunchOscillationPeriod = 0.6f;

        private KMAudio _audio = null;
        private KMAudio.KMAudioRef _ref = null;

        private void Awake()
        {
            _audio = GetComponent<KMAudio>();
        }

        public void DoBump(float duration)
        {
            gameObject.SetActive(true);
            StartCoroutine(Bumps(duration));
        }

        private IEnumerator Bumps(float duration)
        {
            _ref = _audio.PlaySoundAtTransformWithRef(RumbleAudio.name, transform);

            while (duration >= 0.0f)
            {
                Vector3 punchPosition = new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(0.0f, 10.0f), Random.Range(-10.0f, 10.0f));
                float bumpAmount = Random.Range(MinimumBump, MaximumBump);
                float punchDuration = Random.Range(MinimumPunchDuration, MaximumPunchDuration);
                float punchOscillationPeriod = Random.Range(MinimumPunchOscillationPeriod, MaximumPunchOscillationPeriod);

                KTInputManager.Instance.AddInteractionPunch(punchPosition, Assets.Scripts.Input.AbstractHapticUtil.HapticType.Interaction, bumpAmount, punchDuration, punchOscillationPeriod);

                yield return new WaitForSeconds(0.1f);
                duration -= 0.1f;
            }

            _ref.StopSound();
            _ref = null;

            gameObject.SetActive(false);
        }
    }
}
