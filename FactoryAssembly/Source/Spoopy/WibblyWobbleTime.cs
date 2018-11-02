using System.Collections;
using UnityEngine;

namespace FactoryAssembly
{
    public class WibblyWobblyTime : MonoBehaviour
    {
        public AudioClip WobbleAudio = null;
        public Material Material = null;

        private KMAudio _audio = null;
        private KMAudio.KMAudioRef _ref = null;
        private CameraPostProcess _postProcess = null;

        private void Awake()
        {
            _audio = GetComponent<KMAudio>();
        }

        private void OnDestroy()
        {
            if (_postProcess != null)
            {
                DestroyImmediate(_postProcess);
            }
        }

        public void DoWibblyWobblyTime(float duration)
        {
            gameObject.SetActive(true);

            StartCoroutine(WobbleTime(duration));
        }

        private IEnumerator WobbleTime(float duration)
        {
            _postProcess = CameraPostProcess.AddPostProcess(Material);

            _ref = _audio.PlaySoundAtTransformWithRef(WobbleAudio.name, transform);

            float finalTime = Random.Range(0, 2) == 0 ? Random.Range(0.4f, 0.6f) : Random.Range(1.2f, 1.3f);
            float sign = Mathf.Sign(finalTime - 1.0f);

            float startTime = Time.realtimeSinceStartup;
            while ((Time.realtimeSinceStartup - startTime) < (duration * 0.8f))
            {
                float delta = (Time.realtimeSinceStartup - startTime) / (duration * 0.9f);

                Time.timeScale = Mathf.SmoothStep(1.0f, finalTime, delta);
                Material.SetFloat("_Stretch", Mathf.Lerp(1.0f, 1.0f - sign, delta));
                Material.SetFloat("_Vignette", Mathf.Lerp(0.0f, sign * -2.0f, delta));

                yield return null;
            }

            yield return new WaitForSeconds(duration * 0.1f);

            startTime = Time.realtimeSinceStartup;
            while ((Time.realtimeSinceStartup - startTime) < (duration * 0.1f))
            {
                float delta = (Time.realtimeSinceStartup - startTime) / (duration * 0.1f);

                Time.timeScale = Mathf.SmoothStep(finalTime, 1.0f, delta);
                Material.SetFloat("_Stretch", Mathf.Lerp(1.0f - sign, 1.0f, delta));
                Material.SetFloat("_Vignette", Mathf.Lerp(sign * -2.0f, 0.0f, delta));

                yield return null;
            }

            Time.timeScale = 1.0f;

            _ref.StopSound();
            _ref = null;

            DestroyImmediate(_postProcess);
            _postProcess = null;

            gameObject.SetActive(false);
        }
    }
}
