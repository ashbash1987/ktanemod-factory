using System.Collections;
using UnityEngine;

namespace FactoryAssembly
{
    public class GlitchDistort : MonoBehaviour
    {
        public AudioClip GlitchSound = null;
        public Material Material = null;

        private KMAudio _audio = null;
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

        public void DoDistort()
        {
            gameObject.SetActive(true);

            StartCoroutine(Distort());
        }

        private IEnumerator Distort()
        {
            _audio.PlaySoundAtTransform(GlitchSound.name, transform);
            _postProcess = CameraPostProcess.AddPostProcess(Material);

            yield return new WaitForSeconds(GlitchSound.length);

            DestroyImmediate(_postProcess);
            _postProcess = null;

            gameObject.SetActive(false);
        }
    }
}
