using System.Collections;
using UnityEngine;

namespace FactoryAssembly
{
    public class FakeNeedy : MonoBehaviour
    {
        private KMAudio _audio = null;
        private KMAudio.KMAudioRef _ref = null;

        private void Awake()
        {
            _audio = GetComponent<KMAudio>();
        }

        public void DoFakeNeedy()
        {
            gameObject.SetActive(true);

            StartCoroutine(PlayFakeNeedy());
        }

        private IEnumerator PlayFakeNeedy()
        {
            _ref = _audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.NeedyWarning, transform);

            yield return new WaitForSeconds(5.0f);

            _ref.StopSound();
            _ref = null;

            gameObject.SetActive(false);
        }
    }
}
